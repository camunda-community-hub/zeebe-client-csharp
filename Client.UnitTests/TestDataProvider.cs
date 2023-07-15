using System;
using System.Collections.Generic;
using System.IO;
using GatewayProtocol;
using Google.Protobuf;
using NUnit.Framework;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;
using CancelProcessInstanceResponse = GatewayProtocol.CancelProcessInstanceResponse;
using CompleteJobResponse = GatewayProtocol.CompleteJobResponse;
using FailJobResponse = GatewayProtocol.FailJobResponse;
using PublishMessageResponse = GatewayProtocol.PublishMessageResponse;
using ResolveIncidentResponse = GatewayProtocol.ResolveIncidentResponse;
using SetVariablesResponse = GatewayProtocol.SetVariablesResponse;
using ThrowErrorResponse = GatewayProtocol.ThrowErrorResponse;

namespace Zeebe.Client
{
    public delegate IFinalCommandWithRetryStep<T> RequestCreator<T>(IZeebeClient zeebeClient);

    public class TestDataProvider
    {
        private static readonly string DemoProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "demo-process.bpmn");

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
                new ActivateJobsRequest
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
                (zeebeClient => zeebeClient.NewThrowErrorCommand(12113).ErrorCode("Code 13")
                    .ErrorMessage("This is a business error!")));
            yield return new TestCaseData(
                new PublishMessageRequest
                {
                    Name = "messageName",
                    CorrelationKey = "p-1"
                },
                new PublishMessageResponse(),
                (RequestCreator<IPublishMessageResponse>)
                (zeebeClient =>
                    zeebeClient.NewPublishMessageCommand().MessageName("messageName").CorrelationKey("p-1")));
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
                    BpmnProcessId = "process",
                    Version = -1
                },
                new CreateProcessInstanceResponse(),
                (RequestCreator<IProcessInstanceResponse>)
                (zeebeClient =>
                    zeebeClient.NewCreateProcessInstanceCommand().BpmnProcessId("process").LatestVersion()));
            yield return new TestCaseData(
                new CreateProcessInstanceWithResultRequest
                {
                    Request = new CreateProcessInstanceRequest
                    {
                        BpmnProcessId = "process",
                        Version = -1
                    },
                    RequestTimeout = 20000
                },
                new CreateProcessInstanceWithResultResponse(),
                (RequestCreator<IProcessInstanceResult>)
                (zeebeClient => zeebeClient.NewCreateProcessInstanceCommand().BpmnProcessId("process").LatestVersion().WithResult()));
            yield return new TestCaseData(
                new FailJobRequest
                {
                    JobKey = 255,
                    Retries = 1
                },
                new FailJobResponse(),
                (RequestCreator<IFailJobResponse>)
                (zeebeClient => zeebeClient.NewFailCommand(255L).Retries(1)));
            yield return new TestCaseData(
                new DeployResourceRequest
                {
                    Resources =
                    {
                        new Resource
                        {
                            Content = ByteString.FromStream(File.OpenRead(DemoProcessPath)),
                            Name = DemoProcessPath
                        }
                    }
                },
                new GatewayProtocol.DeployResourceResponse(),
                (RequestCreator<IDeployResourceResponse>)
                (zeebeClient => zeebeClient.NewDeployCommand().AddResourceFile(DemoProcessPath)));
        }
    }
}