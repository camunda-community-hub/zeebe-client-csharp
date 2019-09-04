using System.IO;
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

            private string CertificatePath { get; }


            public ZeebeSecureClientBuilder(string address, string certificatePath)
            {
                Address = address;
                CertificatePath = certificatePath;
            }

            public IZeebeClient Build()
            {
                return new ZeebeClient(Address, new SslCredentials(File.ReadAllText(CertificatePath)));
            }
        }
}