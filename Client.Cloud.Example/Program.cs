using System;
using System.Threading.Tasks;
using NLog.Extensions.Logging;
using Zeebe.Client;
using Zeebe.Client.Api.Builder;
using Zeebe.Client.Impl.Builder;

namespace Client.Cloud.Example
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var zeebeClient =
                    CamundaCloudClientBuilder.Builder().FromEnv()
                    .UseLoggerFactory(new NLogLoggerFactory()) // optional
                    .Build();

            var topology = await zeebeClient.TopologyRequest().Send();

            Console.WriteLine("Hello: " + topology);
        }
    }
}