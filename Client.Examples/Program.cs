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
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private static readonly string DemoProcessPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "test-script-worker.bpmn");

        private static readonly string JobType = "AddOneTask";
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

            await client.NewDeployCommand().AddResourceFile(DemoProcessPath).Send();
            await client.NewCreateProcessInstanceCommand().BpmnProcessId("TestScriptWorker").LatestVersion().Send();

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
            Logger.Debug("Handling job: {Key}", job.Key);

            var jobVariables = job.Variables;
            Logger.Debug("Incoming variables: {Variables}", jobVariables);

            var calculation = JsonConvert.DeserializeObject<Calculation>(jobVariables);
            calculation!.AddToCount();
            var result = JsonConvert.SerializeObject(calculation);

            Logger.Debug("Job complete with: {Result}", result);
            await jobClient.NewCompleteJobCommand(job).Variables(result).Send();
        }
    }

    internal class Calculation
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("add")]
        public int Add { get; set; }

        public void AddToCount()
        {
            Count += Add;
        }
    }
}
