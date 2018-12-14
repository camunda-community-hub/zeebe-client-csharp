using GatewayProtocol;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Zeebe.Client
{
    [TestFixture]
    public class CancelWorkflowInstanceTest : BaseZeebeTest
    {
        [Test]
        public async Task shouldSendRequestAsExpected()
        {
            // given
            var expected = new CancelWorkflowInstanceRequest
            {
                WorkflowInstanceKey = 12113
            };

            // when
            await ZeebeClient.NewCancelInstanceCommand(12113).Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expected, request);
        }

    }
}