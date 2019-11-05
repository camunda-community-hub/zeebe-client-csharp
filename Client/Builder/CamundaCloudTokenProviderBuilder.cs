using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Zeebe.Client.Builder
{
    public class CamundaCloudTokenProviderBuilder
    {
        private ILoggerFactory loggerFactory;

        public CamundaCloudTokenProviderBuilder UseLoggerFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            return this;
        }

        public CamundaCloudTokenProviderBuilderStep2 UseAuthServer(string url)
        {
            return new CamundaCloudTokenProviderBuilderStep2(loggerFactory, url);
        }
    }

    public class CamundaCloudTokenProviderBuilderStep2
    {
        private readonly ILoggerFactory loggerFactory;

        private string AuthServer { get; }

        internal CamundaCloudTokenProviderBuilderStep2(ILoggerFactory loggerFactory, string authServer)
        {
            this.loggerFactory = loggerFactory;
            AuthServer = authServer;
        }

        public CamundaCloudTokenProviderBuilderStep3 UseClientId(string clientId)
        {
            return new CamundaCloudTokenProviderBuilderStep3(loggerFactory, AuthServer, clientId);
        }
    }

    public class CamundaCloudTokenProviderBuilderStep3
    {
        private readonly ILoggerFactory loggerFactory;

        private string AuthServer { get; }
        private string ClientId { get; }

        internal CamundaCloudTokenProviderBuilderStep3(ILoggerFactory loggerFactory, string authServer, string clientId)
        {
            this.loggerFactory = loggerFactory;
            AuthServer = authServer;
            ClientId = clientId;
        }

        public CamundaCloudTokenProviderBuilderStep4 UseClientSecret(string clientSecret)
        {
            return new CamundaCloudTokenProviderBuilderStep4(loggerFactory, AuthServer, ClientId, clientSecret);
        }
    }

    public class CamundaCloudTokenProviderBuilderStep4
    {
        private readonly ILoggerFactory loggerFactory;

        private string AuthServer { get; }
        private string ClientId { get; }
        private string ClientSecret { get; }

        internal CamundaCloudTokenProviderBuilderStep4(ILoggerFactory loggerFactory, string authServer, string clientId, string clientSecret)
        {
            this.loggerFactory = loggerFactory;
            AuthServer = authServer;
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        public CamundaCloudTokenProviderBuilderStep4(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public CamundaCloudTokenProviderBuilderFinalStep UseAudience(string audience)
        {
            return new CamundaCloudTokenProviderBuilderFinalStep(loggerFactory, AuthServer, ClientId, ClientSecret, audience);
        }
    }

    public class CamundaCloudTokenProviderBuilderFinalStep
    {
        private readonly ILoggerFactory loggerFactory;

        private string Audience { get; set; }
        private string AuthServer { get; }
        private string ClientId { get; }
        private string ClientSecret { get; }

        internal CamundaCloudTokenProviderBuilderFinalStep(ILoggerFactory loggerFactory, string authServer, string clientId, string clientSecret,
            string audience)
        {
            this.loggerFactory = loggerFactory;
            AuthServer = authServer;
            ClientId = clientId;
            ClientSecret = clientSecret;
            Audience = audience;
        }

        public CamundaCloudTokenProvider Build()
        {
            return new CamundaCloudTokenProvider(AuthServer, ClientId, ClientSecret, Audience, loggerFactory.CreateLogger<CamundaCloudTokenProvider>());
        }
    }
}
