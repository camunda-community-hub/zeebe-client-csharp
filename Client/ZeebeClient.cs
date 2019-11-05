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
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Builder;
using Zeebe.Client.Impl.Commands;
using Zeebe.Client.Impl.Worker;

namespace Zeebe.Client
{
    public class ZeebeClient : IZeebeClient
    {
        private readonly Channel _channelToGateway;
        private readonly ILoggerFactory loggerFactory;
        private Gateway.GatewayClient _gatewayClient;

        internal ZeebeClient(string address, ILoggerFactory loggerFactory = null)
            : this(address, ChannelCredentials.Insecure, loggerFactory)
        { }

        internal ZeebeClient(string address, ChannelCredentials credentials, ILoggerFactory loggerFactory = null)
        {
            this.loggerFactory = loggerFactory;
            _channelToGateway = new Channel(address, credentials);
            _gatewayClient = new Gateway.GatewayClient(_channelToGateway);
        }

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////// JOBS /////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////

        public IJobWorkerBuilderStep1 NewWorker()
        {
            return new JobWorkerBuilder(_gatewayClient, this, loggerFactory);
        }

        public IActivateJobsCommandStep1 NewActivateJobsCommand()
        {
            return new ActivateJobsCommand(_gatewayClient);
        }

        public ICompleteJobCommandStep1 NewCompleteJobCommand(long jobKey)
        {
            return new CompleteJobCommand(_gatewayClient, jobKey);
        }

        public ICompleteJobCommandStep1 NewCompleteJobCommand(IJob activatedJob)
        {
            return new CompleteJobCommand(_gatewayClient, activatedJob.Key);
        }

        public IFailJobCommandStep1 NewFailCommand(long jobKey)
        {
            return new FailJobCommand(_gatewayClient, jobKey);
        }

        public IUpdateRetriesCommandStep1 NewUpdateRetriesCommand(long jobKey)
        {
            return new UpdateRetriesCommand(_gatewayClient, jobKey);
        }

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////// Workflows ////////////////////////////////
        ////////////////////////////////////////////////////////////////////////

        public IDeployWorkflowCommandStep1 NewDeployCommand()
        {
            return new DeployWorkflowCommand(_gatewayClient);
        }

        public ICreateWorkflowInstanceCommandStep1 NewCreateWorkflowInstanceCommand()
        {
            return new CreateWorkflowInstanceCommand(_gatewayClient);
        }

        public ICancelWorkflowInstanceCommandStep1 NewCancelInstanceCommand(long workflowInstanceKey)
        {
            return new CancelWorkflowInstanceCommand(_gatewayClient, workflowInstanceKey);
        }

        public ISetVariablesCommandStep1 NewSetVariablesCommand(long elementInstanceKey)
        {
            return new SetVariablesCommand(_gatewayClient, elementInstanceKey);
        }

        public IResolveIncidentCommandStep1 NewResolveIncidentCommand(long incidentKey)
        {
            return new ResolveIncidentCommand(_gatewayClient, incidentKey);
        }

        public IPublishMessageCommandStep1 NewPublishMessageCommand()
        {
            return new PublishMessageCommand(_gatewayClient);
        }

        public ITopologyRequestStep1 TopologyRequest() => new TopologyRequestCommand(_gatewayClient);

        public void Dispose()
        {
            _gatewayClient = new ClosedGatewayClient();
            _channelToGateway.ShutdownAsync().Wait();
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