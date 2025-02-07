using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client;

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

        var entry = sendMetadata[0];
        Assert.AreEqual("user-agent", entry.Key, $"Expect user agent in metadata '{sendMetadata}'");
        Assert.IsTrue(entry.Value.Contains("zeebe-client-csharp/" + typeof(ZeebeClient).Assembly.GetName().Version),
            $"Expect user agent contains zeebe-client-csharp, but was {entry}");
    }
}