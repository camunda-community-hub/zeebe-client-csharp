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

#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace Zeebe.Client.Helpers;

public class BaseZeebeTest
{
    private IHost? host;

    protected IRequestHandlerRegistration TestService => host?.Services.GetService<IRequestHandlerRegistration>() ??
                                                         throw new ArgumentNullException(nameof(TestService),
                                                             "cannot resolve IRequestHandlerRegistration");

    protected IZeebeClient ZeebeClient { get; private set; } = null!;

    [OneTimeSetUp]
    public void Init()
    {
        host = HostBuilderHelpers.BuildSimpleHost();

        ZeebeClient = Client.ZeebeClient
            .Builder()
            .UseGatewayAddress("http://localhost:5000")
            .UsePlainText()
            .UseRetrySleepDurationProvider(_ => TimeSpan.Zero)
            .Build();
    }

    [OneTimeTearDown]
    public async Task Stop()
    {
        ZeebeClient.Dispose();
        if (host is not null)
        {
            await host.StopAsync();
        }
    }

    public void AwaitRequestCount(Type type, int requestCount)
    {
        while (TestService.Requests[type].Count < requestCount)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }
    }
}