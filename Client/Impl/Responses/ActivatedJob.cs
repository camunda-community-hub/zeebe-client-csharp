//
//  Copyright 2018  camunda services gmbh
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
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(milliseconds).ToLocalTime();
            return dtDateTime;
        }

        public ActivatedJob(GatewayProtocol.ActivatedJob activatedJob)
        {
            Key = activatedJob.Key;
            Type = activatedJob.Type;
            Headers = new JobHeaders(activatedJob.JobHeaders);
            Worker = activatedJob.Worker;
            Retries = activatedJob.Retries;
            Deadline = FromUTCTimestamp(activatedJob.Deadline);
            Payload = activatedJob.Payload;
            PayloadAsDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(Payload);
        }

        public long Key { get; set; }

        public string Type { get; set; }

        public IJobHeaders Headers { get; set; }

        public string Worker { get; set; }

        public int Retries { get; set; }

        public DateTime Deadline { get; set; }

        public string Payload { get; set; }

        public IDictionary<string, object> PayloadAsDictionary { get; set; }

        public InstanceType PayloadAsType<InstanceType>() {
            return JsonConvert.DeserializeObject<InstanceType>(Payload);
        }
    }
}
