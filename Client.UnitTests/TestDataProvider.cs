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
                    new UpdateRetries
                    {
                        ProcessInstanceKey = 12113
                    }, new UpdateRetriesResponse()
                    { RequestCreator<IUpdateRetries>}
                    (ZeebeClientTest => ZeebeClient.NewUpdateRetriesCommand()));
                yield return new TestCaseData(
                    new ThrowErrorRequest()
                    {
                        ProcessInstanceKey = 12113
                    }, new ThrowErrorResponse()
                    { RequestCreator<IThrowErrorCommand>}
                    (ZeebeClientTest => ZeebeClient.NewThrowErrorCommand()));
            }

    }
}}