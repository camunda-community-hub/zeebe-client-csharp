using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using GatewayProtocol;
using Google.Protobuf;
using NUnit.Framework;

namespace Zeebe.Client
{       
    [TestFixture]
    public class DeploymentTest : BaseZeebeTest
    {
        
        private readonly string DemoProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/demo-process.bpmn");
        
        [Test]
        public async Task ShouldSendDeployResourceFileAsExpected()
        {
            // given
            var expectedRequest = new DeployWorkflowRequest
            {
                Workflows =
                {
                    new WorkflowRequestObject
                    {
                        Definition = ByteString.FromStream(File.OpenRead(DemoProcessPath)),
                        Name = DemoProcessPath,
                        Type = WorkflowRequestObject.Types.ResourceType.File
                    }
                }
            };

            // when
            await ZeebeClient.NewDeployCommand().AddResourceFile(DemoProcessPath).Send();
            
            // then
            var actualRequest = TestService.Requests[0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }
        
        [Test]
        public async Task ShouldSendDeployResourceStringAsExpected()
        {
            // given
            var expectedRequest = new DeployWorkflowRequest
            {
                Workflows =
                {
                    new WorkflowRequestObject
                    {
                        Definition = ByteString.FromStream(File.OpenRead(DemoProcessPath)),
                        Name = DemoProcessPath,
                        Type = WorkflowRequestObject.Types.ResourceType.File
                    }
                }
            };

            // when
            var fileContent = File.ReadAllText(DemoProcessPath);
            await ZeebeClient.NewDeployCommand()
                .AddResourceString(fileContent, Encoding.UTF8, DemoProcessPath)
                .Send();
            
            // then
            var actualRequest = TestService.Requests[0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }
        
        [Test]
        public async Task ShouldSendDeployResourceStringUtf8AsExpected()
        {
            // given
            var expectedRequest = new DeployWorkflowRequest
            {
                Workflows =
                {
                    new WorkflowRequestObject
                    {
                        Definition = ByteString.FromStream(File.OpenRead(DemoProcessPath)),
                        Name = DemoProcessPath,
                        Type = WorkflowRequestObject.Types.ResourceType.File
                    }
                }
            };

            // when
            var fileContent = File.ReadAllText(DemoProcessPath);
            await ZeebeClient.NewDeployCommand()
                .AddResourceStringUtf8(fileContent, DemoProcessPath)
                .Send();
            
            // then
            var actualRequest = TestService.Requests[0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }
        
        [Test]
        public async Task ShouldSendDeployResourceBytesAsExpected()
        {
            // given
            var expectedRequest = new DeployWorkflowRequest
            {
                Workflows =
                {
                    new WorkflowRequestObject
                    {
                        Definition = ByteString.FromStream(File.OpenRead(DemoProcessPath)),
                        Name = DemoProcessPath,
                        Type = WorkflowRequestObject.Types.ResourceType.File
                    }
                }
            };

            // when
            var fileContent = File.ReadAllText(DemoProcessPath);
            await ZeebeClient.NewDeployCommand()
                .AddResourceBytes(Encoding.UTF8.GetBytes(fileContent), DemoProcessPath)
                .Send();
            
            // then
            var actualRequest = TestService.Requests[0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }
        
        [Test]
        public async Task ShouldSendDeployResourceStreamAsExpected()
        {
            // given
            var expectedRequest = new DeployWorkflowRequest
            {
                Workflows =
                {
                    new WorkflowRequestObject
                    {
                        Definition = ByteString.FromStream(File.OpenRead(DemoProcessPath)),
                        Name = DemoProcessPath,
                        Type = WorkflowRequestObject.Types.ResourceType.File
                    }
                }
            };

            // when
            await ZeebeClient.NewDeployCommand()
                .AddResourceStream(File.OpenRead(DemoProcessPath), DemoProcessPath)
                .Send();
            
            // then
            var actualRequest = TestService.Requests[0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }
        
        [Test]
        public async Task ShouldSendDeployResourceAndGetResponseAsExpected()
        {
            // given
            var expectedResponse = new DeployWorkflowResponse {Key = 1};
            expectedResponse.Workflows.Add(new WorkflowMetadata
            {
                BpmnProcessId = "process",
                ResourceName = DemoProcessPath,
                Version = 1,
                WorkflowKey = 2
            });
            
            TestService.AddRequestHandler(typeof(DeployWorkflowRequest), request => expectedResponse);


            // when
            var deployWorkflowResponse = await ZeebeClient.NewDeployCommand()
                .AddResourceFile(DemoProcessPath)
                .Send();

            // then
            Assert.AreEqual(1, deployWorkflowResponse.Key);
            Assert.AreEqual(1, deployWorkflowResponse.Workflows.Count);
            
            var workflowMetadata = deployWorkflowResponse.Workflows[0];
            Assert.AreEqual("process", workflowMetadata.BpmnProcessId);
            Assert.AreEqual(1, workflowMetadata.Version);
            Assert.AreEqual(DemoProcessPath, workflowMetadata.ResourceName);
            Assert.AreEqual(2, workflowMetadata.WorkflowKey);
        }
        
        
        [Test]
        public async Task ShouldSendMultipleDeployResourceAsExpected()
        {
            // given
            var expectedRequest = new DeployWorkflowRequest
            {
                Workflows =
                {
                    new WorkflowRequestObject
                    {
                        Definition = ByteString.FromStream(File.OpenRead(DemoProcessPath)),
                        Name = DemoProcessPath,
                        Type = WorkflowRequestObject.Types.ResourceType.File
                    },
                    new WorkflowRequestObject
                    {
                        Definition = ByteString.FromStream(File.OpenRead(DemoProcessPath)),
                        Name = DemoProcessPath,
                        Type = WorkflowRequestObject.Types.ResourceType.File
                    }
                }
            };
            
            // when
            await ZeebeClient.NewDeployCommand()
                .AddResourceFile(DemoProcessPath)
                .AddResourceStream(File.OpenRead(DemoProcessPath), DemoProcessPath)
                .Send();

            // then
            var actualRequest = TestService.Requests[0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }
        
        [Test]
        public async Task ShouldSendMultipleDeployResourceAndGetResponseAsExpected()
        {
            // given
            var expectedResponse = new DeployWorkflowResponse {Key = 1};
            expectedResponse.Workflows.Add(new WorkflowMetadata
            {
                BpmnProcessId = "process",
                ResourceName = DemoProcessPath,
                Version = 1,
                WorkflowKey = 2
            });
            expectedResponse.Workflows.Add(new WorkflowMetadata
            {
                BpmnProcessId = "process2",
                ResourceName = DemoProcessPath,
                Version = 1,
                WorkflowKey = 3
            });
            
            TestService.AddRequestHandler(typeof(DeployWorkflowRequest), request => expectedResponse);

            // when
            var fileContent = File.ReadAllText(DemoProcessPath);
            var deployWorkflowResponse = await ZeebeClient.NewDeployCommand()
                .AddResourceFile(DemoProcessPath)
                .AddResourceString(fileContent, Encoding.UTF8, DemoProcessPath)
                .Send();

            // then
            Assert.AreEqual(1, deployWorkflowResponse.Key);
            Assert.AreEqual(2, deployWorkflowResponse.Workflows.Count);
            
            var workflowMetadata = deployWorkflowResponse.Workflows[0];
            Assert.AreEqual("process", workflowMetadata.BpmnProcessId);
            Assert.AreEqual(1, workflowMetadata.Version);
            Assert.AreEqual(DemoProcessPath, workflowMetadata.ResourceName);
            Assert.AreEqual(2, workflowMetadata.WorkflowKey);
            
            var workflowMetadata2 = deployWorkflowResponse.Workflows[1];
            Assert.AreEqual("process2", workflowMetadata2.BpmnProcessId);
            Assert.AreEqual(1, workflowMetadata2.Version);
            Assert.AreEqual(DemoProcessPath, workflowMetadata2.ResourceName);
            Assert.AreEqual(3, workflowMetadata2.WorkflowKey);
        }
    }
}