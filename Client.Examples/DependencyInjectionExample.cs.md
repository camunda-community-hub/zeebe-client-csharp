﻿## Dependency Injection Example

In the following you see an example of how to use the Zeebe C# client with dependency injection.

```csharp
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Builder;

namespace Client.DependencyInjectionExamples
{
    internal class Program
    {
        private static IServiceProvider serviceProvider;

        private static readonly string DemoProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "demo-process.bpmn");
        private static readonly string ZeebeUrl = "0.0.0.0:26500";
        private static readonly string ProcessInstanceVariables = "{\"a\":\"123\"}";
        private static readonly string JobType = "payment-service";
        private static readonly string WorkerName = Environment.MachineName;
        private static readonly long WorkCount = 100L;

        public static async Task Main(string[] args)
        {
            BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                // create zeebe client
                var client = scope.ServiceProvider.GetRequiredService<IZeebeClient>();

                var topology = await client.TopologyRequest()
                    .Send();
                Console.WriteLine(topology);
                await client.NewPublishMessageCommand()
                    .MessageName("csharp")
                    .CorrelationKey("wow")
                    .Variables("{\"realValue\":2}")
                    .Send();

                // deploy
                var deployResponse = await client.NewDeployCommand()
                    .AddResourceFile(DemoProcessPath)
                    .Send();

                // create process instance
                var processDefinitionKey = deployResponse.Processes[0].ProcessDefinitionKey;

                var processInstance = await client
                    .NewCreateProcessInstanceCommand()
                    .ProcessDefinitionKey(processDefinitionKey)
                    .Variables(ProcessInstanceVariables)
                    .Send();

                await client.NewSetVariablesCommand(processInstance.ProcessInstanceKey)
                    .Variables("{\"wow\":\"this\"}")
                    .Local()
                    .Send();

                for (var i = 0; i < WorkCount; i++)
                {
                    await client
                        .NewCreateProcessInstanceCommand()
                        .ProcessDefinitionKey(processDefinitionKey)
                        .Variables(ProcessInstanceVariables)
                        .Send();
                }

                // open job worker
                using (var signal = new EventWaitHandle(false, EventResetMode.AutoReset))
                {
                    client.NewWorker()
                          .JobType(JobType)
                          .Handler(HandleJob)
                          .MaxJobsActive(5)
                          .Name(WorkerName)
                          .AutoCompletion()
                          .PollInterval(TimeSpan.FromSeconds(1))
                          .Timeout(TimeSpan.FromSeconds(10))
                          .Open();

                    // blocks main thread, so that worker can run
                    signal.WaitOne();
                }
            }
        }

        private static void BuildServiceProvider()
        {
            var services = new ServiceCollection();

            // https://github.com/NLog/NLog/wiki/Getting-started-with-.NET-Core-2---Console-application
            serviceProvider = services.AddLogging(loggingBuilder =>
                {
                    var config = new ConfigurationBuilder()
                        .SetBasePath(System.IO.Directory.GetCurrentDirectory()) //From NuGet Package Microsoft.Extensions.Configuration.Json
                        .Build();

                    // configure Logging with NLog
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                    loggingBuilder.AddNLog(config);

                })
                .AddZeebeBuilders()
                .AddScoped(sp =>
                {
                    var builder = sp.GetRequiredService<IZeebeClientBuilder>();
                    return builder
                        .UseGatewayAddress(ZeebeUrl)
                        .UsePlainText()
                        .Build();
                }).BuildServiceProvider();
        }

        private static void HandleJob(IJobClient jobClient, IJob job)
        {
            // business logic
            var jobKey = job.Key;
            Console.WriteLine("Handling job: " + job);

            if (jobKey % 3 == 0)
            {
                jobClient.NewCompleteJobCommand(jobKey)
                    .Variables("{\"foo\":2}")
                    .Send()
                    .GetAwaiter()
                    .GetResult();
            }
            else if (jobKey % 2 == 0)
            {
                jobClient.NewFailCommand(jobKey)
                    .Retries(job.Retries - 1)
                    .ErrorMessage("Example fail")
                    .Send()
                    .GetAwaiter()
                    .GetResult();
            }
            else
            {
                // auto completion
            }
        }
    }
}
```
