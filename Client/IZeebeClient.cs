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
 * Command to update the retries of a job.
 *
 * <pre>
 * long jobKey = ..;
 *
 * zeebeClient
 *  .newUpdateRetriesCommand(jobKey)
 *  .retries(3)
 *  .send();
 * </pre>
 *
 * <p>If the given retries are greater than zero then this job will be picked up again by a job
 * subscription and a related incident will be marked as resolved.
 *
 * @param jobKey the key of the job to update
 * @return a builder for the command
 */
     IUpdateRetriesCommandStep1 NewUpdateRetriesCommand(long jobKey);

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


     /// <summary>
     /// Command to cancel a workflow instance.
     ///
     /// <pre>
     /// zeebeClient
     ///  .NewCancelInstanceCommand(workflowInstanceKey)
     ///  .Send();
     /// </pre>
     /// </summary>
     /// <param name="elementInstanceKey">workflowInstanceKey the key which identifies the corresponding workflow instance </param>
     /// <returns> a builder for the command </returns>
     ///
     ICancelWorkflowInstanceCommandStep1 NewCancelInstanceCommand(long workflowInstanceKey);

     /// <summary>
     /// Command to update the payload of a workflow instance.
     ///  <pre>
     ///   zeebeClient
     ///    .NewUpdatePayloadCommand(elementInstanceKey)
     ///    .Payload(json)
     ///    .Send();
     ///  </pre>
     /// </summary>
     /// <param name="elementInstanceKey">the key of the element instance to update the payload for</param>
     /// <returns> a builder for the command</returns>
     IUpdatePayloadCommandStep1 NewUpdatePayloadCommand(long elementInstanceKey);


     /// <summary>
     ///   Command to resolve an existing incident.
     ///   <pre>
     ///     zeebeClient
     ///       .NewResolveIncidentCommand(incidentKey)
     ///       .Send();
     ///   </pre>
     /// </summary>
     /// <param name="incidentKey">incidentKey the key of the corresponding incident</param>
     /// <returns>the builder for the command</returns>
     IResolveIncidentCommandStep1 NewResolveIncidentCommand(long incidentKey);

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


     /// <summary>
     /// Request to get all deployed workflows.
     ///
     /// <pre>
     /// IList&#60;Workflow&#62; workflows = await zeebeClient
     ///  .NewWorkflowRequest()
     ///  .Send()
     ///  .getWorkflows();
     ///
     /// String bpmnProcessId = workflow.getBpmnProcessId();
     /// </pre>
     ///
     /// The response does not contain the resources of the workflows. Use {@link #newResourceRequest()}
     /// to get the resource of a workflow.
     /// </summary>
     /// @see #newResourceRequest()
     /// @return a builder of the request
     /// <returns>the builder for the command</returns>
     IListWorkflowsRequestStep1 NewListWorkflowRequest();

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
