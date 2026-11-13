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
        private string audience;
        private string authServer = "https://login.cloud.camunda.io/oauth/token";
        private string clientId;
        private string clientSecret;
        private ILoggerFactory loggerFactory;
        private string path;
        private bool persistedCredentialsCacheEnabled = true;
        private TimeSpan accessTokenDueDateTolerance = TimeSpan.Zero;

        /// <inheritdoc />
        public ICamundaCloudTokenProviderBuilder UseLoggerFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            return this;
        }

        /// <inheritdoc />
        public ICamundaCloudTokenProviderBuilderStep2 UseAuthServer(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            authServer = url;
            return this;
        }

        public ICamundaCloudTokenProviderBuilderFinalStep UsePath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.path = path;
            return this;
        }

        /// <inheritdoc />
        public ICamundaCloudTokenProviderBuilderFinalStep DisableCredentialsCachePersistence()
        {
            this.persistedCredentialsCacheEnabled = false;
            return this;
        }

        /// <inheritdoc />
        public ICamundaCloudTokenProviderBuilderFinalStep UseAccessTokenDueDateTolerance(TimeSpan tolerance)
        {
            if (tolerance < TimeSpan.Zero)
            {
                throw new ArgumentException("AccessToken due date tolerance must be a positive time span", nameof(tolerance));
            }

            this.accessTokenDueDateTolerance = tolerance;
            return this;
        }

        /// <inheritdoc />
        public CamundaCloudTokenProvider Build()
        {
            return new CamundaCloudTokenProvider(
                authServer,
                clientId,
                clientSecret,
                audience,
                path,
                loggerFactory,
                persistedCredentialsCacheEnabled,
                accessTokenDueDateTolerance);
        }

        /// <inheritdoc />
        public ICamundaCloudTokenProviderBuilderStep3 UseClientId(string clientId)
        {
            if (clientId == null)
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            this.clientId = clientId;
            return this;
        }

        /// <inheritdoc />
        public ICamundaCloudTokenProviderBuilderStep4 UseClientSecret(string clientSecret)
        {
            if (clientSecret == null)
            {
                throw new ArgumentNullException(nameof(clientSecret));
            }

            this.clientSecret = clientSecret;
            return this;
        }

        /// <inheritdoc />
        public ICamundaCloudTokenProviderBuilderFinalStep UseAudience(string audience)
        {
            if (audience == null)
            {
                throw new ArgumentNullException(nameof(audience));
            }

            this.audience = audience;
            return this;
        }
    }
}