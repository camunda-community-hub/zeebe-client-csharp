using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Zeebe.Client.Api.Builder;
using Zeebe.Client.Impl.Misc;

namespace Zeebe.Client.Impl.Builder;

public abstract class BaseTokenProvider : IAccessTokenSupplier
{
    protected Dictionary<string, AccessToken> CachedCredentials { get; set; }

    private static readonly string ZeebeRootPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zeebe");

    private readonly string zeebeTokenFileName;
    private readonly ILogger<BaseTokenProvider> logger;

    public string TokenStoragePath { get; set; } = ZeebeRootPath;
    public string Audience { get; set; }
    private string TokenFileName => TokenStoragePath + Path.DirectorySeparatorChar + zeebeTokenFileName;

    public BaseTokenProvider(string cachingTokenFileName, string audience, ILogger<BaseTokenProvider> logger = null)
    {
        zeebeTokenFileName = cachingTokenFileName;
        Audience = audience;
        this.logger = logger;
    }

    public Task<string> GetAccessTokenForRequestAsync(
        string authUri = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        // check in memory
        AccessToken currentAccessToken;
        if (CachedCredentials.TryGetValue(Audience, out currentAccessToken))
        {
            logger?.LogTrace("Use in memory access token.");
            return GetValidToken(currentAccessToken);
        }

        // check if token file exists
        var useCachedFileToken = File.Exists(TokenFileName);
        if (useCachedFileToken)
        {
            logger?.LogTrace("Read cached access token from {tokenFileName}", TokenFileName);
            // read token
            var content = File.ReadAllText(TokenFileName);
            CachedCredentials = JsonConvert.DeserializeObject<Dictionary<string, AccessToken>>(content);
            if (CachedCredentials.TryGetValue(Audience, out currentAccessToken))
            {
                logger?.LogTrace("Found access token in credentials file.");
                return GetValidToken(currentAccessToken);
            }
        }

        // request token
        return RequestAccessTokenAsync();
    }

    private Task<string> GetValidToken(AccessToken currentAccessToken)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var dueDate = currentAccessToken.DueDate;
        if (now < dueDate)
        {
            // still valid
            return Task.FromResult(currentAccessToken.Token);
        }

        logger?.LogTrace("Access token is no longer valid (now: {now} > dueTime: {dueTime}), request new one.", now,
            dueDate);
        return RequestAccessTokenAsync();
    }

    protected void WriteCredentials()
    {
        File.WriteAllText(TokenFileName, JsonConvert.SerializeObject(CachedCredentials));
    }

    protected abstract Task<string> RequestAccessTokenAsync();
}