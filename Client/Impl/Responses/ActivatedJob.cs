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
using System.Collections.Generic;
using Newtonsoft.Json;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class ActivatedJob : IJob
    {
        private static DateTime FromUTCTimestamp(long milliseconds)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(milliseconds).ToLocalTime();
            return dtDateTime;
        }

        public ActivatedJob(GatewayProtocol.ActivatedJob activatedJob)
        {
            Key = activatedJob.Key;
            Type = activatedJob.Type;
            WorkflowInstanceKey = activatedJob.WorkflowInstanceKey;
            BpmnProcessId = activatedJob.BpmnProcessId;
            WorkflowDefinitionVersion = activatedJob.WorkflowDefinitionVersion;
            WorkflowKey = activatedJob.WorkflowKey;
            ElementId = activatedJob.ElementId;
            ElementInstanceKey = activatedJob.ElementInstanceKey;
            Worker = activatedJob.Worker;
            Retries = activatedJob.Retries;
            Deadline = FromUTCTimestamp(activatedJob.Deadline);
            Variables = activatedJob.Variables;
            VariablesAsDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(Variables);
            CustomHeaders = activatedJob.CustomHeaders;
            CustomHeadersAsDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(CustomHeaders);
        }

        public long Key { get; }

        public string Type { get; }

        public long WorkflowInstanceKey { get; }

        public string BpmnProcessId { get; }

        public int WorkflowDefinitionVersion { get; }

        public long WorkflowKey { get; }

        public string ElementId { get; }

        public long ElementInstanceKey { get; }

        public string Worker { get; }

        public int Retries { get; }

        public DateTime Deadline { get; }

        public string Variables { get; }

        public IDictionary<string, object> VariablesAsDictionary { get; }

        public InstanceType VariablesAsType<InstanceType>()
        {
            return JsonConvert.DeserializeObject<InstanceType>(Variables);
        }

        public string CustomHeaders { get; }

        public IDictionary<string, string> CustomHeadersAsDictionary { get; }

        public override string ToString()
        {
            return $"{nameof(Key)}: {Key}, {nameof(Type)}: {Type}, {nameof(WorkflowInstanceKey)}: {WorkflowInstanceKey}, {nameof(BpmnProcessId)}: {BpmnProcessId}, {nameof(WorkflowDefinitionVersion)}: {WorkflowDefinitionVersion}, {nameof(WorkflowKey)}: {WorkflowKey}, {nameof(ElementId)}: {ElementId}, {nameof(ElementInstanceKey)}: {ElementInstanceKey}, {nameof(Worker)}: {Worker}, {nameof(Retries)}: {Retries}, {nameof(Deadline)}: {Deadline}, {nameof(Variables)}: {Variables}, {nameof(VariablesAsDictionary)}: {VariablesAsDictionary}, {nameof(CustomHeaders)}: {CustomHeaders}, {nameof(CustomHeadersAsDictionary)}: {CustomHeadersAsDictionary}";
        }
    }
}
