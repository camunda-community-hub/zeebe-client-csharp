using System;
using System.Threading.Tasks;
using GatewayProtocol;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class PublishMessageTest : BaseZeebeTest
    {
        [Test]
        public async Task shouldSendRequestAsExpected()
        {
            // given
            var expectedRequest = new PublishMessageRequest
            {
                CorrelationKey = "p-1",
                Name = "messageName"
            };

            // when
            await ZeebeClient
                .NewPublishMessageCommand()
                .MessageName("messageName")
                .CorrelationKey("p-1")
                .Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task shouldSendRequestWithTTLAsExpected()
        {
            // given
            var expectedRequest = new PublishMessageRequest
            {
                CorrelationKey = "p-1",
                Name = "messageName",
                TimeToLive = 10_000
            };

            // when
            await ZeebeClient
                .NewPublishMessageCommand()
                .MessageName("messageName")
                .CorrelationKey("p-1")
                .TimeToLive(TimeSpan.FromSeconds(10))
                .Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task shouldSendRequestWithIdAsExpected()
        {
            // given
            var expectedRequest = new PublishMessageRequest
            {
                CorrelationKey = "p-1",
                Name = "messageName",
                MessageId = "id",
                TimeToLive = 10_000
            };

            // when
            await ZeebeClient
                .NewPublishMessageCommand()
                .MessageName("messageName")
                .CorrelationKey("p-1")
                .MessageId("id")
                .TimeToLive(TimeSpan.FromSeconds(10))
                .Send();

            // then
            var request = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, request);
        }

    }
}