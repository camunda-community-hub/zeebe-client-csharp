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
using GatewayProtocol;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zeebe.Client
{
    public delegate IMessage RequestHandler(IMessage request);

    public class GatewayTestService : Gateway.GatewayBase
    {

        private readonly List<IMessage> requests = new List<IMessage>();

        /**
         * Contains the request handler, which return the specific response, for each specific type of request.
         *
         * Per default the response for a specific type are an empty response.
         */
        private readonly Dictionary<Type, RequestHandler> typedRequestHandler = new Dictionary<Type, RequestHandler>();

        public IList<IMessage> Requests => requests;

        public GatewayTestService()
        {
            typedRequestHandler.Add(typeof(TopologyRequest), request => new TopologyResponse());

            typedRequestHandler.Add(typeof(PublishMessageRequest), request => new PublishMessageResponse());

            typedRequestHandler.Add(typeof(ActivateJobsRequest), request => new ActivateJobsResponse());
            typedRequestHandler.Add(typeof(CompleteJobRequest), request => new CompleteJobResponse());
            typedRequestHandler.Add(typeof(FailJobRequest), request => new FailJobResponse());
            typedRequestHandler.Add(typeof(UpdateJobRetriesRequest), request => new UpdateJobRetriesResponse());

            typedRequestHandler.Add(typeof(DeployWorkflowRequest), request => new DeployWorkflowResponse());
            typedRequestHandler.Add(typeof(CreateWorkflowInstanceRequest), request => new CreateWorkflowInstanceResponse());
            typedRequestHandler.Add(typeof(CancelWorkflowInstanceRequest), request => new CancelWorkflowInstanceResponse());
            typedRequestHandler.Add(typeof(UpdateWorkflowInstancePayloadRequest), request => new UpdateWorkflowInstancePayloadResponse());
            typedRequestHandler.Add(typeof(ResolveIncidentRequest), request => new ResolveIncidentResponse());

            typedRequestHandler.Add(typeof(ListWorkflowsRequest), request => new ListWorkflowsResponse());
            typedRequestHandler.Add(typeof(GetWorkflowRequest), request => new GetWorkflowResponse());
        }

        public void AddRequestHandler(Type requestType, RequestHandler requestHandler) => typedRequestHandler[requestType] = requestHandler;

        //
        // overwrite base methods to handle requests
        //

        public override Task<TopologyResponse> Topology(TopologyRequest request, ServerCallContext context)
        {
            return Task.FromResult((TopologyResponse)HandleRequest(request, context));
        }

        public override Task<PublishMessageResponse> PublishMessage(PublishMessageRequest request, ServerCallContext context)
        {
            return Task.FromResult((PublishMessageResponse)HandleRequest(request, context));
        }

        public override async Task ActivateJobs(ActivateJobsRequest request, IServerStreamWriter<ActivateJobsResponse> responseStream, ServerCallContext context)
        {
            await responseStream.WriteAsync((ActivateJobsResponse)HandleRequest(request, context));
        }

        public override Task<CompleteJobResponse> CompleteJob(CompleteJobRequest request, ServerCallContext context)
        {
            return Task.FromResult((CompleteJobResponse)HandleRequest(request, context));
        }

        public override Task<FailJobResponse> FailJob(FailJobRequest request, ServerCallContext context)
        {
            return Task.FromResult((FailJobResponse)HandleRequest(request, context));
        }

        public override Task<UpdateJobRetriesResponse> UpdateJobRetries(UpdateJobRetriesRequest request, ServerCallContext context)
        {
            return Task.FromResult((UpdateJobRetriesResponse)HandleRequest(request, context));
        }

        public override Task<DeployWorkflowResponse> DeployWorkflow(DeployWorkflowRequest request, ServerCallContext context)
        {
            return Task.FromResult((DeployWorkflowResponse) HandleRequest(request, context));
        }

        public override Task<CreateWorkflowInstanceResponse> CreateWorkflowInstance(CreateWorkflowInstanceRequest request, ServerCallContext context)
        {
            return Task.FromResult((CreateWorkflowInstanceResponse) HandleRequest(request, context));
        }

        public override Task<CancelWorkflowInstanceResponse> CancelWorkflowInstance(CancelWorkflowInstanceRequest request, ServerCallContext context)
        {
            return Task.FromResult((CancelWorkflowInstanceResponse) HandleRequest(request, context));
        }

        public override Task<UpdateWorkflowInstancePayloadResponse> UpdateWorkflowInstancePayload(UpdateWorkflowInstancePayloadRequest request, ServerCallContext context)
        {
            return Task.FromResult((UpdateWorkflowInstancePayloadResponse) HandleRequest(request, context));
        }

        public override Task<ResolveIncidentResponse> ResolveIncident(ResolveIncidentRequest request, ServerCallContext context)
        {
            return Task.FromResult((ResolveIncidentResponse) HandleRequest(request, context));
        }

        public override Task<ListWorkflowsResponse> ListWorkflows(ListWorkflowsRequest request, ServerCallContext context)
        {
            return Task.FromResult((ListWorkflowsResponse) HandleRequest(request, context));
        }

        public override Task<GetWorkflowResponse> GetWorkflow(GetWorkflowRequest request, ServerCallContext context)
        {
            return Task.FromResult((GetWorkflowResponse) HandleRequest(request, context));
        }

        private IMessage HandleRequest(IMessage request, ServerCallContext context)
        {
            requests.Add(request);

            var handler = typedRequestHandler[request.GetType()];
            return handler.Invoke(request);
        }
    }
}
