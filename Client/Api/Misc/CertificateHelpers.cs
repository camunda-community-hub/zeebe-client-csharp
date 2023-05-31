using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Zeebe.Client.Api.Misc
{
    public static class CertificateHelpers
    {
        // https://learn.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/channel-credentials#load-a-client-certificate-from-certificate-and-private-key-pem-files
        // item: https://github.com/dotnet/runtime/issues/23749#issuecomment-388231655
        public static X509Certificate2 CreateFromPem(string certificatePemFile, string privateKeyPemFile)
        {
            var certificatePem = File.ReadAllText(certificatePemFile);
            var privateKeyPem = File.ReadAllText(privateKeyPemFile);
            var cert = X509Certificate2.CreateFromPem(certificatePem, privateKeyPem);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var originalCert = cert;
                cert = new X509Certificate2(cert.Export(X509ContentType.Pkcs12));
                originalCert.Dispose();
            }

            return cert;
        }
    }
}