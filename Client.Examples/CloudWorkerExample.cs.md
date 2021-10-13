## Camunda Cloud Worker Example
In the following you see an example of how to use the Zeebe C# client job worker with the CamundaCloud.

We have a separate thread which outputs how many jobs with worker has completed in a second.
With that we can play around with the different job worker configuration and see how it affects the throughput.

```csharp
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Extensions.Logging;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Builder;

namespace Client.Examples
{
    internal class CloudWorkerExample
    {
        private static volatile int lastCompleted;
        private static volatile int currentCompleted;
        private static readonly string DemoProcessPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ten_tasks.bpmn");

        private static readonly string PayloadPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "big_payload.json");

        private static readonly string JobType = "benchmark-task";
        private static readonly string WorkerName = Environment.MachineName;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task Main(string[] args)
        {
            // create zeebe client
            var client = 
                CamundaCloudClientBuilder.Builder()
                    .FromEnv()
                    .UseLoggerFactory(new NLogLoggerFactory())
                    .Build();

            var topology = await client.TopologyRequest().Send();

            Logger.Info(topology.ToString);
            Console.WriteLine(topology);

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    Logger.Info("Completed " + (currentCompleted - lastCompleted) + " in the last second.");
                    lastCompleted = currentCompleted;
                }
            });

            // open job worker
            using (var signal = new EventWaitHandle(false, EventResetMode.AutoReset))
            {
                client.NewWorker()
                    .JobType(JobType)
                    .Handler(HandleJob)
                    .MaxJobsActive(120)
                    .Name(WorkerName)
                    .AutoCompletion()
                    .PollInterval(TimeSpan.FromMilliseconds(100))
                    .Timeout(TimeSpan.FromSeconds(10))
                    .PollingTimeout(TimeSpan.FromSeconds(30))
                    .HandlerThreads(8)
                    .Open();

                // blocks main thread, so that worker can run
                signal.WaitOne();
            }
        }

        private static async Task HandleJob(IJobClient jobClient, IJob job)
        {
            await Task.Delay(150);
            jobClient.NewCompleteJobCommand(job).Send();
            currentCompleted++;
        }
    }
}
```