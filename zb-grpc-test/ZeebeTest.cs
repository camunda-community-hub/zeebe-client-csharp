using NUnit.Framework;
using System;
using Zeebe.Impl;
using GatewayProtocol;
using Grpc.Core.Testing;

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
            Grpc.Core.Server s = new Grpc.Core.Server();
            s.Start();

            // when
            TopologyResponse response = await zeebeClient.TopologyRequest();

            // then
        }
    }
}
