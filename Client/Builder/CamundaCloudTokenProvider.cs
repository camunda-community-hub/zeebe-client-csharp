using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Zeebe.Client.Builder
{
    public class CamundaCloudTokenProvider : IAccessTokenSupplier
    {
        private const string JsonContent =
            "{{\"client_id\":\"{0}\",\"client_secret\":\"{1}\",\"audience\":\"{2}\",\"grant_type\":\"client_credentials\"}}";

        private const string ZeebeCloudTokenFileName = "cloud.token";

        private static readonly string ZeebeRootPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zeebe");

        public HttpMessageHandler HttpMessageHandler { get; set; }
        public string TokenStoragePath { get; set; }
        private string TokenFileName => TokenStoragePath + Path.DirectorySeparatorChar + ZeebeCloudTokenFileName;
        private AccessToken CurrentAccessToken { get; set; }

        private readonly ILogger<CamundaCloudTokenProvider> logger;
        private readonly string authServer;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string audience;

        public static CamundaCloudTokenProviderBuilder Builder()
        {
            return new CamundaCloudTokenProviderBuilder();
        }

        internal CamundaCloudTokenProvider(string authServer, string clientId, string clientSecret, string audience,
            ILogger<CamundaCloudTokenProvider> logger = null)
        {
            this.logger = logger;
            this.authServer = authServer;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.audience = audience;

            // default client handler
            HttpMessageHandler = new HttpClientHandler();
            TokenStoragePath = ZeebeRootPath;
        }

        public Task<string> GetAccessTokenForRequestAsync(
            string authUri = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // check in memory
            if (CurrentAccessToken != null)
            {
                logger?.LogTrace("Use in memory access token.");
                return GetValidToken(CurrentAccessToken);
            }

            // check if token file exists
            var tokenFileName = TokenFileName;
            var existToken = File.Exists(tokenFileName);
            if (existToken)
            {
                logger?.LogTrace("Read cached access token from {tokenFileName}", tokenFileName);
                // read token
                var content = File.ReadAllText(tokenFileName);
                var accessToken = JsonConvert.DeserializeObject<AccessToken>(content);
                CurrentAccessToken = accessToken;
                return GetValidToken(accessToken);
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
            using (var httpClient = new HttpClient(HttpMessageHandler))
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                var httpResponseMessage = await httpClient.PostAsync(authServer, content);

                var result = await httpResponseMessage.Content.ReadAsStringAsync();
                var token = ToAccessToken(result);
                logger?.LogDebug("Received access token {token}, will backup at {path}.", token, tokenFileName);
                File.WriteAllText(tokenFileName, JsonConvert.SerializeObject(token));
                CurrentAccessToken = token;

                return token.Token;
            }
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
    }
}