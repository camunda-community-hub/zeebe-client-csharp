using System;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Builder;

// ReSharper disable All

namespace Zeebe.Client.Impl.Builder
{
    public class CamundaCloudTokenProviderBuilder :
        ICamundaCloudTokenProviderBuilder,
            ICamundaCloudTokenProviderBuilderStep2,
            ICamundaCloudTokenProviderBuilderStep3,
            ICamundaCloudTokenProviderBuilderStep4,
            ICamundaCloudTokenProviderBuilderFinalStep
    {
        private ILoggerFactory loggerFactory;
        private string audience;
        private string authServer = "https://login.cloud.camunda.io/oauth/token";
        private string clientId;
        private string clientSecret;

        /// <inheritdoc/>
        public ICamundaCloudTokenProviderBuilder UseLoggerFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            return this;
        }

        /// <inheritdoc/>
        public ICamundaCloudTokenProviderBuilderStep2 UseAuthServer(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            authServer = url;
            return this;
        }

        /// <inheritdoc/>
        public ICamundaCloudTokenProviderBuilderStep3 UseClientId(string clientId)
        {
            if (clientId == null)
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            this.clientId = clientId;
            return this;
        }

        /// <inheritdoc/>
        public ICamundaCloudTokenProviderBuilderStep4 UseClientSecret(string clientSecret)
        {
            if (clientSecret == null)
            {
                throw new ArgumentNullException(nameof(clientSecret));
            }

            this.clientSecret = clientSecret;
            return this;
        }

        /// <inheritdoc/>
        public ICamundaCloudTokenProviderBuilderFinalStep UseAudience(string audience)
        {
            if (audience == null)
            {
                throw new ArgumentNullException(nameof(audience));
            }

            this.audience = audience;
            return this;
        }

        /// <inheritdoc/>
        public CamundaCloudTokenProvider Build()
        {
            return new CamundaCloudTokenProvider(
                authServer,
                clientId,
                clientSecret,
                audience,
                loggerFactory?.CreateLogger<CamundaCloudTokenProvider>());
        }
    }
}
