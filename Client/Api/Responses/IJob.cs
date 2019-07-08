﻿//
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

namespace Zeebe.Client.Api.Responses
{
    public interface IJob
    {
        /// <returns> The unique key of the job </returns>
        long Key { get; }

        /// <returns> The type of the job </returns>
        string Type { get; }

        /// <returns> Key of the workflow instance </summary>
        long WorkflowInstanceKey { get; }

        /// <returns> BPMN process id of the workflow </summary>
        string BpmnProcessId { get; }

        /// <returns> Version of the workflow </returns>
        int WorkflowDefinitionVersion { get; }

        /// <returns> Key of the workflow </returns>
        long WorkflowKey { get; }

        /// <returns> Id of the workflow element </returns>
        string ElementId { get; }

        /// <returns> Key of the element instance </returns>
        long ElementInstanceKey { get; }

        /// <returns>the assigned worker to complete the job </returns>
        string Worker { get; }

        /// <returns>remaining retries </returns>
        int Retries { get; }

        /// <returns>
        /// The time until when the job is exclusively assigned to this worker. If the deadline is
        ///     exceeded, it can happen that the job is handed to another worker and the work is performed
        ///     twice.
        /// </returns>
        DateTime Deadline { get; }

        /// <returns> JSON-formatted variables </returns>
        string Variables { get; }

        /// <returns> De-serialized variables as map </returns>
        IDictionary<string, object> VariablesAsDictionary { get; }

        /// <returns>de-serialized variables as the given type </returns>
        InstanceType VariablesAsType<InstanceType>();

        /// <returns> JSON-formatted Custom Headers </returns>
        string CustomHeaders { get; }

        /// <returns> De-serialized Custom Headers as map </returns>
        IDictionary<string, string> CustomHeadersAsDictionary { get; }
    }
}