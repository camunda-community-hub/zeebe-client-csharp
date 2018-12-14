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
using Zeebe.Client.Api.Commands;

namespace Zeebe.Client.Api.Clients
{
    /// <summary>
    /// A client with access to all job-related operation:
    /// <li>complete a job
    /// <li> mark a job as failed
    /// <li> update the retries of a job
    /// </summary>
    public interface IJobClient
    {

        /// <summary>
        /// Command to complete a job.
        /// 
        /// <pre>
        /// long jobKey = ..;
        /// 
        /// jobClient
        ///  .NewCompleteJobCommand(jobKey)
        ///  .Payload(json)
        ///  .Send();
        /// </pre>
        /// 
        /// <p>If the job is linked to a workflow instance then this command will complete the related
        /// activity and continue the flow.
        /// </p>
        /// 
        /// <param name="jobKey>the key which identifies the job</param>
        /// <returns>a builder for the command

        ICompleteJobCommandStep1 NewCompleteJobCommand(long jobKey);

        /// <summary>
        /// Command to mark a job as failed.
        /// 
        /// <pre>
        /// long jobKey = ..;
        /// 
        /// jobClient
        ///  .NewFailCommand(jobKey)
        ///  .Retries(3)
        ///  .Send();
        /// </pre>
        /// 
        /// <p>If the given retries are greater than zero then this job will be picked up again by a job
        /// worker. Otherwise, an incident is created for this job.
        /// </p>
        /// <param name="jobKey">the key which identifies the job</param>
        /// <returns>a builder for the command
        /// </summary>
        IFailJobCommandStep1 NewFailCommand(long jobKey);
    }
}
