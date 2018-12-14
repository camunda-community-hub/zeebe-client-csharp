# Zeebe C# client

The Zeebe C# client is a C# wrapper implementation around the GRPC (https://github.com/grpc/grpc) generated Zeebe client.
It makes it possible to communicate with Zeebe Broker via the GRPC protocol, see the [Zeebe documentation](https://docs.zeebe.io/)
for more information about the Zeebe project.

## Requirements

 * .net 4.7.1
 * latest zeebe release ([0.14.0](https://github.com/zeebe-io/zeebe/releases/tag/0.14.0))

## How to use

The Zeebe C# client is available via nuget (https://www.nuget.org/packages/zb-client/).

## Hot to build

Simply run `msbuild Zeebe.sln` or `dotnet build Zeebe.sln`

## Current supported Features

* Request topology
* JobWorker
* Complete Job
* Fail Job
* Publish Message
* Deploy an resource
* Create a workflow instance
* Update an element instance payload 
* Update retries of an job
* Resolve an existing incident
* Cancel an existing workflow instance
* List all workflows
* Request a workflow resource

## Examples
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
      .Limit(5)
      .Name("zb-worker")
      .PollInterval(TimeSpan.FromSeconds(5))
      .Timeout(10_000L)
      .Open();
```

### Complete an job

```csharp
client.NewCompleteJobCommand(JobKey).Payload("{\"foo\":23}").Send();
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
    .Payload("{\"foo\":\"123\"}")
    .Send();
```

### Update payload of an element instance

```csharp
await client.NewUpdatePayloadCommand(workflowInstanceResponse.WorkflowInstanceKey)
    .Payload("{\"a\":\"newPayload\"}")
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


