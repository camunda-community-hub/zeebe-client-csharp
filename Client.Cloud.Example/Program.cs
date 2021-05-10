using System;
using System.Threading.Tasks;
using NLog.Extensions.Logging;
using Zeebe.Client.Impl.Builder;

namespace Client.Cloud.Example
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var zeebeClient = CamundaCloudClientBuilder
                .Builder()
                .UseClientId(args[0])
                .UseClientSecret(args[1])
                .UseContactPoint(args[2])
                .UseAuthServer("https://login.cloud.ultrawombat.com/oauth/token") // optional
                .UseLoggerFactory(new NLogLoggerFactory()) // optional
                .Build();

            var topology = await zeebeClient.TopologyRequest().Send();

            Console.WriteLine("Hello: " + topology);
        }
    }
}