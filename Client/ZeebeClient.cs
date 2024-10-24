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
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Builder;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Builder;
using Zeebe.Client.Impl.Commands;
using Zeebe.Client.Impl.Misc;
using Zeebe.Client.Impl.Worker;

namespace Zeebe.Client;

/// <inheritdoc />
public sealed class ZeebeClient : IZeebeClient
{
    internal static readonly int MaxWaitTimeInSeconds = 60;
    internal static readonly Func<int, TimeSpan> DefaultWaitTimeProvider =
        retryAttempt => TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryAttempt), MaxWaitTimeInSeconds));
    private static readonly TimeSpan DefaultKeepAlive = TimeSpan.FromSeconds(30);

    private readonly GrpcChannel channelToGateway;
    private readonly ILoggerFactory? loggerFactory;
    private volatile Gateway.GatewayClient gatewayClient;
    private readonly IAsyncRetryStrategy asyncRetryStrategy;

    internal ZeebeClient(string address, TimeSpan? keepAlive, Func<int, TimeSpan>? sleepDurationProvider, ILoggerFactory? loggerFactory = null)
        : this(address, ChannelCredentials.Insecure, keepAlive, sleepDurationProvider, loggerFactory)
    { }

    internal ZeebeClient(string address,
        ChannelCredentials credentials,
        TimeSpan? keepAlive,
        Func<int, TimeSpan>? sleepDurationProvider,
        ILoggerFactory? loggerFactory = null,
        X509Certificate2? certificate = null,
        bool allowUntrusted = false)
    {
        this.loggerFactory = loggerFactory;
        var logger = loggerFactory?.CreateLogger<ZeebeClient>();
        logger?.LogDebug("Connect to {Address}", address);

        var sslOptions = new SslClientAuthenticationOptions
        {
            ClientCertificates = new X509Certificate2Collection(certificate is null ? Array.Empty<X509Certificate2>() : new X509Certificate2[] { certificate })
        };
        if (allowUntrusted)
        {
            // https://github.com/dotnet/runtime/issues/42482
            // https://docs.servicestack.net/grpc/csharp#c-protoc-grpc-ssl-example
            // Allows untrusted certificates, used for testing.
            sslOptions.RemoteCertificateValidationCallback = (_, _, _, _) => true;
        }

        channelToGateway = BuildChannelToGateway();

        var callInvoker = channelToGateway.Intercept(new UserAgentInterceptor());
        gatewayClient = new Gateway.GatewayClient(callInvoker);

        asyncRetryStrategy =
            new TransientGrpcErrorRetryStrategy(sleepDurationProvider ??
                                                DefaultWaitTimeProvider);

        GrpcChannel BuildChannelToGateway()
        {
            return GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                // https://learn.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/channel-credentials#combine-channelcredentials-and-callcredentials
                Credentials = credentials,
                LoggerFactory = this.loggerFactory,
                DisposeHttpClient = true,
                // for keep alive configure sockets http handler
                // https://learn.microsoft.com/en-us/aspnet/core/grpc/performance?view=aspnetcore-5.0#keep-alive-pings
                HttpHandler = new SocketsHttpHandler
                {
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = keepAlive.GetValueOrDefault(DefaultKeepAlive),
                    EnableMultipleHttp2Connections = true,
                    SslOptions = sslOptions
                },
            });
        }
    }

    public async Task Connect()
    {
        await channelToGateway.ConnectAsync();
    }

    /// <summary>
    ///     Intercept outgoing call to inject metadata
    ///     The "user-agent" is already filled by grpc-dotnet
    ///     typically something like: key=user-agent, value=grpc-dotnet/2.53.0 (.NET 7.0.5; CLR 7.0.5; net7.0; windows; x64)
    ///     We want to add our version and name. Unfortunately, this will always be appended to the end.
    /// </summary>
    private class UserAgentInterceptor : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var clientVersion = typeof(ZeebeClient).Assembly.GetName().Version;
            var userAgentString = $"zeebe-client-csharp/{clientVersion}";
            var headers = new Metadata
            {
                { "user-agent", userAgentString }
            };
            var newOptions = context.Options.WithHeaders(headers);
            var newContext =
                new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, newOptions);
            return base.AsyncUnaryCall(request, newContext, continuation);
        }
    }

    ////////////////////////////////////////////////////////////////////////
    ///////////////////////////// JOBS /////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////

    public IJobWorkerBuilderStep1 NewWorker()
    {
        return new JobWorkerBuilder(this, gatewayClient, loggerFactory);
    }

    public IActivateJobsCommandStep1 NewActivateJobsCommand()
    {
        return new ActivateJobsCommand(gatewayClient, asyncRetryStrategy);
    }

    public ICompleteJobCommandStep1 NewCompleteJobCommand(long jobKey)
    {
        return new CompleteJobCommand(gatewayClient, asyncRetryStrategy, jobKey);
    }

    public ICompleteJobCommandStep1 NewCompleteJobCommand(IJob activatedJob)
    {
        return new CompleteJobCommand(gatewayClient, asyncRetryStrategy, activatedJob.Key);
    }

    public IFailJobCommandStep1 NewFailCommand(long jobKey)
    {
        return new FailJobCommand(gatewayClient, asyncRetryStrategy, jobKey);
    }

    public IUpdateRetriesCommandStep1 NewUpdateRetriesCommand(long jobKey)
    {
        return new UpdateRetriesCommand(gatewayClient, asyncRetryStrategy, jobKey);
    }

    public IUpdateJobTimeoutCommandStep1 NewUpdateJobTimeoutCommand(long jobKey)
    {
        return new UpdateJobTimeoutCommand(gatewayClient, asyncRetryStrategy, jobKey);
    }

    public IThrowErrorCommandStep1 NewThrowErrorCommand(long jobKey)
    {
        return new ThrowErrorCommand(gatewayClient, asyncRetryStrategy, jobKey);
    }

    ////////////////////////////////////////////////////////////////////////
    ///////////////////////////// Processes ////////////////////////////////
    ////////////////////////////////////////////////////////////////////////

    public IDeployResourceCommandStep1 NewDeployCommand()
    {
        return new DeployResourceCommand(gatewayClient, asyncRetryStrategy);
    }

    public IEvaluateDecisionCommandStep1 NewEvaluateDecisionCommand()
    {
        return new EvaluateDecisionCommand(gatewayClient, asyncRetryStrategy);
    }

    public ICreateProcessInstanceCommandStep1 NewCreateProcessInstanceCommand()
    {
        return new CreateProcessInstanceCommand(gatewayClient, asyncRetryStrategy);
    }

    public ICancelProcessInstanceCommandStep1 NewCancelInstanceCommand(long processInstanceKey)
    {
        return new CancelProcessInstanceCommand(gatewayClient, asyncRetryStrategy, processInstanceKey);
    }

    public ISetVariablesCommandStep1 NewSetVariablesCommand(long elementInstanceKey)
    {
        return new SetVariablesCommand(gatewayClient, asyncRetryStrategy, elementInstanceKey);
    }

    public IResolveIncidentCommandStep1 NewResolveIncidentCommand(long incidentKey)
    {
        return new ResolveIncidentCommand(gatewayClient, asyncRetryStrategy, incidentKey);
    }

    public IPublishMessageCommandStep1 NewPublishMessageCommand()
    {
        return new PublishMessageCommand(gatewayClient, asyncRetryStrategy);
    }

    public IModifyProcessInstanceCommandStep1 NewModifyProcessInstanceCommand(long processInstanceKey)
    {
        return new ModifyProcessInstanceCommand(gatewayClient, asyncRetryStrategy, processInstanceKey);
    }

    public ITopologyRequestStep1 TopologyRequest() => new TopologyRequestCommand(gatewayClient, asyncRetryStrategy);

    public void Dispose()
    {
        if (gatewayClient is ClosedGatewayClient)
        {
            return;
        }

        gatewayClient = new ClosedGatewayClient();
        channelToGateway.Dispose();
    }

    /// <summary>
    /// Creates an new IZeebeClientBuilder. This builder need to be used to construct
    /// a ZeebeClient.
    /// </summary>
    /// <returns>an builder to construct an ZeebeClient.</returns>
    public static IZeebeClientBuilder Builder()
    {
        return new ZeebeClientBuilder();
    }
}