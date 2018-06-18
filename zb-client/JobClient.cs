using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace zbclient
{
    public class JobClient
    {

        public class Response
        {
            public string Error { get; set; }
            public Data Data { get; set; }
        }

        public class Data
        {
            public IList<Job> Jobs { get; set; }
        }

        public class Job
        {
            public string JobKey { get; set; }
            public Object Payload { get; set; }
        }

        private const int timeout = 60_000;
        private const int credits = 10;

        private GoString topicName;
        private GoString workerName;
        private GoString jobType;

        public JobClient(string topicName, string workerName, string jobType)
        {
            this.topicName = new GoString(topicName);
            this.workerName = new GoString(workerName);
            this.jobType = new GoString(jobType);

            JobWorker(this.topicName, this.workerName, this.jobType, timeout, credits);
        }

        [DllImport("lib/libzbc-linux-amd64")]
        private static extern string JobWorker(GoString topic, GoString workerName, GoString jobType, int timeout, int credits);


        [DllImport("lib/libzbc-linux-amd64")]
        private static extern string PollJob(int numberOfJobs);

        [DllImport("lib/libzbc-linux-amd64")]
        private static extern string CompleteJob(GoString jobKey, GoString payload);

        [DllImport("lib/libzbc-linux-amd64")]
        private static extern string FailJob(GoString jobKey);

        public IList<Job> Poll(int count)
        {
            string result = PollJob(count);
            Response response = JsonConvert.DeserializeObject<Response>(result);

            if (response.Data != null)
                return response.Data.Jobs;
            else if (response.Error != null)
                throw new InvalidOperationException(response.Error);
            else
                return new List<Job>();
        }

        public void Complete(string jobKey, string payload)
        {
            GoString goJobKey = new GoString(jobKey);
            GoString goPayload = new GoString(payload);

            CompleteJob(goJobKey, goPayload);
        }

        public void Fail(string jobKey)
        {
            GoString goJobKey = new GoString(jobKey);

            FailJob(goJobKey);
        }

    }
}
