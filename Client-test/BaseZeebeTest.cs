using NUnit.Framework;
using Grpc.Core;
using GatewayProtocol;
using Zeebe.Client.Impl;

namespace Zeebe.Client
{
    public class BaseZeebeTest
    {
        private Server server;
        private GatewayTestService testService;
        private IZeebeClient client;

        public Server Server { get { return server; }}
        public GatewayTestService TestService { get { return testService; }}
        public IZeebeClient ZeebeClient { get { return client; }}


        [SetUp]
        public void Init()
        {
            client = new ZeebeClient("localhost:26500");
            server = new Server();
            server.Ports.Add(new ServerPort("localhost", 26500, ServerCredentials.Insecure));

            testService = new GatewayTestService();
            var serviceDefinition = Gateway.BindService(testService);
            server.Services.Add(serviceDefinition);
            server.Start();
        }

        [TearDown]
        public void Stop()
        {
            server.ShutdownAsync();
        }
    }
}
