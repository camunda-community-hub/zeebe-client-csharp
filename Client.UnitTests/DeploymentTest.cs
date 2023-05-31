using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Google.Protobuf;
using Grpc.Core;
using NUnit.Framework;
using Zeebe.Client.Helpers;

namespace Zeebe.Client
{
    [TestFixture]
    public class DeploymentTest : BaseZeebeTest
    {
        private readonly string _demoProcessPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "demo-process.bpmn");

        [Test]
        public async Task ShouldSendDeployResourceFileAsExpected()
        {
            // given
            var expectedRequest = new DeployProcessRequest
            {
                Processes =
                {
                    new ProcessRequestObject
                    {
                        Definition = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                        Name = _demoProcessPath
                    }
                }
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewDeployCommand().AddResourceFile(_demoProcessPath).Send();

            // then
            var actualRequest = TestService.Requests[typeof(DeployProcessRequest)][0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public void ShouldTimeoutRequest()
        {
            // given
            TestService.Reset();

            // when
            var task = ZeebeClient
                .NewDeployCommand()
                .AddResourceFile(_demoProcessPath)
                .Send(TimeSpan.Zero);
            var rpcException = Assert.Throws<RpcException>(() => task.GetAwaiter().GetResult());

            // then
            Assert.AreEqual(StatusCode.DeadlineExceeded, rpcException!.Status.StatusCode);
        }

        [Test]
        public void ShouldCancelRequest()
        {
            // given
            TestService.Reset();

            // when
            var task = ZeebeClient
                .NewDeployCommand()
                .AddResourceFile(_demoProcessPath)
                .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
            var rpcException = Assert.Throws<RpcException>(() => task.GetAwaiter().GetResult());

            // then
            Assert.AreEqual(StatusCode.Cancelled, rpcException!.Status.StatusCode);
        }

        [Test]
        public async Task ShouldSendDeployResourceStringAsExpected()
        {
            // given
            var expectedRequest = new DeployProcessRequest
            {
                Processes =
                {
                    new ProcessRequestObject
                    {
                        Definition = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                        Name = _demoProcessPath
                    }
                }
            };
            TestService.Reset();

            // when
            var fileContent = File.ReadAllText(_demoProcessPath);
            await ZeebeClient.NewDeployCommand()
                .AddResourceString(fileContent, Encoding.UTF8, _demoProcessPath)
                .Send();

            // then
            var actualRequest = TestService.Requests[typeof(DeployProcessRequest)][0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public async Task ShouldSendDeployResourceStringUtf8AsExpected()
        {
            // given
            var expectedRequest = new DeployProcessRequest
            {
                Processes =
                {
                    new ProcessRequestObject
                    {
                        Definition = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                        Name = _demoProcessPath
                    }
                }
            };
            TestService.Reset();

            // when
            var fileContent = File.ReadAllText(_demoProcessPath);
            await ZeebeClient.NewDeployCommand()
                .AddResourceStringUtf8(fileContent, _demoProcessPath)
                .Send();

            // then
            var actualRequest = TestService.Requests[typeof(DeployProcessRequest)][0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public async Task ShouldSendDeployResourceBytesAsExpected()
        {
            // given
            var expectedRequest = new DeployProcessRequest
            {
                Processes =
                {
                    new ProcessRequestObject
                    {
                        Definition = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                        Name = _demoProcessPath
                    }
                }
            };
            TestService.Reset();

            // when
            var fileContent = File.ReadAllText(_demoProcessPath);
            await ZeebeClient.NewDeployCommand()
                .AddResourceBytes(Encoding.UTF8.GetBytes(fileContent), _demoProcessPath)
                .Send();

            // then
            var actualRequest = TestService.Requests[typeof(DeployProcessRequest)][0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public async Task ShouldSendDeployResourceStreamAsExpected()
        {
            // given
            var expectedRequest = new DeployProcessRequest
            {
                Processes =
                {
                    new ProcessRequestObject
                    {
                        Definition = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                        Name = _demoProcessPath
                    }
                }
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewDeployCommand()
                .AddResourceStream(File.OpenRead(_demoProcessPath), _demoProcessPath)
                .Send();

            // then
            var actualRequest = TestService.Requests[typeof(DeployProcessRequest)][0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public async Task ShouldSendDeployResourceAndGetResponseAsExpected()
        {
            // given
            var expectedResponse = new DeployProcessResponse { Key = 1 };
            expectedResponse.Processes.Add(new ProcessMetadata
            {
                BpmnProcessId = "process",
                ResourceName = _demoProcessPath,
                Version = 1,
                ProcessDefinitionKey = 2
            });

            TestService.AddRequestHandler<DeployProcessRequest>(_ => expectedResponse);

            // when
            var deployProcessResponse = await ZeebeClient.NewDeployCommand()
                .AddResourceFile(_demoProcessPath)
                .Send();

            // then
            Assert.AreEqual(1, deployProcessResponse.Key);
            Assert.AreEqual(1, deployProcessResponse.Processes.Count);

            var processMetadata = deployProcessResponse.Processes[0];
            Assert.AreEqual("process", processMetadata.BpmnProcessId);
            Assert.AreEqual(1, processMetadata.Version);
            Assert.AreEqual(_demoProcessPath, processMetadata.ResourceName);
            Assert.AreEqual(2, processMetadata.ProcessDefinitionKey);
        }

        [Test]
        public async Task ShouldSendMultipleDeployResourceAsExpected()
        {
            // given
            var expectedRequest = new DeployProcessRequest
            {
                Processes =
                {
                    new ProcessRequestObject
                    {
                        Definition = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                        Name = _demoProcessPath
                    },
                    new ProcessRequestObject
                    {
                        Definition = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                        Name = _demoProcessPath
                    }
                }
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewDeployCommand()
                .AddResourceFile(_demoProcessPath)
                .AddResourceStream(File.OpenRead(_demoProcessPath), _demoProcessPath)
                .Send();

            // then
            var actualRequest = TestService.Requests[typeof(DeployProcessRequest)][0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public async Task ShouldSendMultipleDeployResourceAndGetResponseAsExpected()
        {
            // given
            var expectedResponse = new DeployProcessResponse { Key = 1 };
            expectedResponse.Processes.Add(new ProcessMetadata
            {
                BpmnProcessId = "process",
                ResourceName = _demoProcessPath,
                Version = 1,
                ProcessDefinitionKey = 2
            });
            expectedResponse.Processes.Add(new ProcessMetadata
            {
                BpmnProcessId = "process2",
                ResourceName = _demoProcessPath,
                Version = 1,
                ProcessDefinitionKey = 3
            });

            TestService.AddRequestHandler<DeployProcessRequest>(_ => expectedResponse);

            // when
            var fileContent = File.ReadAllText(_demoProcessPath);
            var deployProcessResponse = await ZeebeClient.NewDeployCommand()
                .AddResourceFile(_demoProcessPath)
                .AddResourceString(fileContent, Encoding.UTF8, _demoProcessPath)
                .Send();

            // then
            Assert.AreEqual(1, deployProcessResponse.Key);
            Assert.AreEqual(2, deployProcessResponse.Processes.Count);

            var processMetadata = deployProcessResponse.Processes[0];
            Assert.AreEqual("process", processMetadata.BpmnProcessId);
            Assert.AreEqual(1, processMetadata.Version);
            Assert.AreEqual(_demoProcessPath, processMetadata.ResourceName);
            Assert.AreEqual(2, processMetadata.ProcessDefinitionKey);

            var processMetadata2 = deployProcessResponse.Processes[1];
            Assert.AreEqual("process2", processMetadata2.BpmnProcessId);
            Assert.AreEqual(1, processMetadata2.Version);
            Assert.AreEqual(_demoProcessPath, processMetadata2.ResourceName);
            Assert.AreEqual(3, processMetadata2.ProcessDefinitionKey);
        }
    }
}