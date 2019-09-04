//
//    Copyright (c) 2018 camunda services GmbH (info@camunda.com)
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client
{
    public class BaseZeebeTest
    {
        private Server server;
        private GatewayTestService testService;
        private IZeebeClient client;

        public Server Server => server;
        public GatewayTestService TestService => testService;
        public IZeebeClient ZeebeClient => client;

        [SetUp]
        public void Init()
        {
            server = new Server();
            server.Ports.Add(new ServerPort("localhost", 26500, ServerCredentials.Insecure));

            testService = new GatewayTestService();
            var serviceDefinition = Gateway.BindService(testService);
            server.Services.Add(serviceDefinition);
            server.Start();

            client = Client.ZeebeClient.Builder().UseGatewayAddress("localhost:26500").UsePlainText().Build();
        }

        [TearDown]
        public void Stop()
        {
            client.Dispose();
            server.ShutdownAsync().Wait();
        }
    }
}
