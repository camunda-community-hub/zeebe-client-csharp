#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GatewayProtocol;
using Google.Protobuf;
using Grpc.Core;

namespace Zeebe.Client.Helpers;

public class RequestHandlerRegistration : IRequestHandlerRegistration
{
    public delegate void ConsumeMetadata(Metadata metadata);

    private readonly Dictionary<Type, RequestHandler> defaultTypedRequestHandler = new ()
    {
        { typeof(TopologyRequest), _ => new TopologyResponse() },
        { typeof(PublishMessageRequest), _ => new PublishMessageResponse() },
        { typeof(ActivateJobsRequest), _ => new ActivateJobsResponse() },
        { typeof(CompleteJobRequest), _ => new CompleteJobResponse() },
        { typeof(FailJobRequest), _ => new FailJobResponse() },
        { typeof(UpdateJobRetriesRequest), _ => new UpdateJobRetriesResponse() },
        { typeof(ThrowErrorRequest), _ => new ThrowErrorResponse() },
        { typeof(DeployProcessRequest), _ => new DeployProcessResponse() },
        { typeof(CreateProcessInstanceRequest), _ => new CreateProcessInstanceResponse() },
        { typeof(CancelProcessInstanceRequest), _ => new CancelProcessInstanceResponse() },
        { typeof(SetVariablesRequest), _ => new SetVariablesResponse() },
        { typeof(ResolveIncidentRequest), _ => new ResolveIncidentResponse() },
        { typeof(CreateProcessInstanceWithResultRequest), _ => new CreateProcessInstanceWithResultResponse() }
    };

    private Dictionary<Type, RequestHandler> typedRequestHandler = null!;

    public RequestHandlerRegistration()
    {
        Reset();
    }

    public ConsumeMetadata? MetadataConsumer { get; private set; }

    public IDictionary<Type, IList<IMessage>> Requests { get; } = new ConcurrentDictionary<Type, IList<IMessage>>();

    public void AddRequestHandler<TRequestType>(RequestHandler requestHandler, bool reset = true)
    {
        if (reset)
        {
            Reset();
        }

        typedRequestHandler[typeof(TRequestType)] = requestHandler;
    }

    public IMessage HandleRequest(IMessage request)
    {
        Requests[request.GetType()].Add(request);

        var handler = typedRequestHandler[request.GetType()];
        return handler.Invoke(request);
    }

    public void Reset()
    {
        typedRequestHandler = new Dictionary<Type, RequestHandler>(defaultTypedRequestHandler);
        foreach (var pair in typedRequestHandler)
        {
            Requests[pair.Key] = new List<IMessage>();
        }
    }

    public void ConsumeRequestHeaders(ConsumeMetadata consumer)
    {
        MetadataConsumer = consumer;
    }
}