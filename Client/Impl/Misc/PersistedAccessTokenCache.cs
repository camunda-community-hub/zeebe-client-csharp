using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Zeebe.Client.Impl.Misc;

public class PersistedAccessTokenCache : IAccessTokenCache
{
    private readonly IAccessTokenCache.AccessTokenResolverAsync accessTokenFetcherAsync;

    // private static readonly string ZeebeRootPath =
    //     Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zeebe");

    private readonly ILogger<PersistedAccessTokenCache> logger;

    private readonly string tokenStoragePath;

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
    }

    private static string ZeebeTokenFileName => "credentials";
    private Dictionary<string, AccessToken> CachedCredentials { get; set; }
    private string TokenFileName => Path.Combine(tokenStoragePath, ZeebeTokenFileName);

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

        // fetch new token
        var newAccessToken = await FetchNewAccessToken(audience);
        return newAccessToken.Token;
    }

    private async Task<string> GetValidToken(string audience, AccessToken currentAccessToken)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var dueDate = currentAccessToken.DueDate;
        if (now < dueDate)
        {
            // still valid
            return currentAccessToken.Token;
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
        WriteCredentials();
        return newAccessToken;
    }

    private void WriteCredentials()
    {
        File.WriteAllText(TokenFileName, JsonConvert.SerializeObject(CachedCredentials));
    }
}