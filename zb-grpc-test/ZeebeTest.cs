using NUnit.Framework;
using System;
using Zeebe.Impl;
using GatewayProtocol;
using Grpc.Core.Testing;
using Grpc.Core;

namespace zbgrpctest
{
    [TestFixture]
    public class ZeebeTest
    {
        private readonly ZeebeClient zeebeClient = new ZeebeClient("localhost:26500");

        [Test]
        public async void RequestToplogy()
        {
            // given
            Server server = new Grpc.Core.Server();
            server.Ports.Add(new ServerPort("localhost", 26500, ServerCredentials.Insecure));
            var testService = new GatewayTestService();
            var serviceDefinition = Gateway.BindService(testService);
            server.Services.Add(serviceDefinition);
            server.Start();

            TopologyRequest expectedRequest = new TopologyRequest();

            // when
            TopologyResponse response = await zeebeClient.TopologyRequest();

            // then
            var actualRequest = testService.Requests[0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }
    }
}
