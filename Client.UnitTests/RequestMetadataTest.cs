using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;
using Zeebe.Client.Helpers;

namespace Zeebe.Client
{
    [TestFixture]
    public class RequestMetadataTest : BaseZeebeTest
    {
        [Test]
        public async Task ShouldUseUserAgentHeader()
        {
            // given
            Metadata sendMetadata = null;
            TestService.ConsumeRequestHeaders(metadata => { sendMetadata = metadata; });

            // when
            await ZeebeClient.TopologyRequest().Send();

            // then
            Assert.NotNull(sendMetadata);

            var entry = sendMetadata.Single(e => e.Key == "client");
            // user-agent is typically something like: key=user-agent, value=grpc-dotnet/2.53.0 (.NET 7.0.5; CLR 7.0.5; net7.0; windows; x64)
            StringAssert.Contains("csharp", entry.Value);
            StringAssert.Contains(typeof(ZeebeClient).Assembly.GetName().Version?.ToString() ?? string.Empty, entry.Value);
        }
    }
}