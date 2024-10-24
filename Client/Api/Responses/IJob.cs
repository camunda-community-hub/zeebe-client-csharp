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

namespace Zeebe.Client.Api.Responses;

public interface IJob
{
    /// <returns> The unique key of the job.</returns>
    long Key { get; }

    /// <returns> The type of the job.</returns>
    string Type { get; }

    /// <returns> Key of the process instance.</returns>
    long ProcessInstanceKey { get; }

    /// <returns> BPMN process id of the process.</returns>
    string BpmnProcessId { get; }

    /// <returns> Version of the process.</returns>
    int ProcessDefinitionVersion { get; }

    /// <returns> Key of the process.</returns>
    long ProcessDefinitionKey { get; }

    /// <returns> Id of the process element.</returns>
    string ElementId { get; }

    /// <returns> Key of the element instance.</returns>
    long ElementInstanceKey { get; }

    /// <returns>the assigned worker to complete the job.</returns>
    string Worker { get; }

    /// <returns>remaining retries.</returns>
    int Retries { get; }

    /// <returns>
    /// The time until when the job is exclusively assigned to this worker. If the deadline is
    ///     exceeded, it can happen that the job is handed to another worker and the work is performed
    ///     twice.
    /// </returns>
    DateTime Deadline { get; }

    /// <returns> JSON-formatted variables.</returns>
    string Variables { get; }

    /// <returns> JSON-formatted Custom Headers.</returns>
    string CustomHeaders { get; }

    /// <returns> tenant ID of the process.</returns>
    string TenantId { get; }
}