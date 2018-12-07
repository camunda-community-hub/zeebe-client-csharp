using GatewayProtocol;
using NUnit.Framework;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client
{
    [TestFixture]
    public class CompleteJobTest : BaseZeebeTest
    {
        [Test]
        public async Task ShouldSendRequestAsExpected()
        {
            // given
            const string Payload = "{\"foo\":23}";
            const int JobKey = 255;
            var expectedRequest = new CompleteJobRequest
            {
                JobKey = JobKey,
                Payload = Payload
            };

            // when
            await ZeebeClient.NewCompleteJobCommand(JobKey).Payload(Payload).Send();

            // then
            var actualRequest = TestService.Requests[0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }
    }
}
