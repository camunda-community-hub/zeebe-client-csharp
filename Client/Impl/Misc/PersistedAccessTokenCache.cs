using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Zeebe.Client.Api.Builder;

namespace Zeebe.Client.Impl.Misc;

public class PersistedAccessTokenCache : IAccessTokenCache
{
    private static string ZeebeTokenFileName => "credentials";
    private Dictionary<string, AccessToken> CachedCredentials { get; set; }

    // private static readonly string ZeebeRootPath =
    //     Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zeebe");

    private readonly ILogger<PersistedAccessTokenCache> logger;
    private readonly IAccessTokenCache.AccessTokenResolverAsync accessTokenFetcherAsync;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> tokenLocks = new ();
    private readonly SemaphoreSlim semaphore;

    private readonly string tokenStoragePath;
    private string TokenFileName => Path.Combine(tokenStoragePath, ZeebeTokenFileName);

    public PersistedAccessTokenCache(string path, IAccessTokenCache.AccessTokenResolverAsync fetcherAsync,
        ILogger<PersistedAccessTokenCache> logger = null)
    {
        var directoryInfo = Directory.CreateDirectory(path);
        if (!directoryInfo.Exists)
        {
            throw new IOException("Expected to create '~/.zeebe/' directory, but failed to do so.");
        }

        tokenStoragePath = path;
        this.logger = logger;
        accessTokenFetcherAsync = fetcherAsync;
        CachedCredentials = new Dictionary<string, AccessToken>();
        semaphore = tokenLocks.GetOrAdd("credentials_lock", new SemaphoreSlim(1, 1));
    }

    public async Task<string> Get(string audience)
    {
        // check in memory
        if (CachedCredentials.TryGetValue(audience, out var currentAccessToken))
        {
            logger?.LogTrace("Use in memory access token");
            return await GetValidToken(audience, currentAccessToken);
        }

        // check if token file exists
        var useCachedFileToken = File.Exists(TokenFileName);
        if (useCachedFileToken)
        {
            logger?.LogTrace("Read cached access token from {TokenFileName}", TokenFileName);
            // read token
            var content = await File.ReadAllTextAsync(TokenFileName);
            CachedCredentials = JsonConvert.DeserializeObject<Dictionary<string, AccessToken>>(content);
            if (CachedCredentials.TryGetValue(audience, out currentAccessToken))
            {
                logger?.LogTrace("Found access token in credentials file");
                return await GetValidToken(audience, currentAccessToken);
            }
        }

        if (semaphore.CurrentCount == 0)
        {
            logger?.LogTrace("Semaphore is locked, but there is no token in memory or file");
            return null;
        }

        // fetch new token
        var newAccessToken = await FetchNewAccessToken(audience);
        return newAccessToken.Token;
    }

    private async Task<string> GetValidToken(string audience, AccessToken currentAccessToken)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var dueDate = currentAccessToken.DueDate;

        if (now < dueDate - 15000) // Adding 15 sec buffer to update the token before it really expires.
        {
            // still valid
            return currentAccessToken.Token;
        }

        // token is still valid, but will expire soon
        if (now < dueDate)
        {
            if (semaphore.CurrentCount == 0)
            {
                Console.WriteLine("Semaphore is locked, using existing token...");
                return currentAccessToken.Token;
            }

            var updatedAccessToken = await FetchNewAccessToken(audience);
            return updatedAccessToken.Token;
        }

        logger?.LogTrace("Access token is no longer valid (now: {Now} > dueTime: {DueTime}), request new one", now,
            dueDate);
        var newAccessToken = await FetchNewAccessToken(audience);
        return newAccessToken.Token;
    }

    private async Task<AccessToken> FetchNewAccessToken(string audience)
    {
        var newAccessToken = await accessTokenFetcherAsync();
        CachedCredentials[audience] = newAccessToken;

        await semaphore.WaitAsync();
        try
        {
            WriteCredentials();
        }
        finally
        {
            semaphore.Release();
        }

        return newAccessToken;
    }

    private void WriteCredentials()
    {
        File.WriteAllText(TokenFileName, JsonConvert.SerializeObject(CachedCredentials));
    }
}