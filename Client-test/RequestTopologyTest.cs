using NUnit.Framework;
using GatewayProtocol;

namespace Zeebe.Client
{
    [TestFixture]
    public class RequestTopologyTest : BaseZeebeTest
    {
        [Test]
        public async void ShouldSendRequestAsExpected()
        {
            // given
            TopologyRequest expectedRequest = new TopologyRequest();

            // when
            TopologyResponse response = await ZeebeClient.TopologyRequest();

            // then
            var actualRequest = TestService.Requests[0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }


        [Test]
        public async void ShouldReceiveResponseAsExpected()
        {
            // given

            // when
            TopologyResponse response = await ZeebeClient.TopologyRequest();

            // then

        }
    }
}
