using System.Collections.Generic;
using GatewayProtocol;
using NUnit.Framework;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client
{
    public delegate IFinalCommandWithRetryStep<T> RequestCreator<T>(IZeebeClient zeebeClient);
    public class TestDataProvider
    {
        public static IEnumerable<TestCaseData> Provider()
            {
                yield return new TestCaseData(
                    new CancelProcessInstanceRequest
                    {
                        ProcessInstanceKey = 12113
                    }, new CancelProcessInstanceResponse(),
                    (RequestCreator<ICancelProcessInstanceResponse>)
                    (zeebeClient => zeebeClient.NewCancelInstanceCommand(12113)));
                yield return new TestCaseData(
                    new CompleteJobRequest
                    {
                        JobKey = 12113
                    }, new CompleteJobResponse(),
                    (RequestCreator<ICompleteJobResponse>)
                    (zeebeClient => zeebeClient.NewCompleteJobCommand(12113)));
                yield return new TestCaseData(
                    new ActivateJobsRequest()
                    {
                        Type = "type",
                        MaxJobsToActivate = 12
                    }, new ActivateJobsResponse(),
                    (RequestCreator<IActivateJobsResponse>)
                    (zeebeClient => zeebeClient.NewActivateJobsCommand().JobType("type").MaxJobsToActivate(12)));
                yield return new TestCaseData(
                    new TopologyRequest(),
                    new TopologyResponse(),
                    (RequestCreator<ITopology>)
                    (zeebeClient => zeebeClient.TopologyRequest()));
                yield return new TestCaseData(
                    new UpdateJobRetriesRequest
                    {
                        JobKey = 12113L,
                        Retries = 1
                    }, new UpdateJobRetriesResponse(),
                    (RequestCreator<IUpdateRetriesResponse>)
                    (zeebeClient => zeebeClient.NewUpdateRetriesCommand(12113L).Retries(1)));
                yield return new TestCaseData(
                    new SetVariablesRequest
                    {
                        ElementInstanceKey = 2123,
                        Variables = "{\"foo\":\"bar\"}"
                    },
                    new SetVariablesResponse(),
                    (RequestCreator<ISetVariablesResponse>)
                    (zeebeClient => zeebeClient.NewSetVariablesCommand(2123).Variables("{\"foo\":\"bar\"}")));
                yield return new TestCaseData(
                    new ThrowErrorRequest
                    {
                        JobKey = 12113,
                        ErrorCode = "Code 13",
                        ErrorMessage = "This is a business error!"
                    }, new ThrowErrorResponse(),
                    (RequestCreator<IThrowErrorResponse>)
                    (zeebeClient => zeebeClient.NewThrowErrorCommand(12113).ErrorCode("Code 13").ErrorMessage("This is a business error!")));
                yield return new TestCaseData(
                    new PublishMessageRequest
                    {
                        Name = "messageName",
                        CorrelationKey = "p-1"
                    },
                    new PublishMessageResponse(),
                    (RequestCreator<IPublishMessageResponse>)
                    (zeebeClient => zeebeClient.NewPublishMessageCommand().MessageName("messageName").CorrelationKey("p-1")));
                yield return new TestCaseData(
                    new ResolveIncidentRequest
                    {
                        IncidentKey = 1201321
                    }, new ResolveIncidentResponse(),
                    (RequestCreator<IResolveIncidentResponse>)
                    (zeebeClient => zeebeClient.NewResolveIncidentCommand(1201321)));
                yield return new TestCaseData(
                    new CreateProcessInstanceRequest
                    {
                        BpmnProcessId = "Process"
                    },
                    new CreateProcessInstanceResponse(),
                    (RequestCreator<ICreateProcessInstanceCommandStep3>)
                    (zeebeClient => (IFinalCommandWithRetryStep<ICreateProcessInstanceCommandStep3>)zeebeClient.NewCreateProcessInstanceCommand().BpmnProcessId("process").LatestVersion()));
                yield return new TestCaseData(
                    new CreateProcessInstanceWithResultRequest(),
                    new CreateProcessInstanceWithResultResponse(),
                    (RequestCreator<ICreateProcessInstanceWithResultCommandStep1>)
                    (zeebeClient => (IFinalCommandWithRetryStep<ICreateProcessInstanceWithResultCommandStep1>)zeebeClient.NewCreateProcessInstanceCommand().BpmnProcessId("process").LatestVersion().WithResult()));
                }
    }
}