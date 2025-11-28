using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Zeebe.Client.Impl.Misc;

public class PersistedAccessTokenCache : IAccessTokenCache
{
    private readonly IAccessTokenCache.AccessTokenResolverAsync accessTokenFetcherAsync;

    // private static readonly string ZeebeRootPath =
    //     Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zeebe");

    private readonly ILogger<PersistedAccessTokenCache> logger;
    private readonly string tokenStoragePath;
    private readonly bool persistedCredentialsCacheEnabled;
    private readonly SemaphoreSlim mutex = new SemaphoreSlim(1, 1);
    private readonly TimeSpan dueDateTolerance;

    public PersistedAccessTokenCache(
        string path,
        IAccessTokenCache.AccessTokenResolverAsync fetcherAsync,
        ILogger<PersistedAccessTokenCache> logger = null,
        bool persistedCredentialsCacheEnabled = true,
        TimeSpan dueDateTolerance = default)
    {
        if (persistedCredentialsCacheEnabled)
        {
            var directoryInfo = Directory.CreateDirectory(path);
            if (!directoryInfo.Exists)
            {
                throw new IOException("Expected to create '~/.zeebe/' directory, but failed to do so.");
            }
        }

        this.tokenStoragePath = path;
        this.persistedCredentialsCacheEnabled = persistedCredentialsCacheEnabled;
        this.logger = logger;
        this.accessTokenFetcherAsync = fetcherAsync;
        this.CachedCredentials = new Dictionary<string, AccessToken>();
        this.dueDateTolerance = dueDateTolerance;
    }

    private static string ZeebeTokenFileName => "credentials";

    private Dictionary<string, AccessToken> CachedCredentials { get; set; }

    private string TokenFileName => Path.Combine(this.tokenStoragePath, ZeebeTokenFileName);

    public async Task<string> Get(string audience)
    {
        // shortcut sync lock if in-memory token is valid (read-only)
        if (this.CachedCredentials.TryGetValue(audience, out var currentAccessToken)
            && this.IsValid(currentAccessToken))
        {
            this.logger?.LogTrace("Use in memory access token");
            return currentAccessToken.Token;
        }

        // Secure concurrent access to token cache
        // and prevent race condition when accessing the file system.
        await this.mutex.WaitAsync().ConfigureAwait(false);
        try
        {
            return await this.GetOrRefreshAccessToken(audience).ConfigureAwait(false);
        }
        finally
        {
            this.mutex.Release();
        }
    }

    private async Task<string> GetOrRefreshAccessToken(string audience)
    {
        // check in memory
        if (this.CachedCredentials.TryGetValue(audience, out var currentAccessToken))
        {
            this.logger?.LogTrace("Use in memory access token");
            return await this.GetValidToken(audience, currentAccessToken);
        }

        if (this.persistedCredentialsCacheEnabled)
        {
            // check if token file exists
            var useCachedFileToken = File.Exists(this.TokenFileName);
            if (useCachedFileToken)
            {
                this.logger?.LogTrace("Read cached access token from {TokenFileName}", this.TokenFileName);
                // read token
                var content = await File.ReadAllTextAsync(this.TokenFileName);
                this.CachedCredentials = JsonConvert.DeserializeObject<Dictionary<string, AccessToken>>(content);
                if (this.CachedCredentials.TryGetValue(audience, out currentAccessToken))
                {
                    logger?.LogTrace("Found access token in credentials file");
                    return await this.GetValidToken(audience, currentAccessToken);
                }
            }
        }

        // fetch new token
        var newAccessToken = await this.FetchNewAccessToken(audience);
        return newAccessToken.Token;
    }

    private bool IsValid(AccessToken accessToken)
    {
        // add {dueDateTolerance} to UTC now to compensate network latency
        // and prevent server rejection if due date is too close to now.
        // Meaning that token will be refreshed {dueDateTolerance} before expiration.
        var now = DateTimeOffset.UtcNow.Add(this.dueDateTolerance).ToUnixTimeMilliseconds();
        var dueDate = accessToken.DueDate;

        if (now < dueDate)
        {
            return true;
        }

        var tolerance = (this.dueDateTolerance == default) ? string.Empty : $"(-{this.dueDateTolerance})";
        this.logger?.LogTrace(
            "Access token is no longer valid (now: {Now} > dueTime{DueDateTolerance}: {DueTime}), request new one",
            now,
            tolerance,
            dueDate);

        return false;
    }

    private async Task<string> GetValidToken(string audience, AccessToken currentAccessToken)
    {
        if (this.IsValid(currentAccessToken))
        {
            // still valid
            return currentAccessToken.Token;
        }

        var newAccessToken = await this.FetchNewAccessToken(audience);
        return newAccessToken.Token;
    }

    private async Task<AccessToken> FetchNewAccessToken(string audience)
    {
        var newAccessToken = await this.accessTokenFetcherAsync();
        this.CachedCredentials[audience] = newAccessToken;

        if (this.persistedCredentialsCacheEnabled)
        {
            this.WriteCredentials();
        }

        return newAccessToken;
    }

    private void WriteCredentials()
    {
        File.WriteAllText(this.TokenFileName, JsonConvert.SerializeObject(this.CachedCredentials));
    }
}