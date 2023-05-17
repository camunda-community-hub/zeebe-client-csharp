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

using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
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
using Zeebe.Client.Impl;
using Zeebe.Client.Impl.Builder;
using Zeebe.Client.Impl.Commands;
using Zeebe.Client.Impl.Misc;
using Zeebe.Client.Impl.proto;
using Zeebe.Client.Impl.Worker;

namespace Zeebe.Client;

/// <inheritdoc />
public sealed class ZeebeClient : IZeebeClient
{
    internal static readonly int MaxWaitTimeInSeconds = 60;

    internal static readonly Func<int, TimeSpan> DefaultWaitTimeProvider =
        retryAttempt => TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryAttempt), MaxWaitTimeInSeconds));

    private static readonly TimeSpan DefaultKeepAlive = TimeSpan.FromSeconds(30);
    private readonly IAsyncRetryStrategy asyncRetryStrategy;

    private readonly GrpcChannel channelToGateway;
    private readonly ILoggerFactory loggerFactory;
    private Gateway.GatewayClient gatewayClient;

    internal ZeebeClient(string address, TimeSpan? keepAlive, Func<int, TimeSpan> sleepDurationProvider,
        ILoggerFactory loggerFactory = null)
        : this(address, ChannelCredentials.Insecure, keepAlive, sleepDurationProvider, loggerFactory)
    {
    }

    internal ZeebeClient(string address,
        ChannelCredentials credentials,
        TimeSpan? keepAlive,
        Func<int, TimeSpan> sleepDurationProvider,
        ILoggerFactory loggerFactory = null,
        X509Certificate2 certificate = null)
    {
        this.loggerFactory = loggerFactory;

        var logger = loggerFactory?.CreateLogger<ZeebeClient>();
        logger?.LogDebug("Connect to {Address}", address);

        // Keep alive pings
        // https://learn.microsoft.com/en-us/aspnet/core/grpc/performance?view=aspnetcore-5.0#keep-alive-pings
        var handler = new SocketsHttpHandler
        {
            PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
            KeepAlivePingDelay = keepAlive ?? DefaultKeepAlive,
            KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
            EnableMultipleHttp2Connections = true
        };
        handler.SslOptions = new SslClientAuthenticationOptions
        {
            ClientCertificates = new X509CertificateCollection(GetCertificates()),
            // allow untrusted certificate
            RemoteCertificateValidationCallback = (_, _, _, _) => true
        };

        var options = new GrpcChannelOptions
        {
            HttpHandler = handler,
            DisposeHttpClient = true,
            LoggerFactory = this.loggerFactory,
            Credentials = credentials
        };

        channelToGateway = GrpcChannel.ForAddress(address, options);
        var invoker = channelToGateway.Intercept(new CustomHeaderInterceptor());
        gatewayClient = new Gateway.GatewayClient(invoker);

        asyncRetryStrategy =
            new TransientGrpcErrorRetryStrategy(sleepDurationProvider ?? DefaultWaitTimeProvider);

        X509Certificate[] GetCertificates()
        {
            if (certificate is null)
            {
                return Array.Empty<X509Certificate>();
            }

            return new X509Certificate[] { certificate };
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

    public IThrowErrorCommandStep1 NewThrowErrorCommand(long jobKey)
    {
        return new ThrowErrorCommand(gatewayClient, asyncRetryStrategy, jobKey);
    }

    ////////////////////////////////////////////////////////////////////////
    ///////////////////////////// Processes ////////////////////////////////
    ////////////////////////////////////////////////////////////////////////

    public IDeployProcessCommandStep1 NewDeployCommand()
    {
        return new DeployProcessCommand(gatewayClient, asyncRetryStrategy);
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

    public ITopologyRequestStep1 TopologyRequest()
    {
        return new TopologyRequestCommand(gatewayClient, asyncRetryStrategy);
    }

    public void Dispose()
    {
        if (gatewayClient is ClosedGatewayClient)
        {
            return;
        }

        gatewayClient = new ClosedGatewayClient();
        channelToGateway.ShutdownAsync().Wait();
        channelToGateway.Dispose();
    }

    /// <summary>
    ///     Creates an new IZeebeClientBuilder. This builder need to be used to construct
    ///     a ZeebeClient.
    /// </summary>
    /// <returns>an builder to construct an ZeebeClient.</returns>
    public static IZeebeClientBuilder Builder()
    {
        return new ZeebeClientBuilder();
    }
}