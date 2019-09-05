
In the following you see an example of how to use the Zeebe C# client with CamundaCloud.

The `CamundaCloudTokenProvider` will request an access token from the CamundCloud and store it 
under `~/.zeebe/cloud.token`, such that it is possible to reuse the token.

```csharp
using System.Threading.Tasks;
using NLog;
using Zeebe.Client;
using Zeebe.Client.Builder;

namespace Client.Examples
{
    public class CloudExample
    {
        private const string AuthServer = "https://login.cloud.ultrawombat.com/oauth/token";
        private const string ClientId = "{clientId}";
        private const string ClientSecret = "{clientSecret}";
        private const string Audience = "{cluster-id}.zeebe.ultrawombat.com";
        private const string ZeebeUrl = Audience + ":443";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task Main(string[] args)
        {
            var client =
                ZeebeClient.Builder()
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
        }
    }
}
```
