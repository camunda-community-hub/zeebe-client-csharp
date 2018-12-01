using System;
using System.Collections.Generic;
using Google.Protobuf;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;

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
            typedRequestHandler.Add(typeof(ActivateJobsRequest), request => new ActivateJobsResponse());
            typedRequestHandler.Add(typeof(CompleteJobResponse), request => new CompleteJobResponse());
        }

        public void AddRequestHandler(Type requestType, RequestHandler requestHandler) => typedRequestHandler[requestType] = requestHandler;

        //
        // overwrite base methods to handle requests
        //

        public override Task<TopologyResponse> Topology(TopologyRequest request, ServerCallContext context)
        {
            return Task.FromResult((TopologyResponse) HandleRequest(request, context));
        }

        public override async Task ActivateJobs(ActivateJobsRequest request, IServerStreamWriter<ActivateJobsResponse> responseStream, ServerCallContext context)
        {
            await responseStream.WriteAsync((ActivateJobsResponse) HandleRequest(request, context));
        }

        public override Task<CompleteJobResponse> CompleteJob(CompleteJobRequest request, ServerCallContext context)
        {
            return Task.FromResult((CompleteJobResponse) HandleRequest(request, context));
        }

        private IMessage HandleRequest(IMessage request, ServerCallContext context)
        {
            requests.Add(request);

            var handler = typedRequestHandler[request.GetType()];
            return handler.Invoke(request);
        }
    }
}
