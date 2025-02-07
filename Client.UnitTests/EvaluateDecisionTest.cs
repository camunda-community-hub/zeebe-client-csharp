using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client;

[TestFixture]
public class EvaluateDecisionTest : BaseZeebeTest
{
    [Test]
    public async Task ShouldSendRequestAsExpected()
    {
        // given
        var expectedRequest = new EvaluateDecisionRequest
        {
            DecisionId = "decision"
        };

    // when
        _ = await ZeebeClient.NewEvaluateDecisionCommand()
        .DecisionId("decision")
        .Send();

        // then
        var request = TestService.Requests[typeof(EvaluateDecisionRequest)][0];
        Assert.AreEqual(expectedRequest, request);
    }

    [Test]
    public void ShouldTimeoutRequest()
    {
        // given

        // when
        var task = ZeebeClient.NewEvaluateDecisionCommand()
            .DecisionId("decision")
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
        var task = ZeebeClient.NewEvaluateDecisionCommand()
            .DecisionId("decision")
            .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
        var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
        var rpcException = (RpcException)aggregateException.InnerExceptions[0];

        // then
        Assert.AreEqual(StatusCode.Cancelled, rpcException.Status.StatusCode);
    }

    [Test]
    public async Task ShouldSendRequestWithDecisionKeyExpected()
    {
        // given
        var expectedRequest = new EvaluateDecisionRequest
        {
            DecisionKey = 12
        };

    // when
        _ = await ZeebeClient.NewEvaluateDecisionCommand()
        .DecisionKey(12)
        .Send();

        // then
        var request = TestService.Requests[typeof(EvaluateDecisionRequest)][0];
        Assert.AreEqual(expectedRequest, request);
    }

    [Test]
    public async Task ShouldSendRequestWithVariablesAsExpected()
    {
        // given
        var expectedRequest = new EvaluateDecisionRequest
        {
            DecisionKey = 12,
            Variables = "{\"foo\":1}"
        };

    // when
        _ = await ZeebeClient.NewEvaluateDecisionCommand()
        .DecisionKey(12)
        .Variables("{\"foo\":1}")
        .Send();

        // then
        var request = TestService.Requests[typeof(EvaluateDecisionRequest)][0];
        Assert.AreEqual(expectedRequest, request);
    }

    [Test]
    public async Task ShouldSendRequestWithVariablesAndDecisionIdAsExpected()
    {
        // given
        var expectedRequest = new EvaluateDecisionRequest
        {
            DecisionId = "decision",
            Variables = "{\"foo\":1}"
        };

    // when
        _ = await ZeebeClient.NewEvaluateDecisionCommand()
        .DecisionId("decision")
        .Variables("{\"foo\":1}")
        .Send();

        // then
        var request = TestService.Requests[typeof(EvaluateDecisionRequest)][0];
        Assert.AreEqual(expectedRequest, request);
    }

