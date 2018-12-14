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
namespace Zeebe.Client.Api.Responses
{
    /// <returns> Context in case the job is part of a workflow instance </summary>
    public interface IJobHeaders
    {
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
    }
}