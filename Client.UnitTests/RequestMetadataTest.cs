using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class RequestMetadataTest  : BaseZeebeTest
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

            var entry = sendMetadata[0];
            Assert.AreEqual("user-agent", entry.Key);
            Assert.IsTrue(entry.Value.Contains("csharp"));
            Assert.IsTrue(entry.Value.Contains(typeof(ZeebeClient).Assembly.GetName().Version.ToString()));
        }
    }
}