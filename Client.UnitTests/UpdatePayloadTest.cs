
using GatewayProtocol;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Zeebe.Client
{
    [TestFixture]
    public class SetVariablesTest : BaseZeebeTest
    {
        [Test]
        public async Task ShouldSendRequestAsExpected()
        {
            // given
            var expectedRequest = new SetVariablesRequest
            {
                ElementInstanceKey = 2123,
                Variables = "{\"foo\":\"bar\"}"
            };

            // when
            await ZeebeClient.NewSetVariablesCommand(2123).Variables("{\"foo\":\"bar\"}").Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, request);
        }


        [Test]
        public async Task ShouldSendRequestWithLocalSemanticsAsExpected()
        {
            // given
            var expectedRequest = new SetVariablesRequest
            {
                ElementInstanceKey = 2123,
                Variables = "{\"foo\":\"bar\"}",
                Local = true
            };

            // when
            await ZeebeClient.NewSetVariablesCommand(2123).Variables("{\"foo\":\"bar\"}").Local().Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, request);
        }
    }
}