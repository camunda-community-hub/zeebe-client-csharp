using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Grpc.Auth;
using Grpc.Core;

namespace Zeebe.Client.Builder
{
        internal class ZeebeClientBuilder : IZeebeClientBuilder, IZeebeClientTransportBuilder
        {
            private string GatewayAddress { get; set; }

            public IZeebeClientTransportBuilder UseGatewayAddress(string gatewayAddress)
            {
                GatewayAddress = gatewayAddress;
                return this;
            }

            public IZeebeSecureClientBuilder UseTransportEncryption(string rootCertificatePath)
            {
                return new ZeebeSecureClientBuilder(GatewayAddress, rootCertificatePath);
            }

            public IZeebeSecureClientBuilder UseTransportEncryption()
            {
                return new ZeebeSecureClientBuilder(GatewayAddress);
            }

            public IZeebeClientFinalBuildStep UsePlainText()
            {
                return new ZeebePlainClientBuilder(GatewayAddress);
            }
        }

        internal class ZeebePlainClientBuilder : IZeebeClientFinalBuildStep
        {
            private string Address { get; }

            public ZeebePlainClientBuilder(string address)
            {
                Address = address;
            }

            public IZeebeClient Build()
            {
                return new ZeebeClient(Address);
            }
        }

        internal class ZeebeSecureClientBuilder : IZeebeSecureClientBuilder
        {
            private string Address { get; }

            private ChannelCredentials Credentials { get; set; }


            public ZeebeSecureClientBuilder(string address, string certificatePath)
            {
                Address = address;
                Credentials = new SslCredentials(File.ReadAllText(certificatePath));
            }

            public ZeebeSecureClientBuilder(string address)
            {
                Address = address;
                Credentials = new SslCredentials();
            }

            public IZeebeClientFinalBuildStep UseAccessToken(string accessToken)
            {

                Credentials = ChannelCredentials.Create(Credentials, GoogleGrpcCredentials.FromAccessToken(accessToken));
                return this;
            }

            public IZeebeClientFinalBuildStep UseAccessTokenSupplier(IAccessTokenSupplier supplier)
            {
                Credentials = ChannelCredentials.Create(Credentials, supplier.ToCallCredentials());
                return this;
            }

            public IZeebeClient Build()
            {
                return new ZeebeClient(Address, Credentials);
            }
        }
}