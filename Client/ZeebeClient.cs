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
using System.Collections.Generic;
using GatewayProtocol;
using Grpc.Core;
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

namespace Zeebe.Client
{
    /// <inheritdoc />
    public class ZeebeClient : IZeebeClient
    {
        internal static readonly int MaxWaitTimeInSeconds = 60;
        internal static readonly Func<int, TimeSpan> DefaultWaitTimeProvider =
            retryAttempt => TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryAttempt), MaxWaitTimeInSeconds));
        private static readonly TimeSpan DefaultKeepAlive = TimeSpan.FromSeconds(30);

        private readonly Channel channelToGateway;
        private readonly ILoggerFactory loggerFactory;
        private Gateway.GatewayClient gatewayClient;
        private readonly IAsyncRetryStrategy asyncRetryStrategy;

        internal ZeebeClient(string address, TimeSpan? keepAlive, Func<int, TimeSpan> sleepDurationProvider, ILoggerFactory loggerFactory = null)
            : this(address, ChannelCredentials.Insecure, keepAlive, sleepDurationProvider, loggerFactory)
        { }

        internal ZeebeClient(string address,
            ChannelCredentials credentials,
            TimeSpan? keepAlive,
            Func<int, TimeSpan> sleepDurationProvider,
            ILoggerFactory loggerFactory = null)
        {
            this.loggerFactory = loggerFactory;

            var logger = loggerFactory?.CreateLogger<ZeebeClient>();
            logger?.LogDebug("Connect to {Address}", address);

            var channelOptions = new List<ChannelOption>();
            var clientVersion = typeof(ZeebeClient).Assembly.GetName().Version;
            var userAgentString = $"zeebe-client-csharp/{clientVersion}";
            var userAgentOption = new ChannelOption(ChannelOptions.PrimaryUserAgentString, userAgentString);
            channelOptions.Add(userAgentOption);

            AddKeepAliveToChannelOptions(channelOptions, keepAlive);

            channelToGateway =
                new Channel(address, credentials, channelOptions);
            gatewayClient = new Gateway.GatewayClient(channelToGateway);

            asyncRetryStrategy =
                new TransientGrpcErrorRetryStrategy(sleepDurationProvider ??
                                                    DefaultWaitTimeProvider);
        }

        /// <summary>
        /// Adds keepAlive options to the channel options.
        /// </summary>
        /// <param name="channelOptions">the current existing channel options.</param>
        private void AddKeepAliveToChannelOptions(List<ChannelOption> channelOptions, TimeSpan? keepAlive)
        {
            // GRPC_ARG_KEEPALIVE_PERMIT_WITHOUT_CALLS
            // This channel argument if set to 1 (0 : false; 1 : true), allows keepalive pings to be sent even if there are no calls in flight.
            // channelOptions.Add(new ChannelOption("grpc.keepalive_permit_without_calls", "1"));
            // this will increase load on the system and also increase used resources
            // we should prefer idleTimeout setting
            // https://stackoverflow.com/questions/57930529/grpc-connection-use-keepalive-or-idletimeout

            // GRPC_ARG_KEEPALIVE_TIME_MS
            // This channel argument controls the period (in milliseconds) after which a keepalive ping is sent on the transport.
            var actualKeepAlive = keepAlive.GetValueOrDefault(DefaultKeepAlive);
            channelOptions.Add(new ChannelOption("grpc.keepalive_time_ms", (int) actualKeepAlive.TotalMilliseconds));
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

        public ITopologyRequestStep1 TopologyRequest() => new TopologyRequestCommand(gatewayClient, asyncRetryStrategy);

        public void Dispose()
        {
            if (gatewayClient is ClosedGatewayClient)
            {
                return;
            }

            gatewayClient = new ClosedGatewayClient();
            channelToGateway.ShutdownAsync().Wait();
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
}
