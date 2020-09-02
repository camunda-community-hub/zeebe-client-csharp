//
//    Copyright (c) 2018 camunda services GmbH (info@camunda.com)
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using NLog.Extensions.Logging;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Builder;

namespace Client.Examples
{
    internal class Program
    {
        private const string AuthServer = "https://login.cloud.ultrawombat.com/oauth/token";
        private const string ClientId = "kE5msLdwGU4Qr.Ah9bhb0ZHvl8PyUKBb";
        private const string ClientSecret = "w6.yfQIo-9c2uB.uvq.I2zeJrpUTFSnZiA4m2CV.wzW9Dxd2wcrQ6JXRQ0AUAruw";
        private const string Audience = "b2fb77dd-5574-485c-a64e-9c662a99f4e3.zeebe.ultrawombat.com";
        private const string ZeebeUrl = Audience + ":443";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly string DemoProcessPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "one_task.bpmn");

        private static readonly string PayloadPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "big_payload.json");

        private static readonly string WorkflowInstanceVariables = "{\"a\":\"123\"}";
        private static readonly string JobType = "benchmark-task";
        private static readonly string WorkerName = Environment.MachineName;
        private static readonly long WorkCount = 100L;

        public static async Task Main(string[] args)
        {
            // create zeebe client
            var client = ZeebeClient.Builder()
                .UseLoggerFactory(new NLogLoggerFactory())
                .UseGatewayAddress(ZeebeUrl)
                .UseTransportEncryption()
                .UseAccessTokenSupplier(
                    CamundaCloudTokenProvider.Builder()
                        .UseAuthServer(AuthServer)
                        .UseClientId(ClientId)
                        .UseClientSecret(ClientSecret)
                        .UseAudience(Audience)
                        .Build())
                .Build();

            var topology = await client.TopologyRequest()
                .Send();
            Console.WriteLine(topology);

            // deploy
            var deployResponse = await client.NewDeployCommand()
                .AddResourceFile(DemoProcessPath)
                .Send();

            // create workflow instance
//            var workflowKey = deployResponse.Workflows[0].WorkflowKey;
//            var bigPayload = File.ReadAllText(PayloadPath);

            Task.Run(async () =>
            {
                while (true)
                {
                    var start = DateTime.Now;
                    for (var i = 0; i < 300; i++)
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                                var intArray = Enumerable.Range(0, 100).ToArray();
                                var jsonObject = new {list = intArray};
                                var jsonString = JsonConvert.SerializeObject(jsonObject);

                                client
                                    .NewCreateWorkflowInstanceCommand()
                                    .BpmnProcessId("benchmark")
                                    .LatestVersion()
//                                .Variables(jsonString)
                                    .Send();
                            }
                            catch (Exception e)
                            {
                                Logger.Error("Problem on creating instances", e);
                            }
                        });
                    }

                    Logger.Debug("Created 10 instances");
                    var endTime = DateTime.Now;
                    var diff = endTime.Millisecond - start.Millisecond;
                    if (diff < 1000)
                    {
                        Thread.Sleep(1000 - diff);
                    }
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

        private static void HandleJob(IJobClient jobClient, IJob job)
        {
            // business logic
            var jobKey = job.Key;
            Console.WriteLine("Handling job: " + job);

//            var bigPayload = File.ReadAllText(PayloadPath);
            jobClient.NewCompleteJobCommand(job)
//                .Variables(JsonConvert.SerializeObject(new {output = bigPayload}))
                .Send();
        }
    }
}
