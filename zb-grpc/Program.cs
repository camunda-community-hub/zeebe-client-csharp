using System;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using Zeebe.Impl;
using Zeebe;

namespace zbgrpc
{
    class MainClass
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Sending health request.");

            ZeebeClient client = new ZeebeClient("localhost:26500");
            TopologyResponse response = await client.TopologyRequest();

            var brokers = response.Brokers;
            Console.WriteLine("Got response: " + brokers);
            var enumerator = brokers.GetEnumerator();
            enumerator.MoveNext();
            var partitions = enumerator.Current.Partitions;

            Console.WriteLine("Partitions: " + partitions);

            var parEmumerator = partitions.GetEnumerator();
            parEmumerator.MoveNext();
            var parId = parEmumerator.Current.PartitionId;
            var state = parEmumerator.Current.Role;

            Console.WriteLine("Partition id: " + parId);
            Console.WriteLine("Partition state: " + state);

            await client.WorkflowClient()
                        .NewPublishMessageCommand()
                        .MessageName("messageName")
                        .CorrelationKey("key")
                        .Payload("{}")
                        .Send();
            Console.WriteLine("Publish message.");
        }
    }
}
