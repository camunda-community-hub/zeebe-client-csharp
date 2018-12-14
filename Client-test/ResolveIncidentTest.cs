using System.Threading.Tasks;
using GatewayProtocol;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class ResolveIncidentTest : BaseZeebeTest
    {

        [Test]
        public async Task shouldSendRequestAsExpected()
        {
            // given
            var expected = new ResolveIncidentRequest
            {
                IncidentKey = 1201321
            };

            // when
            await ZeebeClient.NewResolveIncidentCommand(1201321).Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expected, request);
        }

    }
}