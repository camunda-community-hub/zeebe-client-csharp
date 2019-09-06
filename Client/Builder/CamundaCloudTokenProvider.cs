using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Zeebe.Client.Builder
{
    public class CamundaCloudTokenProvider : IAccessTokenSupplier
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string JsonContent =
            "{{\"client_id\":\"{0}\",\"client_secret\":\"{1}\",\"audience\":\"{2}\",\"grant_type\":\"client_credentials\"}}";

        private static readonly string ZeebeRootPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zeebe");

        private const string ZeebeCloudTokenFileName = "cloud.token";

        public HttpMessageHandler HttpMessageHandler { get; set; }
        public string TokenStoragePath { get; set; }
        private string TokenFileName => TokenStoragePath + Path.DirectorySeparatorChar + ZeebeCloudTokenFileName;
        private AccessToken CurrentAccessToken { get; set; }

        private readonly string authServer;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string audience;

        internal CamundaCloudTokenProvider(string authServer, string clientId, string clientSecret, string audience)
        {
            this.authServer = authServer;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.audience = audience;

            // default client handler
            HttpMessageHandler = new HttpClientHandler();
            TokenStoragePath = ZeebeRootPath;
        }

        public Task<string> GetAccessTokenForRequestAsync(string authUri = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            // check in memory
            if (CurrentAccessToken != null)
            {
                Logger.Trace("Use in memory access token.");
                return GetValidToken(CurrentAccessToken);
            }

            // check if token file exists
            var tokenFileName = TokenFileName;
            var existToken = File.Exists(tokenFileName);
            if (existToken)
            {
                Logger.Trace("Read cached access token from {0}", tokenFileName);
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

            Logger.Debug("Access token is no longer valid (now: {0} > dueTime: {1}), request new one.", now, dueDate);
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
                Logger.Debug("Received access token {0}, will backup at {1}.", token, tokenFileName);
                File.WriteAllText(tokenFileName, JsonConvert.SerializeObject(token));
                CurrentAccessToken = token;

                return token.Token;
            }
        }

        public static CamundaCloudTokenProviderBuilder Builder()
        {
            return new CamundaCloudTokenProviderBuilder();
        }

        private static AccessToken ToAccessToken(string result)
        {
            var jsonResult = JObject.Parse(result);
            var accessToken = (string) jsonResult["access_token"];

            var expiresInMilliSeconds = (long) jsonResult["expires_in"] * 1_000L;
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

    public class CamundaCloudTokenProviderBuilder
    {
        public CamundaCloudTokenProviderBuilderStep2 UseAuthServer(string url)
        {
            return new CamundaCloudTokenProviderBuilderStep2(url);
        }
    }

    public class CamundaCloudTokenProviderBuilderStep2
    {
        private string AuthServer { get; }

        internal CamundaCloudTokenProviderBuilderStep2(string authServer)
        {
            AuthServer = authServer;
        }

        public CamundaCloudTokenProviderBuilderStep3 UseClientId(string clientId)
        {
            return new CamundaCloudTokenProviderBuilderStep3(AuthServer, clientId);
        }
    }


    public class CamundaCloudTokenProviderBuilderStep3
    {
        private string AuthServer { get; }
        private string ClientId { get; }

        internal CamundaCloudTokenProviderBuilderStep3(string authServer, string clientId)
        {
            AuthServer = authServer;
            ClientId = clientId;
        }

        public CamundaCloudTokenProviderBuilderStep4 UseClientSecret(string clientSecret)
        {
            return new CamundaCloudTokenProviderBuilderStep4(AuthServer, ClientId, clientSecret);
        }
    }

    public class CamundaCloudTokenProviderBuilderStep4
    {
        private string AuthServer { get; }
        private string ClientId { get; }
        private string ClientSecret { get; }

        internal CamundaCloudTokenProviderBuilderStep4(string authServer, string clientId, string clientSecret)
        {
            AuthServer = authServer;
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        public CamundaCloudTokenProviderBuilderFinalStep UseAudience(string audience)
        {
            return new CamundaCloudTokenProviderBuilderFinalStep(AuthServer, ClientId, ClientSecret, audience);
        }
    }

    public class CamundaCloudTokenProviderBuilderFinalStep
    {
        private string Audience { get; set; }
        private string AuthServer { get; }
        private string ClientId { get; }
        private string ClientSecret { get; }

        internal CamundaCloudTokenProviderBuilderFinalStep(string authServer, string clientId, string clientSecret,
            string audience)
        {
            AuthServer = authServer;
            ClientId = clientId;
            ClientSecret = clientSecret;
            Audience = audience;
        }

        public CamundaCloudTokenProvider Build()
        {
            return new CamundaCloudTokenProvider(AuthServer, ClientId, ClientSecret, Audience);
        }
    }
}