using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Impl.Misc;

namespace Zeebe.Client.Impl.Builder
{
    public class CamundaCloudTokenProvider : BaseTokenProvider, IDisposable
    {
        private const string JsonContent =
            "{{\"client_id\":\"{0}\",\"client_secret\":\"{1}\",\"audience\":\"{2}\",\"grant_type\":\"client_credentials\"}}";

        private readonly ILogger<CamundaCloudTokenProvider> logger;
        private readonly string authServer;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string audience;
        private HttpClient httpClient;
        private HttpMessageHandler httpMessageHandler;

        internal CamundaCloudTokenProvider(
            string authServer,
            string clientId,
            string clientSecret,
            string audience,
            ILogger<CamundaCloudTokenProvider> logger = null) : base("credentials", audience)
        {
            this.logger = logger;
            this.authServer = authServer;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.audience = audience;

            // default client handler
            httpClient = new HttpClient(new HttpClientHandler(), disposeHandler: false);
            CachedCredentials = new Dictionary<string, AccessToken>();
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

        protected override async Task<string> RequestAccessTokenAsync()
        {
            var directoryInfo = Directory.CreateDirectory(TokenStoragePath);
            if (!directoryInfo.Exists)
            {
                throw new IOException("Expected to create '~/.zeebe/' directory, but failed to do so.");
            }

            var json = string.Format(JsonContent, clientId, clientSecret, audience);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var httpResponseMessage = await httpClient.PostAsync(authServer, content);

            var result = await httpResponseMessage.Content.ReadAsStringAsync();
            var token = AccessToken.FromJson(result);
            logger?.LogDebug("Received access token for {audience}", audience);
            CachedCredentials[audience] = token;
            WriteCredentials();

            return token.Token;
        }

        public void Dispose()
        {
            httpClient.Dispose();
            httpMessageHandler.Dispose();
        }
    }
}