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
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Worker;

/// <summary>
/// A client with access to all job-related operation:
/// <list type="bullet">
///   <item>complete a job</item>
///   <item> mark a job as failed</item>
///   <item> update the retries of a job</item>
/// </list>
/// </summary>
public interface IJobClient
{
    /// <summary>
    /// Command to complete a job.
    /// </summary>
    /// <example>
    /// <code>
    /// long jobKey = ..;
    ///
    /// jobClient
    ///      .NewCompleteJobCommand(jobKey)
    ///      .Variables(json)
    ///      .Send();
    /// </code>
    /// </example>
    ///
    /// <para>
    ///     The job is linked to a process instance, which means this command will complete the related
    ///     activity and continue the flow.
    /// </para>
    ///
    /// <param name="jobKey">the key which identifies the job.</param>
    /// <returns>a builder for the command.</returns>
    ICompleteJobCommandStep1 NewCompleteJobCommand(long jobKey);

    /// <summary>
    /// Command to complete a job.
    /// </summary>
    ///
    /// <example>
    /// <code>
    /// IJob activatedJob = ..;
    ///
    /// jobClient
    ///      .NewCompleteJobCommand(activatedJob)
    ///      .Variables(json)
    ///      .Send();
    /// </code>
    /// </example>
    ///
    /// <para>
    ///     The job is linked to a process instance, which means this command will complete the related
    ///     activity and continue the flow.
    /// </para>
    ///
    /// <param name="activatedJob">the job, which should be completed.</param>
    /// <returns>a builder for the command.</returns>
    ICompleteJobCommandStep1 NewCompleteJobCommand(IJob activatedJob);

    /// <summary>
    /// Command to mark a job as failed.
    /// </summary>
    ///
    /// <example>
    /// <code>
    /// long jobKey = ..;
    ///
    /// jobClient
    ///      .NewFailCommand(jobKey)
    ///      .Retries(3)
    ///      .Send();
    /// </code>
    /// </example>
    ///
    /// <para>
    ///     If the given retries are greater than zero then this job will be picked up again by a job
    ///     worker. Otherwise, an incident is created for this job.
    /// </para>
    /// <param name="jobKey">the key which identifies the job.</param>
    /// <returns>a builder for the command.</returns>
    IFailJobCommandStep1 NewFailCommand(long jobKey);

    /// <summary>
    /// Command to report a business error (i.e. non-technical) that occurs while processing a job.
    /// </summary>
    /// <example>
    /// <code>
    ///  long jobKey = ...;
    ///  string code = ...;
    ///  jobClient
    ///     .NewThrowErrorCommand(jobKey)
    ///     .ErrorCode(code)
    ///     .ErrorMessage("Business error message")
    ///     .Send();
    /// </code>
    /// </example>
    /// <param name="jobKey">the key which identifies the job.</param>
    /// <returns>a builder for the command.</returns>
    IThrowErrorCommandStep1 NewThrowErrorCommand(long jobKey);
}