using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Builder;
using Zeebe.Client.Impl.Misc;

namespace Zeebe.Client.Impl.Builder;

public class CamundaCloudTokenProvider : IAccessTokenSupplier, IDisposable
{
    private static readonly string ZeebeRootPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zeebe");

    private readonly string audience;
    private readonly string authServer;
    private readonly string clientId;
    private readonly string clientSecret;

    private readonly ILogger<CamundaCloudTokenProvider> logger;
    private readonly PersistedAccessTokenCache persistedAccessTokenCache;
    private HttpClient httpClient;
    private HttpMessageHandler httpMessageHandler;

    internal CamundaCloudTokenProvider(
        string authServer,
        string clientId,
        string clientSecret,
        string audience,
        string path = null,
        ILoggerFactory loggerFactory = null)
    {
        persistedAccessTokenCache = new PersistedAccessTokenCache(path ?? ZeebeRootPath, FetchAccessToken,
            loggerFactory?.CreateLogger<PersistedAccessTokenCache>());
        logger = loggerFactory?.CreateLogger<CamundaCloudTokenProvider>();
        this.authServer = authServer;
        this.clientId = clientId;
        this.clientSecret = clientSecret;
        this.audience = audience;
        httpClient = new HttpClient(new HttpClientHandler(), false);
    }

    public async Task<string> GetAccessTokenForRequestAsync(string authUri = null,
        CancellationToken cancellationToken = default)
    {
        return await persistedAccessTokenCache.Get(audience);
    }

    public void Dispose()
    {
        httpClient.Dispose();
        httpMessageHandler.Dispose();
    }

    public static CamundaCloudTokenProviderBuilder Builder()
    {
        return new CamundaCloudTokenProviderBuilder();
    }

    internal void SetHttpMessageHandler(HttpMessageHandler handler)
    {
        httpMessageHandler = handler;
        httpClient = new HttpClient(handler);
    }

    private async Task<AccessToken> FetchAccessToken()
    {
        // Requesting the token is similar to this:
        // curl -X POST https://login.cloud.ultrawombat.com/oauth/token \
        //   -H "Content-Type: application/x-www-form-urlencoded"  \
        //   -d "client_id=213131&client_secret=12-23~oU.321&audience=zeebe.ultrawombat.com&grant_type=client_credentials"
        //
        // alternative is json
        //        curl --request POST \
        //        --url https://login.cloud.[ultrawombat.com | camunda.io]/oauth/token \
        //        --header 'content-type: application/json' \
        //        --data '{"client_id":"${clientId}","client_secret":"${clientSecret}","audience":"${audience}","grant_type":"client_credentials"}'

        var formContent = BuildRequestAccessTokenContent();
        var httpResponseMessage = await httpClient.PostAsync(authServer, formContent);

        // Code expects the following result:
        //
        //        {
        //            "access_token":"MTQ0NjJkZmQ5OTM2NDE1ZTZjNGZmZjI3",
        //            "token_type":"bearer",
        //            "expires_in":3600,
        //            "refresh_token":"IwOGYzYTlmM2YxOTQ5MGE3YmNmMDFkNTVk",
        //            "scope":"create"
        //        }
        //
        // Defined here https://www.oauth.com/oauth2-servers/access-tokens/access-token-response/
        var result = await httpResponseMessage.Content.ReadAsStringAsync();
        var token = AccessToken.FromJson(result);
        logger?.LogDebug("Received access token for {Audience}", audience);
        return token;
    }

    private FormUrlEncodedContent BuildRequestAccessTokenContent()
    {
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("audience", audience),
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });
        return formContent;
    }
}