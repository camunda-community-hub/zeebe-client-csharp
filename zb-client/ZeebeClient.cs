using System;
using System.Runtime.InteropServices;

namespace zbclient
{
    public class ZeebeClient
    {
        private GoString bootstrapAddress;

        private TopicClient defaultTopic;
        public TopicClient DefaultTopic
        {
            get
            {
                return defaultTopic;
            }
        }

        public ZeebeClient(string address)
        {
            bootstrapAddress = new GoString(address);
            defaultTopic = new TopicClient("default-topic");

            InitClient(bootstrapAddress);
        }


        [DllImport("lib/libzbc-linux-amd64")]
        private static extern String InitClient(GoString bootstrapAddress);

        public TopicClient TopicClient(string topicName)
        {
            return new TopicClient(topicName);
        }
    }
}
