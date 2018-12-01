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

namespace Zeebe.Client.Api.Clients
{
    /**
     * A client with access to all job-related operation:
     * <li>complete a job
     * <li>mark a job as failed
     * <li>update the retries of a job
     */
    public interface IJobClient
    {

        /**
         * Command to complete a job.
         *
         * <pre>
         * long jobKey = ..;
         *
         * jobClient
         *  .NewCompleteJobCommand(jobKey)
         *  .payload(json)
         *  .send();
         * </pre>
         *
         * <p>If the job is linked to a workflow instance then this command will complete the related
         * activity and continue the flow.
         *
         * @param jobKey the key which identifies the job
         * @return a builder for the command
         */
        ICompleteJobCommandStep1 NewCompleteJobCommand(long jobKey);

    }
}
