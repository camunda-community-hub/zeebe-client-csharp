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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Builder;

namespace Client.Examples
{
    internal class Program
    {
        private const string AuthServer = "https://login.cloud.ultrawombat.com/oauth/token";
        private const string ClientId = "oIgvkg28ptzmwF85u_HIG~rlu0grvDHS";
        private const string ClientSecret = "vlivfA0bIvfk8sOEy4mnnl9W6w.9y.epLgNphrAf_Ed3soaKOWvSfDUhWzRY7Cdo";
        private const string Audience = "de990736-75b6-4e2b-8b51-65b2ccd9174d.zeebe.ultrawombat.com";
        private const string ZeebeUrl = Audience + ":443";

        private static readonly string DemoProcessPath =
            Path.Combine("/tmp", "xorIncident.bpmn");

        private static readonly string JobType = "benchmark-task";
        private static readonly string WorkerName = Environment.MachineName;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

            var topology = await client.TopologyRequest().Send();

            Logger.Info(topology.ToString);
            Console.WriteLine(topology);
            // deploy
            var deployResponse = await client.NewDeployCommand()
                .AddResourceFile(DemoProcessPath)
                .Send();
            // create process instance
            var processDefinitionKey = deployResponse.Processes[0].ProcessDefinitionKey;
            var processInstanceResult = await client
                .NewCreateProcessInstanceCommand()
                .ProcessDefinitionKey(processDefinitionKey)
                .WithResult()
                .Send();

            Console.Out.WriteLine("ProcessInstanceKey: " + processInstanceResult.ProcessInstanceKey);
        }

        private static void HandleJob(IJobClient jobClient, IJob job)
        {
            Logger.Debug("Handle job {job}", job.Key);
            jobClient.NewCompleteJobCommand(job).Send();
//        }
        }

//        private static readonly string DemoProcessPath =
//            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "demo-process.bpmn");
//
//        private static readonly string ZeebeUrl = "0.0.0.0:26500";
//        private static readonly string ProcessInstanceVariables = "{\"a\":\"123\"}";
//        private static readonly string JobType = "payment-service";
//        private static readonly string WorkerName = Environment.MachineName;
//        private static readonly long WorkCount = 100L;
//
//        public static async Task Main(string[] args)
//        {
//            var text = File.ReadAllText("/home/zell/.camunda/credentials");
//
//            var deserializer = new DeserializerBuilder()
//                .WithNamingConvention(CamelCaseNamingConvention.Instance) // see height_in_inches in sample yml
//                .Build();
//
////yml contains a string containing your YAML
//            var p = deserializer.Deserialize<Dictionary<string, Cluster>>(text);
//        }
//    }
//
//    public class Cluster
//    {
//        public Authentication Auth { get; set; }
//    }
//
//    public class Authentication
//    {
//        public Credentials Credentials { get; set; }
//    }
//
//    public class Credentials
//    {
//        public string Accesstoken { get; set; }
//        public string Tokentype { get; set; }
//        public string Refreshtoken { get; set; }
//        public DateTime Expiry { get; set; }
//    }
    }
}