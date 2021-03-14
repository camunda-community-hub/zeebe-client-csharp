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
            ProcessInstanceKey = activatedJob.ProcessInstanceKey;
            BpmnProcessId = activatedJob.BpmnProcessId;
            ProcessDefinitionVersion = activatedJob.ProcessDefinitionVersion;
            ProcessDefinitionKey = activatedJob.ProcessDefinitionKey;
            ElementId = activatedJob.ElementId;
            ElementInstanceKey = activatedJob.ElementInstanceKey;
            Worker = activatedJob.Worker;
            Retries = activatedJob.Retries;
            Deadline = FromUTCTimestamp(activatedJob.Deadline);
            Variables = activatedJob.Variables;
            CustomHeaders = activatedJob.CustomHeaders;
        }

        public long Key { get; }

        public string Type { get; }

        public long ProcessInstanceKey { get; }

        public string BpmnProcessId { get; }

        public int ProcessDefinitionVersion { get; }

        public long ProcessDefinitionKey { get; }

        public string ElementId { get; }

        public long ElementInstanceKey { get; }

        public string Worker { get; }

        public int Retries { get; }

        public DateTime Deadline { get; }

        public string Variables { get; }

        public string CustomHeaders { get; }

        public override string ToString()
        {
            return
                $"{nameof(Key)}: {Key}, {nameof(Type)}: {Type}, {nameof(ProcessInstanceKey)}: {ProcessInstanceKey}, {nameof(BpmnProcessId)}: {BpmnProcessId}, {nameof(ProcessDefinitionVersion)}: {ProcessDefinitionVersion}, {nameof(ProcessDefinitionKey)}: {ProcessDefinitionKey}, {nameof(ElementId)}: {ElementId}, {nameof(ElementInstanceKey)}: {ElementInstanceKey}, {nameof(Worker)}: {Worker}, {nameof(Retries)}: {Retries}, {nameof(Deadline)}: {Deadline}, {nameof(Variables)}: {Variables}, {nameof(CustomHeaders)}: {CustomHeaders}";
        }

        protected bool Equals(ActivatedJob other)
        {
            return Key == other.Key && Type == other.Type && ProcessInstanceKey == other.ProcessInstanceKey &&
                   BpmnProcessId == other.BpmnProcessId &&
                   ProcessDefinitionVersion == other.ProcessDefinitionVersion && ProcessDefinitionKey == other.ProcessDefinitionKey &&
                   ElementId == other.ElementId && ElementInstanceKey == other.ElementInstanceKey &&
                   Worker == other.Worker && Retries == other.Retries && Deadline.Equals(other.Deadline) &&
                   Variables == other.Variables && CustomHeaders == other.CustomHeaders;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((ActivatedJob) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Key.GetHashCode();
                hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ProcessInstanceKey.GetHashCode();
                hashCode = (hashCode * 397) ^ (BpmnProcessId != null ? BpmnProcessId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ProcessDefinitionVersion;
                hashCode = (hashCode * 397) ^ ProcessDefinitionKey.GetHashCode();
                hashCode = (hashCode * 397) ^ (ElementId != null ? ElementId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ElementInstanceKey.GetHashCode();
                hashCode = (hashCode * 397) ^ (Worker != null ? Worker.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Retries;
                hashCode = (hashCode * 397) ^ Deadline.GetHashCode();
                hashCode = (hashCode * 397) ^ (Variables != null ? Variables.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CustomHeaders != null ? CustomHeaders.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
