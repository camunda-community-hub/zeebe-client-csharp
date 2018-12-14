using System.Threading.Tasks;
using GatewayProtocol;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class UpdateRetriesTest : BaseZeebeTest
    {
        [Test]
        public async Task shouldSendRequestAsExpected()
        {
            // given
            var expectedRequest = new UpdateJobRetriesRequest
            {
                JobKey = 1024,
                Retries = 223
            };

            // when
            await ZeebeClient
                .NewUpdateRetriesCommand(1024)
                .Retries(223)
                .Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, request);
        }

    }
}