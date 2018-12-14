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

    /// <summary> 
    /// The client to communicate with a Zeebe broker/cluster. 
    /// </summary>
    public interface IZeebeClient : IJobClient, IDisposable
    {

        /// <summary>
        /// Registers a new job worker for jobs of a given type.
        /// 
        /// <p>After registration, the broker activates available jobs and assigns them to this worker. It
        /// then publishes them to the client. The given worker is called for every received job, works on
        /// them and eventually completes them.
        /// 
        /// <pre>
        /// using(IJobWorker worker = zeebeClient
        ///  .NewWorker()
        ///  .jobType("payment")
        ///  .handler(paymentHandler)
        ///  .open())
        ///  {
        ///  ...
        ///  }
        /// </pre>
        /// 
        /// Example JobHandler implementation:
        /// 
        /// <pre>
        /// var handler = (client, job) =>
        ///   {
        ///     String json = job.Payload;
        ///     // modify payload
        /// 
        ///     client
        ///      .CompleteCommand(job.Key)
        ///      .Payload(json)
        ///      .Send();
        ///   };
        /// </pre>
        /// 
        /// The handler must be thread-safe.
        /// </summary>
        /// 
        /// <returns>a builder for the worker registration</returns>
        IJobWorkerBuilderStep1 NewWorker();

        /// <summary>
        /// Command to update the retries of a job.
        /// 
        /// <pre>
        /// long jobKey = ..;
        /// 
        /// zeebeClient
        ///  .NewUpdateRetriesCommand(jobKey)
        ///  .Retries(3)
        ///  .Send();
        /// </pre>
        /// 
        /// <p>If the given retries are greater than zero then this job will be picked up again by a job
        /// subscription and a related incident will be marked as resolved.
        /// </summary>
        /// <param name="jobKey">the key of the job to update</param>
        /// <returns>a builder for the command</returns>
        IUpdateRetriesCommandStep1 NewUpdateRetriesCommand(long jobKey);

        /// <summary>
        /// Command to deploy new workflows.
        /// 
        /// <pre>
        /// zeebeClient
        ///  .NewDeployCommand()
        ///  .AddResourceFile("~/wf/workflow1.bpmn")
        ///  .AddResourceFile("~/wf/workflow2.bpmn")
        ///  .Send();
        /// </pre>
        /// </summary>
        /// 
        /// <returns>a builder for the deploy command</returns>
        IDeployWorkflowCommandStep1 NewDeployCommand();

        /// <summary>
        /// Command to create/start a new instance of a workflow.
        /// 
        /// <pre>
        /// zeebeClient
        ///  .NewCreateInstanceCommand()
        ///  .BpmnProcessId("my-process")
        ///  .LatestVersion()
        ///  .Payload(json)
        ///  .Send();
        /// </pre>
        /// 
        /// </summary>
        /// <returns>a builder for the command</returns>
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

        /// <summary>
        /// Command to publish a message which can be correlated to a workflow instance.
        /// 
        /// <pre>
        /// zeebeClient
        ///  .NewPublishMessageCommand()
        ///  .MessageName("order canceled")
        ///  .CorrelationKey(orderId)
        ///  .Payload(json)
        ///  .Send();
        /// </pre>
        /// 
        /// </summary>
        /// 
        /// <returns>a builder for the command</returns>
        IPublishMessageCommandStep1 NewPublishMessageCommand();


        /// <summary>
        /// Request to get all deployed workflows.
        ///
        /// <pre>
        /// IList&#60;Workflow&#62; workflows = await zeebeClient
        ///  .NewListWorkflowRequest()
        ///  .Send()
        ///  .getWorkflows();
        ///
        /// String bpmnProcessId = workflow.getBpmnProcessId();
        /// </pre>
        ///
        /// The response does not contain the resources of the workflows. Use {@link #newResourceRequest()}
        /// to get the resource of a workflow.
        /// </summary>
        /// <returns>the builder for the command</returns>
        IListWorkflowsRequestStep1 NewListWorkflowRequest();


        /// <summary>
        /// Request to get the resource of a workflow (i.e. the XML representation).
        ///
        /// <pre>
        /// IWorkflowResourceResponse resource = await zeebeClient
        ///  .NewWorkflowResourceRequest()
        ///  .BpmnProcessId("my-process")
        ///  .LastestVersion()
        ///  .Send()
        ///
        /// String bpmnXml = resoure.BpmnXml;
        /// </pre>
        /// </summary>
        /// <returns>a builder of the request</returns>
        IWorkflowResourceRequestStep1 NewWorkflowResourceRequest();

        /// <summary>
        /// Request the current cluster topology. Can be used to inspect which brokers are available at
        /// which endpoint and which broker is the leader of which partition.
        /// 
        /// <pre>
        /// ITopology response = await ZeebeClient.TopologyRequest().Send();
        /// IList<IBrokerInfo> brokers = response.Brokers;
        /// </pre>
        /// 
        /// </summary>
        /// 
        /// <returns>the request where you must call #send()</returns>
        ITopologyRequestStep1 TopologyRequest();
    }
}
