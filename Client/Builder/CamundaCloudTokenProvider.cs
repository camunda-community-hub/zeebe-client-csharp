using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;

namespace Zeebe.Client.Builder
{
    public class CamundaCloudTokenProvider : IAccessTokenSupplier
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        // Request token similar to this:
        //        curl --request POST \
        //        --url https://login.cloud.[ultrawombat.com | camunda.io]/oauth/token \
        //        --header 'content-type: application/json' \
        //        --data '{"client_id":"${clientId}","client_secret":"${clientSecret}","audience":"${audience}","grant_type":"client_credentials"}'

        private const string JsonContent =
            "{{\"client_id\":\"{0}\",\"client_secret\":\"{1}\",\"audience\":\"{2}\",\"grant_type\":\"client_credentials\"}}";

        private static readonly string ZeebeRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zeebe");
        private static readonly string ZeebeCloudTokenPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zeebe",
            "cloud.token");

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
        }

        public async Task<string> GetAccessTokenForRequestAsync(string authUri = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            // check if file exists
            var existToken = File.Exists(ZeebeCloudTokenPath);
            if (existToken)
            {
                Logger.Info("Read cached access token from {0}", ZeebeCloudTokenPath);
                // read token
                return File.ReadAllText(ZeebeCloudTokenPath);
            }

            // get token
            return await RequestAccessTokenAsync(ZeebeCloudTokenPath);
        }

        private async Task<string> RequestAccessTokenAsync(string zeebeCloudTokenPath)
        {
            var directoryInfo = Directory.CreateDirectory(ZeebeRootPath);
            if (!directoryInfo.Exists)
            {
                throw new IOException("Expected to create '~/.zeebe/' directory, but failed to do so.");
            }

            using (var httpClient = new HttpClient())
            {
                var json = string.Format(JsonContent, clientId, clientSecret, audience);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var httpResponseMessage = await httpClient.PostAsync(authServer, content);

                var result = await httpResponseMessage.Content.ReadAsStringAsync();
                var jsonResult = JObject.Parse(result);
                var accessToken = (string) jsonResult["access_token"];

                Logger.Info("Received access token {0}, will backup at {1}.", accessToken, zeebeCloudTokenPath);
                File.WriteAllText(zeebeCloudTokenPath, accessToken);

                return accessToken;
            }
        }

        public static CamundaCloudTokenProviderBuilder Builder()
        {
            return new CamundaCloudTokenProviderBuilder();
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
        private string Audience { get; set; }
        private string AuthServer { get; }
        private string ClientId { get; }
        private string ClientSecret { get; }

        internal CamundaCloudTokenProviderBuilderStep4(string authServer, string clientId, string clientSecret)
        {
            AuthServer = authServer;
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        public CamundaCloudTokenProviderBuilderStep4 UseAudience(string audience)
        {
            Audience = audience;
            return this;
        }

        public CamundaCloudTokenProvider Build()
        {
            return new CamundaCloudTokenProvider(AuthServer, ClientId, ClientSecret, Audience);
        }
    }
}