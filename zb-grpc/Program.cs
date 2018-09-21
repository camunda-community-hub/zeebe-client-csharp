using System;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;

namespace zbgrpc
{
    class MainClass
    {
        public static async Task Main(string[] args)
        {
            Channel channel = new Channel("127.0.0.1:26500", ChannelCredentials.Insecure);
            Console.WriteLine("Sending health request.");

            HealthResponse response = await HealthRequest(channel);


            Console.WriteLine("Got response: " + response.Brokers);
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
