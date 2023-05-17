using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Zeebe.Client.Impl
{
    /// <summary>
    ///     Intercept outgoing call to inject metadata
    ///     The "user-agent" is already filled by grpc-dotnet
    ///     typically something like: key=user-agent, value=grpc-dotnet/2.53.0 (.NET 7.0.5; CLR 7.0.5; net7.0; windows; x64)
    /// </summary>
    internal class CustomHeaderInterceptor : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var headers = new Metadata
            {
                { "client", $"csharp/{typeof(ZeebeClient).Assembly.GetName().Version}" }
            };
            var newOptions = context.Options.WithHeaders(headers);
            var newContext =
                new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, newOptions);
            return base.AsyncUnaryCall(request, newContext, continuation);
        }
    }
}