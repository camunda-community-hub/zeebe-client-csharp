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

namespace Zeebe.Client.Impl.Responses;

public class ActivatedJob(GatewayProtocol.ActivatedJob activatedJob) : IJob
{
    public long Key { get; } = activatedJob.Key;

    public string Type { get; } = activatedJob.Type;

    public long ProcessInstanceKey { get; } = activatedJob.ProcessInstanceKey;

    public string BpmnProcessId { get; } = activatedJob.BpmnProcessId;

    public int ProcessDefinitionVersion { get; } = activatedJob.ProcessDefinitionVersion;

    public long ProcessDefinitionKey { get; } = activatedJob.ProcessDefinitionKey;

    public string ElementId { get; } = activatedJob.ElementId;

    public long ElementInstanceKey { get; } = activatedJob.ElementInstanceKey;

    public string Worker { get; } = activatedJob.Worker;

    public int Retries { get; } = activatedJob.Retries;

    public DateTime Deadline { get; } = FromUTCTimestamp(activatedJob.Deadline);

    public string Variables { get; } = activatedJob.Variables;

    public string CustomHeaders { get; } = activatedJob.CustomHeaders;

    public string TenantId { get; } = activatedJob.TenantId;

    private static DateTime FromUTCTimestamp(long milliseconds)
    {
        var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddMilliseconds(milliseconds).ToLocalTime();
        return dtDateTime;
    }

    public override string ToString()
    {
        return
            $"{nameof(Key)}: {Key}, {nameof(Type)}: {Type}, {nameof(TenantId)}: {TenantId}, {nameof(ProcessInstanceKey)}: {ProcessInstanceKey}, {nameof(BpmnProcessId)}: {BpmnProcessId}, {nameof(ProcessDefinitionVersion)}: {ProcessDefinitionVersion}, {nameof(ProcessDefinitionKey)}: {ProcessDefinitionKey}, {nameof(ElementId)}: {ElementId}, {nameof(ElementInstanceKey)}: {ElementInstanceKey}, {nameof(Worker)}: {Worker}, {nameof(Retries)}: {Retries}, {nameof(Deadline)}: {Deadline}, {nameof(Variables)}: {Variables}, {nameof(CustomHeaders)}: {CustomHeaders}";
    }

    protected bool Equals(ActivatedJob other)
    {
        return Key == other.Key && Type == other.Type && ProcessInstanceKey == other.ProcessInstanceKey &&
               TenantId == other.TenantId &&
               BpmnProcessId == other.BpmnProcessId &&
               ProcessDefinitionVersion == other.ProcessDefinitionVersion &&
               ProcessDefinitionKey == other.ProcessDefinitionKey &&
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

        if (obj.GetType() != GetType())
    {
      return false;
    }

        return Equals((ActivatedJob)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Key.GetHashCode();
            hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (TenantId != null ? TenantId.GetHashCode() : 0);
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