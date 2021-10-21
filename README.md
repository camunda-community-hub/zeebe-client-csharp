[![Build Status](https://github.com/camunda-community-hub/zeebe-client-csharp/actions/workflows/aspnetcore.yml/badge.svg)](https://github.com/camunda-community-hub/zeebe-client-csharp/actions/workflows/aspnetcore.yml)
[![](https://img.shields.io/nuget/v/zb-client.svg)](https://www.nuget.org/packages/zb-client/) 
[![](https://img.shields.io/nuget/dt/zb-client)](https://www.nuget.org/stats/packages/zb-client?groupby=Version) 
[![](https://img.shields.io/github/license/zeebe-io/zeebe-client-csharp.svg)](https://www.apache.org/licenses/LICENSE-2.0) 
[![Total alerts](https://img.shields.io/lgtm/alerts/g/zeebe-io/zeebe-client-csharp.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/zeebe-io/zb-csharp-client/alerts/)
[![](https://img.shields.io/badge/Community%20Extension-An%20open%20source%20community%20maintained%20project-FF4700)](https://github.com/camunda-community-hub/community)
[![](https://img.shields.io/badge/Lifecycle-Stable-brightgreen)](https://github.com/Camunda-Community-Hub/community/blob/main/extension-lifecycle.md#stable-)




# Zeebe C# client

The Zeebe C# client is a C# wrapper implementation around the GRPC (https://github.com/grpc/grpc) generated Zeebe client.
It makes it possible to communicate with Zeebe Broker via the GRPC protocol, see the [Zeebe documentation](https://docs.zeebe.io/)
for more information about the Zeebe project.

## Requirements

 * .net standard 2.0 or higher, which means
   * .net core 2.1 or higher or
   * .net framework 4.7.1 or higher
 * latest [Zeebe release](https://github.com/zeebe-io/zeebe/releases/)

## How to use

The Zeebe C# client is available via nuget (https://www.nuget.org/packages/zb-client/).

Please have a look at the [API documentation](https://camunda-community-hub.github.io/zeebe-client-csharp/).

## Camunda Cloud

The Zeebe C# Client is Camunda Cloud ready.
To get an example how to use the Zeebe C# Client with the Cloud take a look at [Client.Cloud.Example/](Client.Cloud.Example/).

### Quick start
As quick start you can use the following code:

```csharp
var zeebeClient = CamundaCloudClientBuilder
    .Builder()
      .UseClientId("CLIENT_ID")
      .UseClientSecret("CLIENT_SECRET")
      .UseContactPoint("ZEEBE_ADDRESS")
    .Build();
```

Alternatively you could also read the credentials from the environment:

```csharp
var zeebeClient = CamundaCloudClientBuilder
    .Builder()
      .FromEnv()
    .Build();
```

#### Implementing a worker
A job worker is a service capable of performing a particular task in a process. After having build the C# Zeebe Client as shown in the example above you can create a new worker which subscribes to a certain **JobType**. 


```csharp
using (var signal = new EventWaitHandle(false, EventResetMode.AutoReset)){
  zeebeClient.NewWorker()
    .JobType("worker")
    .Handler(HandleJob)
    .MaxJobsActive(5)
    .Name(Environment.MachineName)
    .AutoCompletion()
    .PollInterval(TimeSpan.FromSeconds(1))
    .Timeout(TimeSpan.FromSeconds(10))
    .Open();

  // blocks main thread, so that worker can run
  signal.WaitOne();
}
```

This code will also call a method called **HandleJob** which can execute your business logic of choice. A stub of this method is show below. You can also see how to pass variables back to Zeebe. 

```csharp
private static void HandleJob(IJobClient jobClient, IJob job){
  // business logic

  //Completion of a job
  jobClient.NewCompleteJobCommand(job.Key)
                    .Variables("{\"foo\":2}")
                    .Send()
                    .GetAwaiter()
                    .GetResult();
}
```

#### Making a typology request
This example shows which broker is leader and follower for which partition. This is particularly useful when you run a cluster with multiple Zeebe brokers. To do so in C# use the code displayed below: 

```csharp
var topology = await client.TopologyRequest().Send();
Console.WriteLine(topology);
```

## How to build

Run `msbuild Zeebe.sln` or `dotnet build Zeebe.sln`

