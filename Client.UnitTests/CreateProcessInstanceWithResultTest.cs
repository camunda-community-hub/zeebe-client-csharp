using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client;

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

        // when
        _ = await ZeebeClient.NewCreateProcessInstanceCommand()
            .BpmnProcessId("process")
            .LatestVersion()
            .WithResult()
            .Send();

        // then
        var request = TestService.Requests[typeof(CreateProcessInstanceWithResultRequest)][0];
        Assert.AreEqual(expectedRequest, request);
    }

    [Test]
    public async Task ShouldSendRequestWithStartInstructionAsExpected()
    {
        // given
        var expectedRequest = new CreateProcessInstanceWithResultRequest
        {
            Request = new CreateProcessInstanceRequest
            {
                StartInstructions = { new ProcessInstanceCreationStartInstruction { ElementId = "StartHere" } }
            },
            RequestTimeout = 20 * 1000
        };

        // when
        await ZeebeClient.NewCreateProcessInstanceCommand()
            .AddStartInstruction("StartHere")
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
        TestService.AddRequestHandler(typeof(CreateProcessInstanceWithResultRequest),
            request =>
            {
                _ = new EventWaitHandle(false, EventResetMode.AutoReset).WaitOne();
                return null;
            });

        // when
        var task = ZeebeClient.NewCreateProcessInstanceCommand()
            .BpmnProcessId("process")
            .LatestVersion()
            .WithResult()
            .Send(TimeSpan.Zero);
        var aggregateException = Assert.Throws<AggregateException>(() => task.Wait(TimeSpan.FromSeconds(15)));
        var rpcException = (RpcException)aggregateException.InnerExceptions[0];

        // then
        Assert.AreEqual(StatusCode.DeadlineExceeded, rpcException.Status.StatusCode);
    }

    [Test]
    public void ShouldCancelRequest()
    {
        // given
        TestService.AddRequestHandler(typeof(CreateProcessInstanceWithResultRequest),
            request =>
            {
                _ = new EventWaitHandle(false, EventResetMode.AutoReset).WaitOne();
                return null;
            });

        // when
        var task = ZeebeClient.NewCreateProcessInstanceCommand()
            .BpmnProcessId("process")
            .LatestVersion()
            .WithResult()
            .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
        var aggregateException = Assert.Throws<AggregateException>(() => task.Wait(TimeSpan.FromSeconds(15)));
        var rpcException = (RpcException)aggregateException.InnerExceptions[0];

        // then
        Assert.AreEqual(StatusCode.Cancelled, rpcException.Status.StatusCode);
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

        // when
        _ = await ZeebeClient.NewCreateProcessInstanceCommand()
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

        // when
        _ = await ZeebeClient.NewCreateProcessInstanceCommand()
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

        // when
        _ = await ZeebeClient.NewCreateProcessInstanceCommand()
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

        // when
        _ = await ZeebeClient.NewCreateProcessInstanceCommand()
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

        // when
        _ = await ZeebeClient.NewCreateProcessInstanceCommand()
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
    public async Task ShouldSendRequestWithTenantIdAsExpected()
    {
        // given
        var expectedRequest = new CreateProcessInstanceWithResultRequest
        {
            Request = new CreateProcessInstanceRequest
            {
                ProcessDefinitionKey = 1,
                TenantId = "tenant1"
            },
            RequestTimeout = 20 * 1000
        };

        // when
        _ = await ZeebeClient.NewCreateProcessInstanceCommand()
        .ProcessDefinitionKey(1)
        .AddTenantId("tenant1")
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

        // when
        _ = await ZeebeClient.NewCreateProcessInstanceCommand()
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

        // when
        _ = await ZeebeClient.NewCreateProcessInstanceCommand()
        .BpmnProcessId("process")
        .LatestVersion()
        .Variables("{\"foo\":1}")
        .WithResult()
        .FetchVariables(new List<string> { "foo", "bar" })
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

        TestService.AddRequestHandler(typeof(CreateProcessInstanceWithResultRequest), request => expectedResponse);

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