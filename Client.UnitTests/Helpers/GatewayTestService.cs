//
//    Copyright (c) 2018 camunda services GmbH (info@camunda.com)
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#nullable enable
using System;
using System.Threading.Tasks;
using GatewayProtocol;
using Google.Protobuf;
using Grpc.Core;
using NLog;

namespace Zeebe.Client.Helpers;

public delegate IMessage RequestHandler(IMessage request);

public class GatewayTestService : Gateway.GatewayBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IRequestHandlerRegistration registration;

    public GatewayTestService(IRequestHandlerRegistration registration)
    {
        this.registration = registration;
    }

    //
    // overwrite base methods to handle requests
    //

    public override async Task<TopologyResponse?> Topology(TopologyRequest request, ServerCallContext context)
    {
        return (TopologyResponse?) await HandleRequest(request, context);
    }

    public override async Task<PublishMessageResponse?> PublishMessage(PublishMessageRequest request,
        ServerCallContext context)
    {
        return (PublishMessageResponse?) await HandleRequest(request, context);
    }

    public override async Task ActivateJobs(ActivateJobsRequest request,
        IServerStreamWriter<ActivateJobsResponse?> responseStream, ServerCallContext context)
    {
        await responseStream.WriteAsync((ActivateJobsResponse?) await HandleRequest(request, context));
    }

    public override async Task<CompleteJobResponse?> CompleteJob(CompleteJobRequest request, ServerCallContext context)
    {
        return (CompleteJobResponse?) await HandleRequest(request, context);
    }

    public override async Task<FailJobResponse?> FailJob(FailJobRequest request, ServerCallContext context)
    {
        return (FailJobResponse?) await HandleRequest(request, context);
    }

    public override async Task<UpdateJobRetriesResponse?> UpdateJobRetries(UpdateJobRetriesRequest request,
        ServerCallContext context)
    {
        return (UpdateJobRetriesResponse?) await HandleRequest(request, context);
    }

    public override async Task<ThrowErrorResponse?> ThrowError(ThrowErrorRequest request, ServerCallContext context)
    {
        return (ThrowErrorResponse?) await HandleRequest(request, context);
    }

    public override async Task<DeployProcessResponse?> DeployProcess(DeployProcessRequest request,
        ServerCallContext context)
    {
        return (DeployProcessResponse?) await HandleRequest(request, context);
    }

    public override async Task<CreateProcessInstanceResponse?> CreateProcessInstance(
        CreateProcessInstanceRequest request,
        ServerCallContext context)
    {
        return (CreateProcessInstanceResponse?) await HandleRequest(request, context);
    }

    public override async Task<CancelProcessInstanceResponse?> CancelProcessInstance(
        CancelProcessInstanceRequest request,
        ServerCallContext context)
    {
        return (CancelProcessInstanceResponse?) await HandleRequest(request, context);
    }

    public override async Task<SetVariablesResponse?> SetVariables(SetVariablesRequest request,
        ServerCallContext context)
    {
        return (SetVariablesResponse?) await HandleRequest(request, context);
    }

    public override async Task<ResolveIncidentResponse?> ResolveIncident(ResolveIncidentRequest request,
        ServerCallContext context)
    {
        return (ResolveIncidentResponse?) await HandleRequest(request, context);
    }

    public override async Task<CreateProcessInstanceWithResultResponse?> CreateProcessInstanceWithResult(
        CreateProcessInstanceWithResultRequest request, ServerCallContext context)
    {
        return (CreateProcessInstanceWithResultResponse?) await HandleRequest(request, context);
    }

    private async Task<IMessage?> HandleRequest(IMessage request, ServerCallContext context)
    {
        registration.MetadataConsumer?.Invoke(context.RequestHeaders);
        if ((context.Deadline - DateTime.UtcNow).Milliseconds <= 1000)
        {
            // when we have set a timeout in the test we want GRPC to exceed the deadline
            var contextDeadline = DateTime.UtcNow - context.Deadline;
            await Task.Delay(contextDeadline.Milliseconds + 1000);
        }

        Logger.Debug("Received request '{0}'", request.GetType());

        return registration.HandleRequest(request);
    }
}