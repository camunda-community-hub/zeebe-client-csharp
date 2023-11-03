using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Google.Protobuf;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class DeploymentTest : BaseZeebeTest
    {
        private readonly string _demoProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "demo-process.bpmn");

        [Test]
        public async Task ShouldSendDeployResourceFileAsExpected()
        {
            // given
            var expectedRequest = new DeployResourceRequest
            {
                Resources =
                {
                    new Resource
                    {
                        Content = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                        Name = _demoProcessPath,
                    }
                }
            };

            // when
            await ZeebeClient.NewDeployCommand().AddResourceFile(_demoProcessPath).Send();

            // then
            var actualRequest = TestService.Requests[typeof(DeployResourceRequest)][0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public void ShouldTimeoutRequest()
        {
            // given

            // when
            var task = ZeebeClient
                .NewDeployCommand()
                .AddResourceFile(_demoProcessPath)
                .Send(TimeSpan.Zero);
            var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
            var rpcException = (RpcException)aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(StatusCode.DeadlineExceeded, rpcException.Status.StatusCode);
        }

        [Test]
        public void ShouldCancelRequest()
        {
            // given

            // when
            var task = ZeebeClient
                .NewDeployCommand()
                .AddResourceFile(_demoProcessPath)
                .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
            var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
            var rpcException = (RpcException)aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(StatusCode.Cancelled, rpcException.Status.StatusCode);
        }

        [Test]
        public async Task ShouldSendDeployResourceStringAsExpected()
        {
            // given
            var expectedRequest = new DeployResourceRequest
            {
                Resources =
                {
                    new Resource
                    {
                        Content = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                        Name = _demoProcessPath,
                    }
                }
            };

            // when
            var fileContent = File.ReadAllText(_demoProcessPath);
            await ZeebeClient.NewDeployCommand()
                .AddResourceString(fileContent, Encoding.UTF8, _demoProcessPath)
                .Send();

            // then
            var actualRequest = TestService.Requests[typeof(DeployResourceRequest)][0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public async Task ShouldSendDeployResourceStringUtf8AsExpected()
        {
            // given
            var expectedRequest = new DeployResourceRequest
            {
                Resources =
                {
                    new Resource
                    {
                        Content = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                        Name = _demoProcessPath,
                    }
                }
            };

            // when
            var fileContent = File.ReadAllText(_demoProcessPath);
            await ZeebeClient.NewDeployCommand()
                .AddResourceStringUtf8(fileContent, _demoProcessPath)
                .Send();

            // then
            var actualRequest = TestService.Requests[typeof(DeployResourceRequest)][0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public async Task ShouldSendDeployResourceBytesAsExpected()
        {
            // given
            var expectedRequest = new DeployResourceRequest
            {
                Resources =
                {
                    new Resource
                    {
                        Content = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                        Name = _demoProcessPath,
                    }
                }
            };

            // when
            var fileContent = File.ReadAllText(_demoProcessPath);
            await ZeebeClient.NewDeployCommand()
                .AddResourceBytes(Encoding.UTF8.GetBytes(fileContent), _demoProcessPath)
                .Send();

            // then
            var actualRequest = TestService.Requests[typeof(DeployResourceRequest)][0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public async Task ShouldSendDeployResourceStreamAsExpected()
        {
            // given
            var expectedRequest = new DeployResourceRequest
            {
                Resources =
                {
                    new Resource
                    {
                        Content = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                        Name = _demoProcessPath,
                    }
                }
            };

            // when
            await ZeebeClient.NewDeployCommand()
                .AddResourceStream(File.OpenRead(_demoProcessPath), _demoProcessPath)
                .Send();

            // then
            var actualRequest = TestService.Requests[typeof(DeployResourceRequest)][0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public async Task ShouldSendDeployResourceAndGetResponseAsExpected()
        {
            // given
            var expectedResponse = new DeployResourceResponse
            {
                Key = 1,
                Deployments =
                {
                    new Deployment
                    {
                        Process = new ProcessMetadata
                        {
                            BpmnProcessId = "process",
                            ResourceName = _demoProcessPath,
                            Version = 1,
                            ProcessDefinitionKey = 2
                        }
                    }
                }
            };

            TestService.AddRequestHandler(typeof(DeployResourceRequest), request => expectedResponse);

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
            var expectedRequest = new DeployResourceRequest
            {
                Resources =
                {
                    new Resource
                    {
                        Content = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                        Name = _demoProcessPath,
                    },
                    new Resource
                    {
                        Content = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                        Name = _demoProcessPath,
                    }
                }
            };

            // when
            await ZeebeClient.NewDeployCommand()
                .AddResourceFile(_demoProcessPath)
                .AddResourceStream(File.OpenRead(_demoProcessPath), _demoProcessPath)
                .Send();

            // then
            var actualRequest = TestService.Requests[typeof(DeployResourceRequest)][0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public async Task ShouldSendMultipleDeployResourceAndGetResponseAsExpected()
        {
            // given
            var expectedResponse = new DeployResourceResponse
            {
                Key = 1,
                Deployments =
                {
                    new Deployment
                    {
                        Process = new ProcessMetadata
                        {
                            BpmnProcessId = "process",
                            ResourceName = _demoProcessPath,
                            Version = 1,
                            ProcessDefinitionKey = 2
                        }
                    },
                    new Deployment
                    {
                        Decision = new DecisionMetadata
                        {
                            DecisionKey = 1,
                            DecisionRequirementsKey = 2,
                            Version = 3,
                            DmnDecisionId = "decisionId",
                            DmnDecisionName = "decisionName",
                            DmnDecisionRequirementsId = "idk",
                        }
                    },
                    new Deployment
                    {
                        DecisionRequirements = new DecisionRequirementsMetadata
                        {
                            Version = 1,
                            ResourceName = "requirement",
                            DecisionRequirementsKey = 2,
                            DmnDecisionRequirementsId = "id",
                            DmnDecisionRequirementsName = "nameRequirement"
                        }
                    }
                }
            };

            TestService.AddRequestHandler(typeof(DeployResourceRequest), request => expectedResponse);

            // when
            var fileContent = File.ReadAllText(_demoProcessPath);
            var deployProcessResponse = await ZeebeClient.NewDeployCommand()
                .AddResourceFile(_demoProcessPath)
                .AddResourceString(fileContent, Encoding.UTF8, _demoProcessPath)
                .Send();

            // then
            Assert.AreEqual(1, deployProcessResponse.Key);
            Assert.AreEqual(1, deployProcessResponse.Processes.Count);
            Assert.AreEqual(1, deployProcessResponse.Decisions.Count);
            Assert.AreEqual(1, deployProcessResponse.DecisionRequirements.Count);

            var processMetadata = deployProcessResponse.Processes[0];
            Assert.AreEqual("process", processMetadata.BpmnProcessId);
            Assert.AreEqual(1, processMetadata.Version);
            Assert.AreEqual(_demoProcessPath, processMetadata.ResourceName);
            Assert.AreEqual(2, processMetadata.ProcessDefinitionKey);

            var decisionMetadata = deployProcessResponse.Decisions[0];
            Assert.AreEqual(1, decisionMetadata.DecisionKey);
            Assert.AreEqual(2, decisionMetadata.DecisionRequirementsKey);
            Assert.AreEqual(3, decisionMetadata.Version);
            Assert.AreEqual("decisionId", decisionMetadata.DmnDecisionId);
            Assert.AreEqual("decisionName", decisionMetadata.DmnDecisionName);
            Assert.AreEqual("idk", decisionMetadata.DmnDecisionRequirementsId);

            var decisionRequirementsMetadata = deployProcessResponse.DecisionRequirements[0];
            Assert.AreEqual(2, decisionRequirementsMetadata.DecisionRequirementsKey);
            Assert.AreEqual(1, decisionRequirementsMetadata.Version);
            Assert.AreEqual("requirement", decisionRequirementsMetadata.ResourceName);
            Assert.AreEqual("nameRequirement", decisionRequirementsMetadata.DmnDecisionRequirementsName);
            Assert.AreEqual("id", decisionRequirementsMetadata.DmnDecisionRequirementsId);
        }

        [Test]
        public async Task ShouldSetTenantIdAsExpected()
        {
            // given
            var expectedRequest = new DeployResourceRequest
            {
                TenantId = "1234",
                Resources =
                {
                    new Resource
                    {
                        Content = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                        Name = _demoProcessPath
                    }
                }
            };

            // when
            await ZeebeClient.NewDeployCommand().AddTenantId("1234").AddResourceFile(_demoProcessPath).Send();

            // then
            var actualRequest = TestService.Requests[typeof(DeployResourceRequest)][0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }
    }
}
