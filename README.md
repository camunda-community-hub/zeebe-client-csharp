[![Build Status](https://github.com/camunda-community-hub/zeebe-client-csharp/actions/workflows/aspnetcore.yml/badge.svg)](https://github.com/camunda-community-hub/zeebe-client-csharp/actions/workflows/aspnetcore.yml)
[![](https://img.shields.io/nuget/v/zb-client.svg)](https://www.nuget.org/packages/zb-client/) 
[![](https://img.shields.io/nuget/dt/zb-client)](https://www.nuget.org/stats/packages/zb-client?groupby=Version) 
[![](https://img.shields.io/github/license/zeebe-io/zeebe-client-csharp.svg)](https://www.apache.org/licenses/LICENSE-2.0) 
[![Total alerts](https://img.shields.io/lgtm/alerts/g/zeebe-io/zeebe-client-csharp.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/zeebe-io/zb-csharp-client/alerts/)
[![](https://img.shields.io/badge/Community%20Extension-An%20open%20source%20community%20maintained%20project-FF4700)](https://github.com/camunda-community-hub/community)
[![](https://img.shields.io/badge/Lifecycle-Stable-brightgreen)](https://github.com/Camunda-Community-Hub/community/blob/main/extension-lifecycle.md#stable-)
![Compatible with: Camunda Platform 8](https://img.shields.io/badge/Compatible%20with-Camunda%20Platform%208-0072Ce)




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

var topology = await zeebeClient.TopologyRequest().Send();
```

Alternatively you could also read the credentials from the environment:

```csharp
var zeebeClient = CamundaCloudClientBuilder
    .Builder()
      .FromEnv()
    .Build();

var topology = await zeebeClient.TopologyRequest().Send();
```

## How to build

Run `msbuild Zeebe.sln` or `dotnet build Zeebe.sln`

