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
using Zeebe.Client.Api.Clients;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Subscription
{
    public interface IJobWorkerBuilderStep1
    {
        /// <summary>
        /// Set the type of jobs to work on.
        /// </summary>
        /// <param name="type">the type of jobs (e.g. "payment")</param>
        /// <returns>the builder for this worker</returns>
        IJobWorkerBuilderStep2 JobType(string type);
    }

    /// <summary>
    /// The job handler which contains the business logic.
    /// </summary>
    /// <param name="client">the job client to complete or fail the job</param>
    /// <param name="activatedJob">the job, which was activated by the worker</param>
    public delegate void JobHandler(IJobClient client, IJob activatedJob);

    public interface IJobWorkerBuilderStep2
    {
        /// <summary>
        /// Set the handler to process the jobs. At the end of the processing, the handler should
        /// complete the job or mark it as failed;
        ///
        /// <p>Example JobHandler implementation:
        ///
        /// <pre>
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
        /// </pre>
        ///
        /// The handler must be thread-safe.
        /// </summary>
        /// <param name="handler">the handle to process the jobs</param>
        /// <returns>the builder for this worker</returns>
        IJobWorkerBuilderStep3 Handler(JobHandler handler);
    }

    public interface IJobWorkerBuilderStep3
    {
        /// <summary>
        /// Set the time for how long a job is exclusively assigned for this worker.
        ///
        /// <p>In this time, the job can not be assigned by other workers to ensure that only one worker
        /// work on the job. When the time is over then the job can be assigned again by this or other
        /// worker if it's not completed yet.
        ///
        /// <p>If no timeout is set, then the default is used from the configuration.
        /// </summary>
        /// <param name="timeout">the time in milliseconds</param>
        /// <returns>the builder for this worker</returns>
        IJobWorkerBuilderStep3 Timeout(long timeout);

        /// <summary>
        /// Set the time for how long a job is exclusively assigned for this worker.
        ///
        /// <p>In this time, the job can not be assigned by other workers to ensure that only one worker
        /// work on the job. When the time is over then the job can be assigned again by this or other
        /// worker if it's not completed yet.
        ///
        /// <p>If no time is set then the default is used from the configuration.
        ///
        /// <param name="timeout">the time as time span (e.g. "TimeSpan.FromMinutes(10)")</param>
        /// <returns>the builder for this worker
        /// </summary>
        IJobWorkerBuilderStep3 Timeout(TimeSpan timeout);

        /// <summary>
        /// Set the name of the worker owner.
        ///
        /// <p>This name is used to identify the worker to which a job is exclusively assigned to.
        ///
        /// <p>
        /// If no name is set then the default is used from the configuration.
        /// </p>
        /// </summary>
        /// <param name="workerName">the name of the worker (e.g. "payment-service")</param>
        /// <returns>the builder for this worker</returns>
        IJobWorkerBuilderStep3 Name(string workerName);

        /// <summary>
        /// Set the maximum number of jobs which will be exclusively activated for this worker at the same
        /// time.
        ///
        ///<p>This is used to control the back pressure of the worker. When the maximum is reached then
        /// the worker will stop activating new jobs in order to not overwhelm the client and give other
        /// workers the chance to work on the jobs. The worker will try to activate new jobs again when
        /// jobs are completed (or marked as failed).
        ///
        /// <p>Considerations:
        ///
        /// <ul>
        ///   <li>A greater value can avoid situations in which the client waits idle for the broker to
        ///       provide more jobs. This can improve the worker's throughput.
        ///   <li>The memory used by the worker is linear with respect to this value.
        ///   <li>The job's timeout starts to run down as soon as the broker pushes the job. Keep in mind
        ///       that the following must hold to ensure fluent job handling: <code>
        ///       time spent in buffer + time job handler needs until job completion < job timeout</code>
        /// </summary>
        /// <param name="maxJobsActive">the maximum jobs active by this worker</param>
        /// <returns>the builder for this worker</returns>
        IJobWorkerBuilderStep3 MaxJobsActive(int maxJobsActive);

        /// <summary>
        /// Set a list of variable names which should be fetch on job activation.
        ///
        /// <p>The jobs which are activated by this command will only contain variables from this list.
        ///
        /// <p>This can be used to limit the number of variables of the activated jobs.
        /// </summary>
        /// <param name="fetchVariables">list of variables names to fetch on activation</param>
        /// <returns>the builder for this worker</returns>
        IJobWorkerBuilderStep3 FetchVariables(IList<string> fetchVariables);

        /// <summary>
        /// Set a list of variable names which should be fetch on job activation.
        ///
        /// <p>The jobs which are activated by this command will only contain variables from this list.
        ///
        /// <p>This can be used to limit the number of variables of the activated jobs.
        /// </summary>
        /// <param name="fetchVariables">list of variables names to fetch on activation</param>
        /// <returns>the builder for this worker</returns>
        IJobWorkerBuilderStep3 FetchVariables(params string[] fetchVariables);

        /// <summary>
        /// Set the maximal interval between polling for new jobs.
        ///
        /// <p>A job worker will automatically try to always activate new jobs after completing jobs. If
        /// no jobs can be activated after completing the worker will periodically poll for new jobs.
        ///
        /// <p>If no poll interval is set then the default is used from the {@link
        /// ZeebeClientConfiguration}
        /// </summary>
        /// <param name="pollInterval">the maximal interval to check for new jobs</param>
        /// <returns>the builder for this worker</returns>
        IJobWorkerBuilderStep3 PollInterval(TimeSpan pollInterval);

        /// <summary>
        /// Open the worker and start to work on available tasks.
        /// </summary>
        /// <returns>the worker</returns>
        IJobWorker Open();
    }
}