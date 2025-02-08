﻿//
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
using NLog.Extensions.Logging;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace Client.Examples;

internal class Program
{
    private static readonly string DemoProcessPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "demo-process.bpmn");

    private static readonly string ZeebeUrl = "0.0.0.0:26500";
    private static readonly string ProcessInstanceVariables = "{\"a\":\"123\"}";
    private static readonly string JobType = "payment-service";
    private static readonly string WorkerName = Environment.MachineName;
    private static readonly long WorkCount = 100L;

    public static async Task Main(string[] args)
    {
        // create zeebe client
        var client = ZeebeClient.Builder()
            .UseLoggerFactory(new NLogLoggerFactory())
            .UseGatewayAddress(ZeebeUrl)
            .UsePlainText()
            .Build();

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

        await client.NewSetVariablesCommand(processInstance.ProcessInstanceKey).Variables("{\"wow\":\"this\"}").Local()
            .Send();

        for (var i = 0; i < WorkCount; i++)
            await client
                .NewCreateProcessInstanceCommand()
                .ProcessDefinitionKey(processDefinitionKey)
                .Variables(ProcessInstanceVariables)
                .Send();

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
        // auto completion
    }
}