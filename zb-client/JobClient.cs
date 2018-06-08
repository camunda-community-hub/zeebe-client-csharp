using System;
using System.Runtime.InteropServices;

namespace zbclient
{
    public class JobClient
    {
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

        public void Poll(int count)
        {
            string result = PollJob(count);
            Console.WriteLine(result);
        }

    }
}
