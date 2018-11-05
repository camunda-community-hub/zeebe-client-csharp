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

using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Subscription;

namespace Zeebe.Client.Api.Clients
{
    /**
     * A client with access to all job-related operation:
     * <li>create a (standalone) job
     * <li>complete a job
     * <li>mark a job as failed
     * <li>update the retries of a job
     */
    public interface IJobClient
    {
        /**
         * Registers a new job worker for jobs of a given type.
         *
         * <p>After registration, the broker activates available jobs and assigns them to this worker. It
         * then publishes them to the client. The given worker is called for every received job, works on
         * them and eventually completes them.
         *
         * <pre>
         * IJobWorker worker = jobClient
         *  .Worker()
         *  .jobType("payment")
         *  .handler(paymentHandler)
         *  .open();
         *
         * ...
         * worker.close();
         * </pre>
         *
         * Example JobHandler implementation:
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
         * @return a builder for the worker registration
         */
        IJobWorkerBuilderStep1 Worker();
    }
}
