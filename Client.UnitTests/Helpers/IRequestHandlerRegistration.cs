#nullable enable
using System;
using System.Collections.Generic;
using Google.Protobuf;

namespace Zeebe.Client.Helpers;

public interface IRequestHandlerRegistration
{
    IDictionary<Type, IList<IMessage>> Requests { get; }
    RequestHandlerRegistration.ConsumeMetadata? MetadataConsumer { get; }
    void AddRequestHandler<TRequestType>(RequestHandler requestHandler, bool reset = true);
    IMessage? HandleRequest(IMessage request);
    void Reset();
    void ConsumeRequestHeaders(RequestHandlerRegistration.ConsumeMetadata consumer);
}