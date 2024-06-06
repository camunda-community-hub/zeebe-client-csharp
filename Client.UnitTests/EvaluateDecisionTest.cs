using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class EvaluateDecisionTest : BaseZeebeTest
    {
        [Test]
        public async Task ShouldSendRequestAsExpected()
        {
            // given
            var expectedRequest = new EvaluateDecisionRequest
            {
                DecisionId = "decision",
            };

            // when
            await ZeebeClient.NewEvaluateDecisionCommand()
                .DecisionId("decision")
                .Send();

            // then
            var request = TestService.Requests[typeof(EvaluateDecisionRequest)][0];
            Assert.Equals(expectedRequest, request);
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
            Assert.Equals(Grpc.Core.StatusCode.DeadlineExceeded, rpcException.Status.StatusCode);
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
            Assert.Equals(Grpc.Core.StatusCode.Cancelled, rpcException.Status.StatusCode);
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
            await ZeebeClient.NewEvaluateDecisionCommand()
                .DecisionKey(12)
                .Send();

            // then
            var request = TestService.Requests[typeof(EvaluateDecisionRequest)][0];
            Assert.Equals(expectedRequest, request);
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
            await ZeebeClient.NewEvaluateDecisionCommand()
                .DecisionKey(12)
                .Variables("{\"foo\":1}")
                .Send();

            // then
            var request = TestService.Requests[typeof(EvaluateDecisionRequest)][0];
            Assert.Equals(expectedRequest, request);
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
            await ZeebeClient.NewEvaluateDecisionCommand()
                .DecisionId("decision")
                .Variables("{\"foo\":1}")
                .Send();

            // then
            var request = TestService.Requests[typeof(EvaluateDecisionRequest)][0];
            Assert.Equals(expectedRequest, request);
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
                },
            };

            TestService.AddRequestHandler(typeof(EvaluateDecisionRequest), request => expectedResponse);

            // when
            var evaluatedDecisionResponse = await ZeebeClient.NewEvaluateDecisionCommand()
                .DecisionId("decision")
                .Send();

            // then
            Assert.Equals("decision", evaluatedDecisionResponse.DecisionId);
            Assert.Equals(123, evaluatedDecisionResponse.DecisionKey);
            Assert.Equals("decision-123", evaluatedDecisionResponse.DecisionName);
            Assert.Equals("1", evaluatedDecisionResponse.DecisionOutput);
            Assert.Equals(2, evaluatedDecisionResponse.DecisionVersion);
            Assert.Equals("", evaluatedDecisionResponse.FailureMessage);
            Assert.Equals("", evaluatedDecisionResponse.FailedDecisionId);
            Assert.Equals("12", evaluatedDecisionResponse.DecisionRequirementsId);
            Assert.Equals(1234, evaluatedDecisionResponse.DecisionRequirementsKey);

            var evaluatedDecisions = evaluatedDecisionResponse.EvaluatedDecisions;
            Assert.Equals(1, evaluatedDecisions.Count);

            var decision = evaluatedDecisions[0];
            Assert.Equals("decision", decision.DecisionId);
            Assert.Equals(123, decision.DecisionKey);
            Assert.Equals("decision-123", decision.DecisionName);
            Assert.Equals("1", decision.DecisionOutput);
            Assert.Equals(2, decision.DecisionVersion);
            Assert.Equals("noop", decision.DecisionType);

            var decisionEvaluatedInputs = decision.EvaluatedInputs;
            Assert.Equals(2, decisionEvaluatedInputs.Count);

            var decisionEvaluatedInput = decisionEvaluatedInputs[0];
            Assert.Equals("moep", decisionEvaluatedInput.InputId);
            Assert.Equals("moepmoep", decisionEvaluatedInput.InputName);
            Assert.Equals("boom", decisionEvaluatedInput.InputValue);

            decisionEvaluatedInput = decisionEvaluatedInputs[1];
            Assert.Equals("moeb", decisionEvaluatedInput.InputId);
            Assert.Equals("moebmoeb", decisionEvaluatedInput.InputName);
            Assert.Equals("boom", decisionEvaluatedInput.InputValue);


            var decisionMatchedRules = decision.MatchedRules;
            Assert.Equals(1, decisionMatchedRules.Count);
            var decisionMatchedRule = decisionMatchedRules[0];

            Assert.Equals("ruleid", decisionMatchedRule.RuleId);
            Assert.Equals(1, decisionMatchedRule.RuleIndex);

            var evaluatedDecisionOutputs = decisionMatchedRule.EvaluatedOutputs;
            Assert.Equals(2, evaluatedDecisionOutputs.Count);

            var evaluatedDecisionOutput = evaluatedDecisionOutputs[0];
            Assert.Equals("outputId", evaluatedDecisionOutput.OutputId);
            Assert.Equals("output", evaluatedDecisionOutput.OutputName);
            Assert.Equals("val", evaluatedDecisionOutput.OutputValue);

            evaluatedDecisionOutput = evaluatedDecisionOutputs[1];
            Assert.Equals("outputId2", evaluatedDecisionOutput.OutputId);
            Assert.Equals("output2", evaluatedDecisionOutput.OutputName);
            Assert.Equals("val2", evaluatedDecisionOutput.OutputValue);
        }
    }
}



