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
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands;

public interface IFailJobCommandStep1
{
    /// <summary>
    /// Set the remaining retries of this job.
    ///
    /// <para>If the retries are greater than zero then this job will be picked up again by a job
    /// worker. Otherwise, an incident is created for this job.
    /// </para>
    /// </summary>
    ///
    /// <param name="remainingRetries">the remaining retries of this job (e.g. "jobEvent.getRetries() - 1").</param>
    /// <returns>the builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.
    ///     </returns>
    IFailJobCommandStep2 Retries(int remainingRetries);
}

public interface IFailJobCommandStep2 : IFinalCommandWithRetryStep<IFailJobResponse>
{
    /// <summary>
    /// Set the error message of this failing job.
    ///
    /// If the retries are zero then this error message will be used for the incident creation.
    ///
    /// </summary>
    ///
    /// <param name="errorMsg">the error msg for this failing job.</param>
    /// <returns>the builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.
    /// </returns>
    IFailJobCommandStep2 ErrorMessage(string errorMsg);

    /// <summary>
    /// Set the backoff timeout for the next retry of this job.
    ///
    /// </summary>
    ///
    /// <param name="retryBackOff">the backoff timeout for the next retry of this job.</param>
    /// <returns>the builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.
    /// </returns>
    IFailJobCommandStep2 RetryBackOff(TimeSpan retryBackOff);

    /// <summary>
    /// Set the variables to fail the job with.
    /// </summary>
    /// <param name="variables">the variables (JSON) as String.</param>
    /// <returns>the builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.</returns>
    IFailJobCommandStep2 Variables(string variables);
}