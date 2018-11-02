using NUnit.Framework;
using GatewayProtocol;
using Zeebe.Client.Impl.Responses;
using Zeebe.Client.Api.Responses;

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
            ITopology topology = await ZeebeClient.TopologyRequest().Send();

            // then
            var actualRequest = TestService.Requests[0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }


        [Test]
        public async void ShouldReceiveResponseAsExpected()
        {
            // given

            // when
            ITopology response = await ZeebeClient.TopologyRequest().Send();

            // then

        }
    }
}
