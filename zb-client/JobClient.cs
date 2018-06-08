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
            public string error { get; set; }
            public Data data { get; set; }
        }

        public class Data
        {
            public IList<Job> jobs { get; set; }
        }

        public class Job
        {
            public string jobKey { get; set; }
            public Object payload { get; set; }
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

        [DllImport("libzbc-linux-amd64")]
        private static extern string JobWorker(GoString topic, GoString workerName, GoString jobType, int timeout, int credits);


        [DllImport("libzbc-linux-amd64")]
        private static extern string PollJob(int numberOfJobs);

        [DllImport("libzbc-linux-amd64")]
        private static extern string CompleteJob(GoString jobKey, GoString payload);

        public IList<Job> Poll(int count)
        {
            string result = PollJob(count);
            Response response = JsonConvert.DeserializeObject<Response>(result);

            if (response.data != null)
                return response.data.jobs;
            else if (response.error != null)
                throw new InvalidOperationException(response.error);
            else
                return new List<Job>();
        }

        public void Complete(string jobKey, string payload)
        {
            GoString goJobKey = new GoString(jobKey);
            GoString goPayload = new GoString(payload);

            string result = CompleteJob(goJobKey, goPayload);
            Console.WriteLine(result);
        }

    }
}
