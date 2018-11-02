using NUnit.Framework;
using System;
using Zeebe.Impl;
using GatewayProtocol;


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

            // when
            HealthResponse response = await zeebeClient.HealtRequest();

            // then


        }
    }
}
