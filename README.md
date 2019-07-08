[![Build Status](https://travis-ci.org/zeebe-io/zeebe-client-csharp.svg?branch=master)](https://travis-ci.org/zeebe-io/zeebe-client-csharp)
[![](https://img.shields.io/nuget/v/zb-client.svg)](https://www.nuget.org/packages/zb-client/) 
[![](https://img.shields.io/github/license/zeebe-io/zeebe-client-csharp.svg)](https://www.apache.org/licenses/LICENSE-2.0) 
[![Total alerts](https://img.shields.io/lgtm/alerts/g/zeebe-io/zb-csharp-client.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/zeebe-io/zb-csharp-client/alerts/) 



# Zeebe C# client

The Zeebe C# client is a C# wrapper implementation around the GRPC (https://github.com/grpc/grpc) generated Zeebe client.
It makes it possible to communicate with Zeebe Broker via the GRPC protocol, see the [Zeebe documentation](https://docs.zeebe.io/)
for more information about the Zeebe project.

## Requirements

 * .net standard 2.0 or higher, which means
   * .net core 2.1 or higher or
   * .net framework 4.7.1 or higher
 * latest [Zeebe release](https://github.com/zeebe-io/zeebe/releases/))

## How to use

The Zeebe C# client is available via nuget (https://www.nuget.org/packages/zb-client/).

## How to build

Simply run `msbuild Zeebe.sln` or `dotnet build Zeebe.sln`

## Current supported Features

* Request topology
* JobWorker
* Activate Jobs
* Complete Job
* Fail Job
* Publish Message
* Deploy an resource
* Create a workflow instance
* Set variables on an element instance
* Update retries of an job
* Resolve an existing incident
* Cancel an existing workflow instance

## Examples

There exist an example project under `Client.Examples/`, which contains some of the examples below.
You can run the example with the following command:

```bash
  dotnet run --project Client.Examples/Client.Examples.csproj 
```

Make sure that you have an broker runing before you execute the examples.
Easiest way to run an broker is to use docker, see the following command:

```
  docker run -p 26500:26500 camunda/zeebe:latest
```

### Client

To create a client use this:

```csharp
  var zeebeClient = ZeebeClient.NewZeebeClient("localhost:26500");
```

### Request topology

```csharp
  ITopology top = await zeebeClient.TopologyRequest().Send(); 
```

### Create an job worker

```csharp
  zeebeClient.NewWorker()
      .JobType("bar")
      .Handler((client, job) =>
      {
        // business logic
      })
      .MaxJobsActive(5)
      .Name("zb-worker")
      .PollInterval(TimeSpan.FromSeconds(5))
      .Timeout(10_000L)
      .Open();
```

### Activate Jobs

```csharp
  zeebeClient.NewActivateJobsCommand()
             .JobType("foo")
             .MaxJobsToActivate(3)
             .Timeout(TimeSpan.FromSeconds(10))
             .WorkerName("jobWorker")
             .FetchVariables("foo", "bar")
             .Send();
```

### Complete an job

```csharp
client.NewCompleteJobCommand(JobKey).Variables("{\"foo\":23}").Send();
```

### Fail an job

```csharp
client.NewFailCommand(job.Key).Retries(job.Retries - 1).ErrorMessage("This job failed.").Send();
```

### Deploy a resource

```csharp
var deployResponse = await client.NewDeployCommand().AddResourceFile(DemoProcessPath).Send();
```

### Create a workflow instance
```csharp
var workflowKey = deployResponse.Workflows[0].WorkflowKey;
var workflowInstanceResponse = await client
    .NewCreateWorkflowInstanceCommand()
    .WorkflowKey(workflowKey)
    .Variables("{\"foo\":\"123\"}")
    .Send();
```

### Set variables on an element instance

```csharp
await client.NewSetVariablesCommand(workflowInstanceResponse.WorkflowInstanceKey)
    .Variables("{\"a\":\"newValue\"}")
    .Send();
```

### Update retries of an job

```csharp
await client.NewUpdateRetriesCommand(45).Retries(2).Send();
```

### Resolve an existing incident

```csharp
await client.NewResolveIncidentCommand(17).Send();
```

### Cancel an existing workflow instance
```csharp
await client.NewCancelInstanceCommand(workflowInstanceResponse.WorkflowInstanceKey).Send();
```

### List all workflows

```csharp
var workflowListResponse = await client.NewListWorkflowRequest().Send();
```

### Request workflow resource

```csharp
var workflowResourceResponse = await client.NewWorkflowResourceRequest().BpmnProcessId("ship-parcel").LatestVersion().Send();
```


