
using System.Threading.Tasks;
using GatewayProtocol;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class UpdatePayloadTest : BaseZeebeTest
    {
        [Test]
        public async Task shouldSendRequestAsExpected()
        {
            // given
            var expectedRequest = new UpdateWorkflowInstancePayloadRequest
            {
                ElementInstanceKey = 2123,
                Payload = "{\"foo\":\"bar\"}"
            };

            // when
            await ZeebeClient.NewUpdatePayloadCommand(2123).Payload("{\"foo\":\"bar\"}").Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, request);
        }
    }
}