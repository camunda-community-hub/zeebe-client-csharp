using System;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using Zeebe.Impl;

namespace zbgrpc
{
    class MainClass
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Sending health request.");

            ZeebeClient client = new ZeebeClient("localhost:26500");
            HealthResponse response = await client.HealtRequest();

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


            client.PublishMessage();
            Console.WriteLine("Publish message.");
        }

        public static async Task<HealthResponse> HealthRequest(Channel channel)
        {
            var client = new Gateway.GatewayClient(channel);
            var reply = client.HealthAsync(new HealthRequest());

            Console.WriteLine("Send health request.");

            var response = await reply.ResponseAsync;
            return response;
        }



    }
}
