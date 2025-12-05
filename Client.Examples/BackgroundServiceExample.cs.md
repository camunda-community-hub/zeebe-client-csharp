## Using Job Worker In BackgroundService Example

This example showcases how Zeebe job worker can be used in ASP.NET BackgroundService with asynchronous job handler method.

It uses the job handler delegate variant that supports injection of the job worker cancellation token.

It also passes the BackroundService stopping token to the job worker, so that cancellation of the background service is propagated to the worker.

```csharp
using Microsoft.Extensions.Hosting;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace MyWebApplication;

public class ZeebeBackgroundService : BackgroundService
{
    private readonly IZeebeClient client;

    public ZeebeBackgroundService(IZeebeClient client)
    {
        this.client = client;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.client.NewWorker()
            .JobType("My-Job")
            .Handler(HandleJobAsync)
            .MaxJobsActive(5)
            .Name(Environment.MachineName)
            .AutoCompletion()
            .PollInterval(TimeSpan.FromSeconds(1))
            .Timeout(TimeSpan.FromSeconds(10))
            .PollingTimeout(TimeSpan.FromSeconds(30))
            .Open(stoppingToken); // Passes the stopping token to the worker to gracefully cancel it in case of background service cancellation.

        return Task.CompletedTask;
    }

    private static async Task HandleJobAsync(IJobClient jobClient, IJob job, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

        await jobClient
            .NewCompleteJobCommand(job)
            .Send(cancellationToken);
    }
}
```