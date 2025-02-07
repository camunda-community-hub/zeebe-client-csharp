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
using System.Threading;
using GatewayProtocol;
using Grpc.Core;
using Grpc.Core.Logging;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NUnit.Framework;

namespace Zeebe.Client;

public class BaseZeebeTest : IDisposable
{
    public readonly ILoggerFactory LoggerFactory = new NLogLoggerFactory();
    private Server server;

    protected GatewayTestService TestService { get; private set; }

    protected IZeebeClient ZeebeClient { get; private set; }

    [SetUp]
    public void Init()
    {
        GrpcEnvironment.SetLogger(new ConsoleLogger());
        server = new Server();
        _ = server.Ports.Add(new ServerPort("localhost", 26500, ServerCredentials.Insecure));

        TestService = new GatewayTestService();
        var serviceDefinition = Gateway.BindService(TestService);
        server.Services.Add(serviceDefinition);
        server.Start();

        ZeebeClient = Client.ZeebeClient
            .Builder()
            .UseLoggerFactory(LoggerFactory)
            .UseGatewayAddress("localhost:26500")
            .UsePlainText()
            .UseRetrySleepDurationProvider(retryAttempt => TimeSpan.Zero)
            .Build();
    }

    [TearDown]
    public void Stop()
    {
        server.ShutdownAsync();//.Wait();
        TestService.Requests.Clear();
    }

    public void AwaitRequestCount(Type type, int requestCount)
    {
        while (TestService.Requests[type].Count < requestCount)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }
    }

    public void Dispose()
    {
        Stop();
        ZeebeClient.Dispose();
        TestService = null;
        server = null;
        ZeebeClient = null;
    }
}