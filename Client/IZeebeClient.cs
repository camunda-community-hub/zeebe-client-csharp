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
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client
{
    /// <summary>
    /// The client to communicate with a Zeebe broker/cluster.
    /// </summary>
    public interface IZeebeClient : IJobClient, IDisposable
    {
        /// <summary>
        /// Registers a new job worker for jobs of a given type.
        /// </summary>
        ///
        /// <para>
        /// After registration, the broker activates available jobs and assigns them to this worker. It
        /// then publishes them to the client. The given worker is called for every received job, works on
        /// them and eventually completes them.
        /// </para>
        ///
        /// <code>
        /// using(IJobWorker worker = zeebeClient
        ///  .NewWorker()
        ///  .jobType("payment")
        ///  .handler(paymentHandler)
        ///  .open())
        ///  {
        ///  ...
        ///  }
        /// </code>
        ///
        /// Example JobHandler implementation:
        ///
        /// <code>
        /// var handler = (client, job) =>
        ///   {
        ///     String json = job.Variables;
        ///     // modify variables
        ///
        ///     client
        ///      .CompleteCommand(job.Key)
        ///      .Variables(json)
        ///      .Send();
        ///   };
        /// </code>
        ///
        /// The handler must be thread-safe.
        /// <returns>a builder for the worker registration</returns>
        IJobWorkerBuilderStep1 NewWorker();

        /// <summary>
        /// Command to activate multiple jobs of a given type.
        /// </summary>
        ///
        /// <code>
        /// zeebeClient
        ///  .NewActivateJobsCommand()
        ///  .JobType("payment")
        ///  .maxJobsToActivate(10)
        ///  .WorkerName("paymentWorker")
        ///  .Timeout(TimeSpan.FromMinutes(10))
        ///  .Send();
        /// </code>
        ///
        /// <para>
        ///     The command will try to use <c>maxJobsToActivate</c>
        ///     for given <c>jobType</c>. If less
        ///     then the requested <c>maxJobsToActivate</c> jobs of the
        ///     <c>jobType</c> are available for
        ///     activation the returned list will have fewer elements.
        /// </para>
        ///
        /// <returns>
        /// a builder for the command
        /// </returns>
        IActivateJobsCommandStep1 NewActivateJobsCommand();

        /// <summary>
        /// Command to update the retries of a job.
        /// </summary>
        ///
        /// <code>
        /// long jobKey = ..;
        ///
        /// zeebeClient
        ///  .NewUpdateRetriesCommand(jobKey)
        ///  .Retries(3)
        ///  .Send();
        /// </code>
        ///
        /// <para>
        /// If the given retries are greater than zero then this job will be picked up again by a job
        /// subscription and a related incident will be marked as resolved.
        /// </para>
        /// <param name="jobKey">
        ///     the key of the job to update
        /// </param>
        /// <returns>
        ///     a builder for the command
        /// </returns>
        IUpdateRetriesCommandStep1 NewUpdateRetriesCommand(long jobKey);

        /// <summary>
        /// Command to deploy new workflows.
        /// </summary>
        ///
        /// <code>
        /// zeebeClient
        ///  .NewDeployCommand()
        ///  .AddResourceFile("~/wf/workflow1.bpmn")
        ///  .AddResourceFile("~/wf/workflow2.bpmn")
        ///  .Send();
        /// </code>
        ///
        /// <returns>
        ///     a builder for the deploy command
        /// </returns>
        IDeployWorkflowCommandStep1 NewDeployCommand();

        /// <summary>
        /// Command to create/start a new instance of a workflow.
        /// </summary>
        ///
        /// <code>
        /// zeebeClient
        ///  .NewCreateInstanceCommand()
        ///  .BpmnProcessId("my-process")
        ///  .LatestVersion()
        ///  .Variables(json)
        ///  .Send();
        /// </code>
        ///
        /// <returns>a builder for the command</returns>
        ICreateWorkflowInstanceCommandStep1 NewCreateWorkflowInstanceCommand();

        /// <summary>
        /// Command to cancel a workflow instance.
        /// </summary>
        ///
        /// <code>
        /// zeebeClient
        ///  .NewCancelInstanceCommand(workflowInstanceKey)
        ///  .Send();
        /// </code>
        ///
        /// <param name="workflowInstanceKey">
        ///     workflowInstanceKey the key which identifies the corresponding workflow instance
        /// </param>
        /// <returns>
        /// a builder for the command
        /// </returns>
        ICancelWorkflowInstanceCommandStep1 NewCancelInstanceCommand(long workflowInstanceKey);

        /// <summary>
        /// Command to update the variables of a workflow instance.
        /// </summary>
        ///
        ///  <code>
        ///   zeebeClient
        ///    .NewSetVariablesCommand(elementInstanceKey)
        ///    .Variables(json)
        ///    .Send();
        ///  </code>
        /// <param name="elementInstanceKey">
        ///     the key of the element instance to set the variables for
        /// </param>
        /// <returns>
        ///     a builder for the command
        /// </returns>
        ISetVariablesCommandStep1 NewSetVariablesCommand(long elementInstanceKey);

        /// <summary>
        ///   Command to resolve an existing incident.
        /// </summary>
        ///
        /// <code>
        ///     zeebeClient
        ///         .NewResolveIncidentCommand(incidentKey)
        ///         .Send();
        /// </code>
        /// <param name="incidentKey">
        ///     incidentKey the key of the corresponding incident
        /// </param>
        /// <returns>
        ///     the builder for the command
        /// </returns>
        IResolveIncidentCommandStep1 NewResolveIncidentCommand(long incidentKey);

        /// <summary>
        /// Command to publish a message which can be correlated to a workflow instance.
        /// </summary>
        ///
        /// <code>
        /// zeebeClient
        ///  .NewPublishMessageCommand()
        ///  .MessageName("order canceled")
        ///  .CorrelationKey(orderId)
        ///  .Variables(json)
        ///  .Send();
        /// </code>
        ///
        /// <returns>
        ///     a builder for the command
        /// </returns>
        IPublishMessageCommandStep1 NewPublishMessageCommand();

        /// <summary>
        /// Request the current cluster topology. Can be used to inspect which brokers are available at
        /// which endpoint and which broker is the leader of which partition.
        /// </summary>
        ///
        /// <code>
        /// ITopology response = await ZeebeClient.TopologyRequest().Send();
        /// IList{IBrokerInfo} brokers = response.Brokers;
        /// </code>
        ///
        /// <returns>
        ///     the request where you must call <see cref="IFinalCommandStep{T}.Send"/>
        /// </returns>
        ITopologyRequestStep1 TopologyRequest();
    }
}
