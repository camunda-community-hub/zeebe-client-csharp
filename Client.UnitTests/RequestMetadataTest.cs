using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;

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
            Assert.That(sendMetadata, Is.Not.Null);

            var entry = sendMetadata[0];
            Assert.That(entry.Key, Is.EqualTo("user-agent"), $"Expect user agent in metadata '{sendMetadata}'");
            Assert.That(entry.Value.Contains("zeebe-client-csharp/" + typeof(ZeebeClient).Assembly.GetName().Version), Is.True, $"Expect user agent contains zeebe-client-csharp, but was {entry}");
        }
    }
}
