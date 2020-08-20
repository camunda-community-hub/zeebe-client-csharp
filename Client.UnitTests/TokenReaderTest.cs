using System;
using System.IO;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;
using Zeebe.Client.Impl.Builder;

namespace Zeebe.Client
{
    [TestFixture]
    public class TokenReaderTest
    {
        private readonly string CredentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "credentials");

        [Test]
        public void ShouldReadTokens()
        {
            var tokenReader = new TokenReader(CredentialsPath);
        }
    }
}