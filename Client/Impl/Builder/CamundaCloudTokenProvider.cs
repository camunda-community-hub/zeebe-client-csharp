using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zeebe.Client.Api.Builder;

namespace Zeebe.Client.Impl.Builder
{
    public class CamundaCloudTokenProvider : IAccessTokenSupplier, IDisposable
    {
        private const string JsonContent =
            "{{\"client_id\":\"{0}\",\"client_secret\":\"{1}\",\"audience\":\"{2}\",\"grant_type\":\"client_credentials\"}}";

        private const string ZeebeCloudTokenFileName = "credentials";

        private static readonly string ZeebeRootPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zeebe");

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
            ILogger<CamundaCloudTokenProvider> logger = null)
        {
            this.logger = logger;
            this.authServer = authServer;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.audience = audience;

            // default client handler
            httpClient = new HttpClient(new HttpClientHandler(), disposeHandler: false);
            TokenStoragePath = ZeebeRootPath;
            Credentials = new Dictionary<string, AccessToken>();
        }

        public static CamundaCloudTokenProviderBuilder Builder()
        {
            return new CamundaCloudTokenProviderBuilder();
        }

        public string TokenStoragePath { get; set; }
        private string TokenFileName => TokenStoragePath + Path.DirectorySeparatorChar + ZeebeCloudTokenFileName;
        private Dictionary<string, AccessToken> Credentials { get; set; }

        public Task<string> GetAccessTokenForRequestAsync(
            string authUri = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // check in memory
            AccessToken currentAccessToken;
            if (Credentials.TryGetValue(audience, out currentAccessToken))
            {
                logger?.LogTrace("Use in memory access token.");
                return GetValidToken(currentAccessToken);
            }

            // check if token file exists
            var tokenFileName = TokenFileName;
            var existToken = File.Exists(tokenFileName);
            if (existToken)
            {
                logger?.LogTrace("Read cached access token from {tokenFileName}", tokenFileName);
                // read token
                var content = File.ReadAllText(tokenFileName);
                Credentials = JsonConvert.DeserializeObject<Dictionary<string, AccessToken>>(content);
                if (Credentials.TryGetValue(audience, out currentAccessToken))
                {
                    logger?.LogTrace("Found access token in credentials file.");
                    return GetValidToken(currentAccessToken);
                }
            }

            // request token
            return RequestAccessTokenAsync();
        }

        internal void SetHttpMessageHandler(HttpMessageHandler handler)
        {
            httpMessageHandler = handler;
            httpClient = new HttpClient(handler);
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

            logger?.LogTrace("Access token is no longer valid (now: {now} > dueTime: {dueTime}), request new one.", now, dueDate);
            return RequestAccessTokenAsync();
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

        private async Task<string> RequestAccessTokenAsync()
        {
            var directoryInfo = Directory.CreateDirectory(TokenStoragePath);
            if (!directoryInfo.Exists)
            {
                throw new IOException("Expected to create '~/.zeebe/' directory, but failed to do so.");
            }

            var tokenFileName = TokenFileName;
            var json = string.Format(JsonContent, clientId, clientSecret, audience);

            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                var httpResponseMessage = await httpClient.PostAsync(authServer, content);

                var result = await httpResponseMessage.Content.ReadAsStringAsync();
                var token = ToAccessToken(result);
                logger?.LogDebug("Received access token for {audience}, will backup at {path}.", audience, tokenFileName);
                Credentials[audience] = token;
                WriteCredentials();

                return token.Token;
            }
        }

        private void WriteCredentials()
        {
            File.WriteAllText(TokenFileName, JsonConvert.SerializeObject(Credentials));
        }

        private static AccessToken ToAccessToken(string result)
        {
            var jsonResult = JObject.Parse(result);
            var accessToken = (string)jsonResult["access_token"];

            var expiresInMilliSeconds = (long)jsonResult["expires_in"] * 1_000L;
            var dueDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + expiresInMilliSeconds;
            var token = new AccessToken(accessToken, dueDate);
            return token;
        }

        public class AccessToken
        {
            public string Token { get; set; }
            public long DueDate { get; set; }

            public AccessToken(string token, long dueDate)
            {
                Token = token;
                DueDate = dueDate;
            }

            public override string ToString()
            {
                return $"{nameof(Token)}: {Token}, {nameof(DueDate)}: {DueDate}";
            }
        }

        public void Dispose()
        {
            httpClient.Dispose();
            httpMessageHandler.Dispose();
        }
    }
}