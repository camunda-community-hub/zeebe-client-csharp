using NUnit.Framework;
using System;
using Zeebe.Impl;
using GatewayProtocol;
using Grpc.Core.Testing;
using Grpc.Core;

namespace zbgrpctest
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
    }
}
