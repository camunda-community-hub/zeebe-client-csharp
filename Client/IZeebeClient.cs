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
using Zeebe.Client.Api.Clients;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.CommandsClient;
using Zeebe.Client.Api.Subscription;

namespace Zeebe.Client
{

    /** The client to communicate with a Zeebe broker/cluster. */
    public interface IZeebeClient : IJobClient, IDisposable
    {

        /**
         * Registers a new job worker for jobs of a given type.
         *
         * <p>After registration, the broker activates available jobs and assigns them to this worker. It
         * then publishes them to the client. The given worker is called for every received job, works on
         * them and eventually completes them.
         *
         * <pre>
         * using(IJobWorker worker = zeebeClient
         *  .NewWorker()
         *  .jobType("payment")
         *  .handler(paymentHandler)
         *  .open())
         *  {
         *  ...
         *  }
         * </pre>
         *
         * Example JobHandler implementation:
         *
         * <pre>
         * public class PaymentHandler : IJobHandler
         * {
         *   public override void handle(IJobClient client, JobEvent jobEvent)
         *   {
         *     String json = jobEvent.getPayload();
         *     // modify payload
         *
         *     client
         *      .CompleteCommand()
         *      .event(jobEvent)
         *      .payload(json)
         *      .send();
         *   }
         * };
         * </pre>
         *
         * @return a builder for the worker registration
         */
        IJobWorkerBuilderStep1 NewWorker();

        /**
         * Command to deploy new workflows.
         *
         * <pre>
         * zeebeClient
         *  .NewDeployCommand()
         *  .AddResourceFile("~/wf/workflow1.bpmn")
         *  .AddResourceFile("~/wf/workflow2.bpmn")
         *  .Send();
         * </pre>
         *
         * @return a builder for the deploy command
         */
        IDeployWorkflowCommandStep1 NewDeployCommand();

        /**
         * Command to create/start a new instance of a workflow.
         *
         * <pre>
         * zeebeClient
         *  .NewCreateInstanceCommand()
         *  .BpmnProcessId("my-process")
         *  .LatestVersion()
         *  .Payload(json)
         *  .Send();
         * </pre>
         *
         * @return a builder for the command
         */
        ICreateWorkflowInstanceCommandStep1 NewCreateWorkflowInstanceCommand();

        /**
         * Command to publish a message which can be correlated to a workflow instance.
         *
         * <pre>
         * zeebeClient
         *  .newPublishMessageCommand()
         *  .messageName("order canceled")
         *  .correlationKey(orderId)
         *  .payload(json)
         *  .send();
         * </pre>
         *
         * @return a builder for the command
         */
        IPublishMessageCommandStep1 NewPublishMessageCommand();

        /**
         * Request the current cluster topology. Can be used to inspect which brokers are available at
         * which endpoint and which broker is the leader of which partition.
         *
         * <pre>
         * ITopology response = await ZeebeClient.TopologyRequest().Send();
         * IList<IBrokerInfo> brokers = response.Brokers;
         * </pre>
         *
         * @return the request where you must call {@code send()}
         */
        ITopologyRequestStep1 TopologyRequest();
    }
}
