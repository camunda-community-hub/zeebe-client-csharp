using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client;

[TestFixture]
public class SetVariablesTest : BaseZeebeTest
{
    [Test]
    public async Task ShouldSendRequestAsExpected()
    {
        // given
        var expectedRequest = new SetVariablesRequest
        {
            ElementInstanceKey = 2123,
            Variables = "{\"foo\":\"bar\"}"
        };

        // when
        _ = await ZeebeClient.NewSetVariablesCommand(2123).Variables("{\"foo\":\"bar\"}").Send();

        // then
        var request = TestService.Requests[typeof(SetVariablesRequest)][0];
        Assert.AreEqual(expectedRequest, request);
    }

    [Test]
    public void ShouldTimeoutRequest()
    {
        // given

        // when
        var task = ZeebeClient
            .NewSetVariablesCommand(2123)
            .Variables("{\"foo\":\"bar\"}")
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
            .NewSetVariablesCommand(2123)
            .Variables("{\"foo\":\"bar\"}")
            .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
        var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
        var rpcException = (RpcException)aggregateException.InnerExceptions[0];

        // then
        Assert.AreEqual(StatusCode.Cancelled, rpcException.Status.StatusCode);
    }

    [Test]
    public async Task ShouldSendRequestWithLocalSemanticsAsExpected()
    {
        // given
        var expectedRequest = new SetVariablesRequest
        {
            ElementInstanceKey = 2123,
            Variables = "{\"foo\":\"bar\"}",
            Local = true
        };

        // when
        _ = await ZeebeClient.NewSetVariablesCommand(2123).Variables("{\"foo\":\"bar\"}").Local().Send();

        // then
        var request = TestService.Requests[typeof(SetVariablesRequest)][0];
        Assert.AreEqual(expectedRequest, request);
    }

    [Test]
    public async Task ShouldReceiveResponseAsExpected()
    {
        // given
        var expectedResponse = new SetVariablesResponse
        {
            Key = 12
        };
        TestService.AddRequestHandler(typeof(SetVariablesRequest), request => expectedResponse);

        // when
        var response = await ZeebeClient.NewSetVariablesCommand(2123).Variables("{\"foo\":\"bar\"}").Local().Send();

        // then
        Assert.AreEqual(12, response.Key);
    }
}