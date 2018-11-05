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

namespace Zeebe.Client.Api.Responses
{
    /** Context in case the job is part of a workflow instance */
    public interface IJobHeaders
    {
        /** Key of the workflow instance */
        long WorkflowInstanceKey { get; }

        /** BPMN process id of the workflow */
        string BpmnProcessId { get; }

        /** Version of the workflow */
        int WorkflowDefinitionVersion { get; }

        /** Key of the workflow */
        long WorkflowKey { get; }

        /** Id of the workflow element */
        string ElementId { get; }

        /** Key of the element instance */
        long ElementInstanceKey { get; }
    }
}