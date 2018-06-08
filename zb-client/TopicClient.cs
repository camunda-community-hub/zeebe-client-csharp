using System;
namespace zbclient
{
    public class TopicClient
    {
        private string topicName;

        public TopicClient(string topicName)
        {
            this.topicName = topicName;
        }

        public JobClient JobClient(string workerName, string jobType)
        {
            return new JobClient(topicName, workerName, jobType);
        }
    }
}
