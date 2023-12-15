using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Builder;
using Zeebe.Client.Impl.Misc;

namespace Zeebe.Client.Impl.Builder
{
    public class CamundaCloudTokenProvider : IAccessTokenSupplier, IDisposable
    {
        private static readonly string ZeebeRootPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zeebe");

        private const string JsonContent =
            "{{\"client_id\":\"{0}\",\"client_secret\":\"{1}\",\"audience\":\"{2}\",\"grant_type\":\"client_credentials\"}}";

        private readonly ILogger<CamundaCloudTokenProvider> logger;
        private readonly string authServer;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string audience;
        private HttpClient httpClient;
        private HttpMessageHandler httpMessageHandler;
        private readonly PersistedAccessTokenCache persistedAccessTokenCache;

        internal CamundaCloudTokenProvider(
            string authServer,
            string clientId,
            string clientSecret,
            string audience,
            string path = null,
            ILogger<CamundaCloudTokenProvider> logger = null)
        {
            persistedAccessTokenCache = new PersistedAccessTokenCache(path ?? ZeebeRootPath, FetchAccessToken);
            this.logger = logger;
            this.authServer = authServer;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.audience = audience;
            httpClient = new HttpClient(new HttpClientHandler(), disposeHandler: false);
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
            var json = string.Format(JsonContent, clientId, clientSecret, audience);

            // Requesting the token is similar to this:
            //        curl --request POST \
            //        --url https://login.cloud.[ultrawombat.com | camunda.io]/oauth/token \
            //        --header 'content-type: application/json' \
            //        --data '{"client_id":"${clientId}","client_secret":"${clientSecret}","audience":"${audience}","grant_type":"client_credentials"}'

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
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var httpResponseMessage = await httpClient.PostAsync(authServer, content);

            var result = await httpResponseMessage.Content.ReadAsStringAsync();
            var token = AccessToken.FromJson(result);
            logger?.LogDebug("Received access token for {Audience}", audience);
            return token;
        }

        public void Dispose()
        {
            httpClient.Dispose();
            httpMessageHandler.Dispose();
        }

        public async Task<string> GetAccessTokenForRequestAsync(string authUri = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await persistedAccessTokenCache.Get(audience);
        }
    }
}