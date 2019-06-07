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
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.CommandsClient;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Subscription;
using Zeebe.Client.Impl.Commands;
using Zeebe.Client.Impl.Subscription;

namespace Zeebe.Client
{
    public class ZeebeClient : IZeebeClient
    {
        private const string DEFAULT_ADDRESS = "localhost:26500";

        private readonly Channel channelToGateway;
        private readonly Gateway.GatewayClient gatewayClient;

        private ZeebeClient(string address)
        {
            channelToGateway = new Channel(address, ChannelCredentials.Insecure);
            gatewayClient = new Gateway.GatewayClient(channelToGateway);
        }

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////// JOBS /////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////

        public IJobWorkerBuilderStep1 NewWorker()
        {
            return new JobWorkerBuilder(gatewayClient, this);
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

        public void Dispose() => channelToGateway.ShutdownAsync();

        public static IZeebeClient NewZeebeClient()
        {
            return new ZeebeClient(DEFAULT_ADDRESS);
        }

        public static IZeebeClient NewZeebeClient(string address)
        {
            return new ZeebeClient(address);
        }
    }
}
