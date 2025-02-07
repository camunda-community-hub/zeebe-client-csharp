//
//    Copyright (c) 2021 camunda services GmbH (info@camunda.com)
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
using System.Threading.Tasks;
using NLog.Extensions.Logging;
using Zeebe.Client.Impl.Builder;

namespace Client.Cloud.Example;

public class Program
{
    public static async Task Main(string[] args)
    {
        var zeebeClient =
            CamundaCloudClientBuilder.Builder()
                .UseClientId("ZEEBE_CLIENT_ID")
                .UseClientSecret("ZEEBE_CLIENT_SECRET")
                .UseContactPoint("ZEEBE_ADDRESS")
                .UseLoggerFactory(new NLogLoggerFactory()) // optional
                .Build();

        var topology = await zeebeClient.TopologyRequest().Send();

        Console.WriteLine("Hello: " + topology);
    }
}