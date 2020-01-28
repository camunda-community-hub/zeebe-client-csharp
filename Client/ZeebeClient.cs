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
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Zeebe.Client.Api.Builder;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Builder;
using Zeebe.Client.Impl.Commands;
using Zeebe.Client.Impl.Worker;

namespace Zeebe.Client
{
    /// <inheritdoc />
    public class ZeebeClient : IZeebeClient
    {
        private readonly Channel channelToGateway;
        private readonly ILoggerFactory loggerFactory;
        private Gateway.GatewayClient gatewayClient;

        internal ZeebeClient(string address, ILoggerFactory loggerFactory = null)
            : this(address, ChannelCredentials.Insecure, loggerFactory)
        { }

        internal ZeebeClient(string address, ChannelCredentials credentials, ILoggerFactory loggerFactory = null)
        {
            this.loggerFactory = loggerFactory;
            channelToGateway = new Channel(address, credentials);
            gatewayClient = new Gateway.GatewayClient(channelToGateway);
        }

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////// JOBS /////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////

        public IJobWorkerBuilderStep1 NewWorker()
        {
            return new JobWorkerBuilder(gatewayClient, this, loggerFactory);
        }

        public IActivateJobsCommandStep1 NewActivateJobsCommand()
        {
            return new ActivateJobsCommand(gatewayClient);
        }

        public ICompleteJobCommandStep1 NewCompleteJobCommand(long jobKey)
        {
            return new CompleteJobCommand(gatewayClient, jobKey);
        }

        public ICompleteJobCommandStep1 NewCompleteJobCommand(IJob activatedJob)
        {
            return new CompleteJobCommand(gatewayClient, activatedJob.Key);
        }

        public IFailJobCommandStep1 NewFailCommand(long jobKey)
        {
            return new FailJobCommand(gatewayClient, jobKey);
        }

        public IUpdateRetriesCommandStep1 NewUpdateRetriesCommand(long jobKey)
        {
            return new UpdateRetriesCommand(gatewayClient, jobKey);
        }

        public IThrowErrorCommandStep1 NewThrowErrorCommand(long jobKey)
        {
            return new ThrowErrorCommand(gatewayClient, jobKey);
        }

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////// Workflows ////////////////////////////////
        ////////////////////////////////////////////////////////////////////////

        public IDeployWorkflowCommandStep1 NewDeployCommand()
        {
            return new DeployWorkflowCommand(gatewayClient);
        }

        public ICreateWorkflowInstanceCommandStep1 NewCreateWorkflowInstanceCommand()
        {
            return new CreateWorkflowInstanceCommand(gatewayClient);
        }

        public ICancelWorkflowInstanceCommandStep1 NewCancelInstanceCommand(long workflowInstanceKey)
        {
            return new CancelWorkflowInstanceCommand(gatewayClient, workflowInstanceKey);
        }

        public ISetVariablesCommandStep1 NewSetVariablesCommand(long elementInstanceKey)
        {
            return new SetVariablesCommand(gatewayClient, elementInstanceKey);
        }

        public IResolveIncidentCommandStep1 NewResolveIncidentCommand(long incidentKey)
        {
            return new ResolveIncidentCommand(gatewayClient, incidentKey);
        }

        public IPublishMessageCommandStep1 NewPublishMessageCommand()
        {
            return new PublishMessageCommand(gatewayClient);
        }

        public ITopologyRequestStep1 TopologyRequest() => new TopologyRequestCommand(gatewayClient);

        public void Dispose()
        {
            gatewayClient = new ClosedGatewayClient();
            channelToGateway.ShutdownAsync().Wait();
        }

        /// <summary>
        /// Creates an new IZeebeClientBuilder. This builder need to be used to construct
        /// a ZeebeClient.
        /// </summary>
        /// <returns>an builder to construct an ZeebeClient</returns>
        public static IZeebeClientBuilder Builder()
        {
            return new ZeebeClientBuilder();
        }
    }
}