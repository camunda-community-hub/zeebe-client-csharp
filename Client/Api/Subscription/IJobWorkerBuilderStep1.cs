/*
 * Copyright © 2017 camunda services GmbH (info@camunda.com)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Threading.Tasks;
using Zeebe.Client.Api.Clients;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Subscription
{
    public interface IJobWorkerBuilderStep1
    {
        /**
       * Set the type of jobs to work on.
       *
       * @param type the type of jobs (e.g. "payment")
       * @return the builder for this worker
       */
        IJobWorkerBuilderStep2 JobType(string type);
    }

    public delegate void JobHandler(IJobClient client, IJob activatedJob);

    public interface IJobWorkerBuilderStep2
    {
        /**
         * Set the handler to process the jobs. At the end of the processing, the handler should
         * complete the job or mark it as failed;
         *
         * <p>Example JobHandler implementation:
         *
         * <pre>
         * public class PaymentHandler implements JobHandler
         * {
         *   &#64;Override
         *   public void handle(JobClient client, JobEvent jobEvent)
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
         * The handler must be thread-safe.
         *
         * @param handler the handle to process the jobs
         * @return the builder for this worker
         */
        IJobWorkerBuilderStep3 Handler(JobHandler handler);
    }

    public interface IJobWorkerBuilderStep3
    {
        /**
         * Set the time for how long a job is exclusively assigned for this worker.
         *
         * <p>In this time, the job can not be assigned by other workers to ensure that only one worker
         * work on the job. When the time is over then the job can be assigned again by this or other
         * worker if it's not completed yet.
         *
         * <p>If no timeout is set, then the default is used from the configuration.
         *
         * @param timeout the time in milliseconds
         * @return the builder for this worker
         */
        IJobWorkerBuilderStep3 Timeout(long timeout);

        /**
         * Set the time for how long a job is exclusively assigned for this worker.
         *
         * <p>In this time, the job can not be assigned by other workers to ensure that only one worker
         * work on the job. When the time is over then the job can be assigned again by this or other
         * worker if it's not completed yet.
         *
         * <p>If no time is set then the default is used from the configuration.
         *
         * @param timeout the time as duration (e.g. "Duration.ofMinutes(5)")
         * @return the builder for this worker
         */
        IJobWorkerBuilderStep3 Timeout(TimeSpan timeout);

        /**
         * Set the name of the worker owner.
         *
         * <p>This name is used to identify the worker to which a job is exclusively assigned to.
         *
         * <p>If no name is set then the default is used from the configuration.
         *
         * @param workerName the name of the worker (e.g. "payment-service")
         * @return the builder for this worker
         */
        IJobWorkerBuilderStep3 Name(string workerName);

        /**
         * Set the maximum number of jobs which will be exclusively assigned to this worker at the same
         * time.
         *
         * <p>This is used to control the backpressure of the worker. When the number of assigned jobs
         * is reached then the broker will stop assigning new jobs to the worker in order to to not
         * overwhelm the client and give other workers the chance to work on the jobs. The broker will
         * assign new jobs again when jobs are completed (or marked as failed) which were assigned to
         * the worker.
         *
         * <p>If no limit is set then the default is used from the {@link
         * ZeebeClientConfiguration}.
         *
         * <p>Considerations:
         *
         * <ul>
         *   <li>A greater value can avoid situations in which the client waits idle for the broker to
         *       provide more jobs. This can improve the worker's throughput.
         *   <li>The memory used by the worker is linear with respect to this value.
         *   <li>The job's timeout starts to run down as soon as the broker pushes the job. Keep in mind
         *       that the following must hold to ensure fluent job handling: <code>
         *       time spent in buffer + time job handler needs until job completion < job timeout</code>
         *       .
         *
         * @param numberOfJobs the number of assigned jobs
         * @return the builder for this worker
         */
        IJobWorkerBuilderStep3 Limit(int numberOfJobs);

        /**
         * Set the maximal interval between polling for new jobs.
         *
         * <p>A job worker will automatically try to always activate new jobs after completing jobs. If
         * no jobs can be activated after completing the worker will periodically poll for new jobs.
         *
         * <p>If no poll interval is set then the default is used from the {@link
         * ZeebeClientConfiguration}
         *
         * @param pollInterval the maximal interval to check for new jobs
         * @return the builder for this worker
         */
        IJobWorkerBuilderStep3 PollInterval(TimeSpan pollInterval);

        /**
         * Open the worker and start to work on available tasks.
         *
         * @return the worker
         */
        IJobWorker Open();
    }
}