using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Impl.Misc;

namespace Zeebe.Client.Impl.Builder
{
    public class OAuth2TokenProvider : BaseTokenProvider, IDisposable
    {
        private readonly ILogger<CamundaCloudTokenProvider> logger;
        private readonly string authServer;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string audience;
        private HttpClient httpClient;
        private HttpMessageHandler httpMessageHandler;

        public OAuth2TokenProvider(
            string authServer,
            string clientId,
            string clientSecret,
            string audience,
            ILogger<CamundaCloudTokenProvider> logger = null) : base("oauth2_credentials", audience)
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

        protected override async Task<string> RequestAccessTokenAsync()
        {
            var directoryInfo = Directory.CreateDirectory(TokenStoragePath);
            if (!directoryInfo.Exists)
            {
                throw new IOException("Expected to create '~/.zeebe/' directory, but failed to do so.");
            }

            var formContent = BuildRequestAccessTokenContent();

            var httpResponseMessage = await httpClient.PostAsync(authServer, formContent);

            var result = await httpResponseMessage.Content.ReadAsStringAsync();
            var token = AccessToken.FromJson(result);
            logger?.LogDebug("Received access token for {audience}", audience);
            CachedCredentials[audience] = token;
            WriteCredentials();

            return token.Token;
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

        public void Dispose()
        {
            httpClient.Dispose();
            httpMessageHandler.Dispose();
        }
    }
}