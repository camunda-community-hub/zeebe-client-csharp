using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;
using Zeebe.Client.Helpers;

namespace Zeebe.Client
{
    [TestFixture]
    public class CreateProcessInstanceWithResultTest : BaseZeebeTest
    {
        [Test]
        public async Task ShouldSendRequestAsExpected()
        {
            // given
            var expectedRequest = new CreateProcessInstanceWithResultRequest
            {
                Request = new CreateProcessInstanceRequest
                {
                    BpmnProcessId = "process",
                    Version = -1
                },
                RequestTimeout = 20 * 1000
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .WithResult()
                .Send();

            // then
            var request = TestService.Requests[typeof(CreateProcessInstanceWithResultRequest)][0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public void ShouldTimeoutRequest()
        {
            // given
            TestService.AddRequestHandler<CreateProcessInstanceWithResultRequest>(
                _ =>
                {
                    new EventWaitHandle(false, EventResetMode.AutoReset).WaitOne();
                    return null;
                });

            // when
            var task = ZeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .WithResult()
                .Send(TimeSpan.Zero);
            var rpcException = Assert.Throws<RpcException>(() => task.GetAwaiter().GetResult());

            // then
            Assert.AreEqual(StatusCode.DeadlineExceeded, rpcException!.Status.StatusCode);
        }

        [Test]
        public void ShouldCancelRequest()
        {
            // given
            TestService.AddRequestHandler<CreateProcessInstanceWithResultRequest>(
                _ =>
                {
                    new EventWaitHandle(false, EventResetMode.AutoReset).WaitOne();
                    return null;
                });

            // when
            var task = ZeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .WithResult()
                .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
            var rpcException = Assert.Throws<RpcException>(() => task.GetAwaiter().GetResult());

            // then
            Assert.AreEqual(StatusCode.Cancelled, rpcException!.Status.StatusCode);
        }

        [Test]
        public async Task ShouldSendRequestWithVersionAsExpected()
        {
            // given
            var expectedRequest = new CreateProcessInstanceWithResultRequest
            {
                Request = new CreateProcessInstanceRequest
                {
                    BpmnProcessId = "process",
                    Version = 1
                },
                RequestTimeout = 20 * 1000
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("process")
                .Version(1)
                .WithResult()
                .Send();

            // then
            var request = TestService.Requests[typeof(CreateProcessInstanceWithResultRequest)][0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task ShouldSendRequestWithProcessDefinitionKeyAsExpected()
        {
            // given
            var expectedRequest = new CreateProcessInstanceWithResultRequest
            {
                Request = new CreateProcessInstanceRequest
                {
                    ProcessDefinitionKey = 1
                },
                RequestTimeout = 20 * 1000
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewCreateProcessInstanceCommand()
                .ProcessDefinitionKey(1)
                .WithResult()
                .Send();

            // then
            var request = TestService.Requests[typeof(CreateProcessInstanceWithResultRequest)][0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task ShouldSendRequestWithRequestTimeoutAsExpected()
        {
            // given
            var expectedRequest = new CreateProcessInstanceWithResultRequest
            {
                Request = new CreateProcessInstanceRequest
                {
                    ProcessDefinitionKey = 1
                },
                RequestTimeout = 123 * 1000
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewCreateProcessInstanceCommand()
                .ProcessDefinitionKey(1)
                .WithResult()
                .Send(TimeSpan.FromSeconds(123));

            // then
            var request = TestService.Requests[typeof(CreateProcessInstanceWithResultRequest)][0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task ShouldSendRequestWithVariablesAsExpected()
        {
            // given
            var expectedRequest = new CreateProcessInstanceWithResultRequest
            {
                Request = new CreateProcessInstanceRequest
                {
                    ProcessDefinitionKey = 1,
                    Variables = "{\"foo\":1}"
                },
                RequestTimeout = 20 * 1000
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewCreateProcessInstanceCommand()
                .ProcessDefinitionKey(1)
                .Variables("{\"foo\":1}")
                .WithResult()
                .Send();

            // then
            var request = TestService.Requests[typeof(CreateProcessInstanceWithResultRequest)][0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task ShouldSendRequestWithVariablesAndProcessIdAsExpected()
        {
            // given
            var expectedRequest = new CreateProcessInstanceWithResultRequest
            {
                Request = new CreateProcessInstanceRequest
                {
                    BpmnProcessId = "process",
                    Version = -1,
                    Variables = "{\"foo\":1}"
                },
                RequestTimeout = 20 * 1000
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .Variables("{\"foo\":1}")
                .WithResult()
                .Send();

            // then
            var request = TestService.Requests[typeof(CreateProcessInstanceWithResultRequest)][0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task ShouldSendRequestWithFetchVariables()
        {
            // given
            var expectedRequest = new CreateProcessInstanceWithResultRequest
            {
                Request = new CreateProcessInstanceRequest
                {
                    BpmnProcessId = "process",
                    Version = -1,
                    Variables = "{\"foo\":1}"
                },
                RequestTimeout = 20 * 1000,
                FetchVariables = { "foo", "bar" }
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .Variables("{\"foo\":1}")
                .WithResult()
                .FetchVariables("foo", "bar")
                .Send();

            // then
            var request = TestService.Requests[typeof(CreateProcessInstanceWithResultRequest)][0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task ShouldSendRequestWithFetchVariablesAsList()
        {
            // given
            var expectedRequest = new CreateProcessInstanceWithResultRequest
            {
                Request = new CreateProcessInstanceRequest
                {
                    BpmnProcessId = "process",
                    Version = -1,
                    Variables = "{\"foo\":1}"
                },
                RequestTimeout = 20 * 1000,
                FetchVariables = { "foo", "bar" }
            };
            TestService.Reset();

            // when
            await ZeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .Variables("{\"foo\":1}")
                .WithResult()
                .FetchVariables("foo", "bar")
                .Send();

            // then
            var request = TestService.Requests[typeof(CreateProcessInstanceWithResultRequest)][0];
            Assert.AreEqual(expectedRequest, request);
        }

        [Test]
        public async Task ShouldReceiveResponseAsExpected()
        {
            // given
            var expectedResponse = new CreateProcessInstanceWithResultResponse
            {
                BpmnProcessId = "process",
                Version = 1,
                ProcessDefinitionKey = 2,
                ProcessInstanceKey = 121,
                Variables = "{\"foo\":\"bar\"}"
            };

            TestService.AddRequestHandler<CreateProcessInstanceWithResultRequest>(_ => expectedResponse);

            // when
            var processInstanceResponse = await ZeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("process")
                .LatestVersion()
                .WithResult()
                .Send();

            // then
            Assert.AreEqual(2, processInstanceResponse.ProcessDefinitionKey);
            Assert.AreEqual(1, processInstanceResponse.Version);
            Assert.AreEqual(121, processInstanceResponse.ProcessInstanceKey);
            Assert.AreEqual("process", processInstanceResponse.BpmnProcessId);
            Assert.AreEqual("{\"foo\":\"bar\"}", processInstanceResponse.Variables);
        }
    }
}