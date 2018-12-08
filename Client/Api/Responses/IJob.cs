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

namespace Zeebe.Client.Api.Responses
{
    public interface IJob
    {
        /** The unique key of the job */
        long Key { get; }

        /** The type of the job */
        string Type { get; }

        /**
         * Broker-defined headers associated with this job. For example, if this job is created in
         *     the context of workflow instance, the header provide context information on which activity
         *     is executed, etc.
         */
        IJobHeaders Headers { get; }

        /** @return the assigned worker to complete the job */
        string Worker { get; }

        /** @return remaining retries */
        int Retries { get; }

        /**
         * The time until when the job is exclusively assigned to this worker. If the deadline is
         *     exceeded, it can happen that the job is handed to another worker and the work is performed
         *     twice.
         */
        DateTime Deadline { get; }

        /** JSON-formatted payload */
        string Payload { get; }

        /** De-serialized payload as map */
        IDictionary<String, Object> PayloadAsDictionary { get; }

        /** @return de-serialized payload as the given type */
        InstanceType PayloadAsType<InstanceType>();
    }
}