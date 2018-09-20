using System;
using GatewayProtocol;
using Grpc.Core;

namespace zbgrpc
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Channel channel = new Channel("127.0.0.1:26500", ChannelCredentials.Insecure);
            HealthRequest(channel);

            Console.WriteLine("Send health request.");
        }

        public async static void HealthRequest(Channel channel)
        {
            var client = new Gateway.GatewayClient(channel);

            var reply = client.HealthAsync(new HealthRequest());

            var response = await reply.ResponseAsync;

            Console.WriteLine("Response: " + response.Brokers);
        }



    }
}
