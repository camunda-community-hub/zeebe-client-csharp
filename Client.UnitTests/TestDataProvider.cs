using System.Collections.Generic;
using GatewayProtocol;
using NUnit.Framework;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;
using CancelProcessInstanceResponse = GatewayProtocol.CancelProcessInstanceResponse;
using CompleteJobResponse = GatewayProtocol.CompleteJobResponse;

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
                        JobKey = 12113
                    }, new UpdateRetriesResponse(),
                    (RequestCreator<IUpdateRetriesResponse>)
                    (zeebeClient => zeebeClient.NewUpdateRetriesCommand(12113L).Retries(1)));
                yield return new TestCaseData(
                    new SetVariablesRequest(),
                    new GatewayProtocol.SetVariablesResponse(),
                    (RequestCreator<ISetVariablesResponse>)
                    (zeebeClient => zeebeClient.NewSetVariablesCommand(2123).Variables("{\"foo\":\"bar\"}")));
                yield return new TestCaseData(
                    new ThrowErrorRequest()
                    {
                        JobKey = 12113
                    }, new GatewayProtocol.ThrowErrorResponse(),
                    (RequestCreator<IThrowErrorResponse>)
                    (zeebeClient => zeebeClient.NewThrowErrorCommand(12113).ErrorCode("Code 13").ErrorMessage("This is a business error!")));
                yield return new TestCaseData(
                    new ResolveIncidentRequest
                    {
                        IncidentKey = 1201321
                    }, new GatewayProtocol.ResolveIncidentResponse(),
                    (RequestCreator<IResolveIncidentResponse>)
                    (zeebeClient => zeebeClient.NewResolveIncidentCommand(1201321)));
        }
    }
}