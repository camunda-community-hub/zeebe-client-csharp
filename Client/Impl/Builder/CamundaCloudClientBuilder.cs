using System;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Builder;
using Zeebe.Client.Impl.Builder;

namespace Zeebe.Client.Impl.Builder
{
    public class CamundaCloudClientBuilder : ICamundaCloudClientBuilder, ICamundaCloudClientBuilderStep1, ICamundaCloudClientBuilderStep2, ICamundaCloudClientBuilderFinalStep
    {
        private readonly CamundaCloudTokenProviderBuilder camundaCloudTokenProviderBuilder;
        private string gatewayAddress;
        private ILoggerFactory loggerFactory;

        private CamundaCloudClientBuilder()
        {
            camundaCloudTokenProviderBuilder = CamundaCloudTokenProvider.Builder();
        }

        public static ICamundaCloudClientBuilder Builder()
        {
            return new CamundaCloudClientBuilder();
        }

        public ICamundaCloudClientBuilderStep1 UseClientId(string clientId)
        {
            camundaCloudTokenProviderBuilder.UseClientId(clientId);
            return this;
        }

        public ICamundaCloudClientBuilderStep2 UseClientSecret(string clientSecret)
        {
            camundaCloudTokenProviderBuilder.UseClientSecret(clientSecret);
            return this;
        }

        public ICamundaCloudClientBuilderFinalStep UseContactPoint(string contactPoint)
        {
            _ = contactPoint ?? throw new ArgumentNullException(nameof(contactPoint));

            if (!contactPoint.EndsWith(":443"))
            {
                gatewayAddress = contactPoint + ":443";
                camundaCloudTokenProviderBuilder.UseAudience(contactPoint);
            }
            else
            {
                gatewayAddress = contactPoint;
                camundaCloudTokenProviderBuilder.UseAudience(contactPoint.Replace(":443", ""));
            }

            return this;
        }

        public ICamundaCloudClientBuilderFinalStep UseLoggerFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            camundaCloudTokenProviderBuilder.UseLoggerFactory(this.loggerFactory);
            return this;
        }

        public ICamundaCloudClientBuilderFinalStep UseAuthServer(string url)
        {
            camundaCloudTokenProviderBuilder.UseAuthServer(url);
            return this;
        }

        public IZeebeClient Build()
        {
            return ZeebeClient.Builder()
                .UseLoggerFactory(loggerFactory)
                .UseGatewayAddress(gatewayAddress)
                .UseTransportEncryption()
                .UseAccessTokenSupplier(camundaCloudTokenProviderBuilder.Build())
                .Build();
        }
    }
}