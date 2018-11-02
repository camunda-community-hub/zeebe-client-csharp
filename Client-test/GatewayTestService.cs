using System;
using System.Collections.Generic;
using Google.Protobuf;
using System.Threading.Tasks;

namespace Zeebe.Client
{
    public class GatewayTestService : GatewayProtocol.Gateway.GatewayBase
    {
        
        private readonly List<IMessage> requests = new List<IMessage>();

        /**
         * Contains the request handler, which return the specific response, for each specific type of request.
         * 
         * Per default the response for a specific type are an empty response.
         */
        private readonly Dictionary<Type, RequestHandler> typedRequestHandler = new Dictionary<Type, RequestHandler>();

        delegate IMessage RequestHandler(IMessage request);

        public IList<IMessage> Requests { get { return requests; } }

        public GatewayTestService()
        {
            typedRequestHandler.Add(typeof(GatewayProtocol.TopologyRequest), (request) => new GatewayProtocol.TopologyResponse());
        }

        //
        // overwrite base methods to handle requests
        //

        public override System.Threading.Tasks.Task<GatewayProtocol.TopologyResponse> Topology(GatewayProtocol.TopologyRequest request, Grpc.Core.ServerCallContext context)
        {
            return Task.FromResult((GatewayProtocol.TopologyResponse) HandleRequest(request, context));
        }


        private IMessage HandleRequest(IMessage request, Grpc.Core.ServerCallContext context)
        {
            requests.Add(request);

            RequestHandler handler = typedRequestHandler[request.GetType()];
            return handler.Invoke(request);
        }
    }
}