    [Test]
    public async Task ShouldReceiveResponseAsExpected()
    {
        // given
        var expectedResponse = new EvaluateDecisionResponse
        {
            DecisionId = "decision",
            DecisionKey = 123,
            DecisionName = "decision-123",
            DecisionOutput = "1",
            DecisionVersion = 2,
            FailureMessage = "",
            FailedDecisionId = "",
            DecisionRequirementsId = "12",
            DecisionRequirementsKey = 1234,
            EvaluatedDecisions =
            {
                new EvaluatedDecision
                {
                    DecisionId = "decision",
                    DecisionKey = 123,
                    DecisionName = "decision-123",
                    DecisionOutput = "1",
                    DecisionVersion = 2,
                    DecisionType = "noop",
                    EvaluatedInputs =
                    {
                        new EvaluatedDecisionInput
                        {
                            InputId = "moep",
                            InputName = "moepmoep",
                            InputValue = "boom"
                        },
                        new EvaluatedDecisionInput
                        {
                            InputId = "moeb",
                            InputName = "moebmoeb",
                            InputValue = "boom"
                        }
                    },
                    MatchedRules =
                    {
                        new MatchedDecisionRule
                        {
                            EvaluatedOutputs =
                            {
                                new EvaluatedDecisionOutput
                                {
                                    OutputId = "outputId",
                                    OutputName = "output",
                                    OutputValue = "val"
                                },
                                new EvaluatedDecisionOutput
                                {
                                    OutputId = "outputId2",
                                    OutputName = "output2",
                                    OutputValue = "val2"
                                }
                            },
                            RuleId = "ruleid",
                            RuleIndex = 1
                        }
                    }
                }
            }
        };

        TestService.AddRequestHandler(typeof(EvaluateDecisionRequest), request => expectedResponse);

        // when
        var evaluatedDecisionResponse = await ZeebeClient.NewEvaluateDecisionCommand()
            .DecisionId("decision")
            .Send();

        // then
        Assert.AreEqual("decision", evaluatedDecisionResponse.DecisionId);
        Assert.AreEqual(123, evaluatedDecisionResponse.DecisionKey);
        Assert.AreEqual("decision-123", evaluatedDecisionResponse.DecisionName);
        Assert.AreEqual("1", evaluatedDecisionResponse.DecisionOutput);
        Assert.AreEqual(2, evaluatedDecisionResponse.DecisionVersion);
        Assert.AreEqual("", evaluatedDecisionResponse.FailureMessage);
        Assert.AreEqual("", evaluatedDecisionResponse.FailedDecisionId);
        Assert.AreEqual("12", evaluatedDecisionResponse.DecisionRequirementsId);
        Assert.AreEqual(1234, evaluatedDecisionResponse.DecisionRequirementsKey);

        var evaluatedDecisions = evaluatedDecisionResponse.EvaluatedDecisions;
        Assert.AreEqual(1, evaluatedDecisions.Count);

        var decision = evaluatedDecisions[0];
        Assert.AreEqual("decision", decision.DecisionId);
        Assert.AreEqual(123, decision.DecisionKey);
        Assert.AreEqual("decision-123", decision.DecisionName);
        Assert.AreEqual("1", decision.DecisionOutput);
        Assert.AreEqual(2, decision.DecisionVersion);
        Assert.AreEqual("noop", decision.DecisionType);

        var decisionEvaluatedInputs = decision.EvaluatedInputs;
        Assert.AreEqual(2, decisionEvaluatedInputs.Count);

        var decisionEvaluatedInput = decisionEvaluatedInputs[0];
        Assert.AreEqual("moep", decisionEvaluatedInput.InputId);
        Assert.AreEqual("moepmoep", decisionEvaluatedInput.InputName);
        Assert.AreEqual("boom", decisionEvaluatedInput.InputValue);

        decisionEvaluatedInput = decisionEvaluatedInputs[1];
        Assert.AreEqual("moeb", decisionEvaluatedInput.InputId);
        Assert.AreEqual("moebmoeb", decisionEvaluatedInput.InputName);
        Assert.AreEqual("boom", decisionEvaluatedInput.InputValue);

        var decisionMatchedRules = decision.MatchedRules;
        Assert.AreEqual(1, decisionMatchedRules.Count);
        var decisionMatchedRule = decisionMatchedRules[0];

        Assert.AreEqual("ruleid", decisionMatchedRule.RuleId);
        Assert.AreEqual(1, decisionMatchedRule.RuleIndex);

        var evaluatedDecisionOutputs = decisionMatchedRule.EvaluatedOutputs;
        Assert.AreEqual(2, evaluatedDecisionOutputs.Count);

        var evaluatedDecisionOutput = evaluatedDecisionOutputs[0];
        Assert.AreEqual("outputId", evaluatedDecisionOutput.OutputId);
        Assert.AreEqual("output", evaluatedDecisionOutput.OutputName);
        Assert.AreEqual("val", evaluatedDecisionOutput.OutputValue);

        evaluatedDecisionOutput = evaluatedDecisionOutputs[1];
        Assert.AreEqual("outputId2", evaluatedDecisionOutput.OutputId);
        Assert.AreEqual("output2", evaluatedDecisionOutput.OutputName);
        Assert.AreEqual("val2", evaluatedDecisionOutput.OutputValue);
    }

    [Test]
    public async Task ShouldSendRequestWithTenantIdAsExpected()
    {
        // given
        var expectedRequest = new EvaluateDecisionRequest
        {
            DecisionId = "decision",
            TenantId = "tenant1"
        };

    // when
        _ = await ZeebeClient.NewEvaluateDecisionCommand()
        .DecisionId("decision")
        .AddTenantId("tenant1")
        .Send();

        // then
        var request = TestService.Requests[typeof(EvaluateDecisionRequest)][0];
        Assert.AreEqual(expectedRequest, request);
    }
}