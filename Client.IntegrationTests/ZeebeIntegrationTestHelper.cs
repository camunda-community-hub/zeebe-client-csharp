using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Zeebe.Client;
using Zeebe.Client.Impl.Builder;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace Client.IntegrationTests;

public class ZeebeIntegrationTestHelper
{
    public const string LatestVersion = "8.3.0";

    private const ushort ZeebePort = 26500;
    private const ushort KeycloakPort = 8080;
    private const ushort IdentityPort = 8084;
    private readonly string audience;
    public readonly ILoggerFactory LoggerFactory;

    private readonly string version;
    private IZeebeClient client;
    private int count = 1;
    private IContainer identityContainer;
    private IContainer keycloakContainer;
    private IContainer postgresContainer;
    private bool withIdentity;

    private IContainer zeebeContainer;

    private ZeebeIntegrationTestHelper(string version)
    {
        this.version = version;
        audience = Guid.NewGuid().ToString();
        LoggerFactory = new NLogLoggerFactory();
    }

    public static ZeebeIntegrationTestHelper Latest()
    {
        return new ZeebeIntegrationTestHelper(LatestVersion);
    }

    public ZeebeIntegrationTestHelper WithPartitionCount(int count)
    {
        this.count = count;
        return this;
    }

    public static ZeebeIntegrationTestHelper OfVersion(string version)
    {
        return new ZeebeIntegrationTestHelper(version);
    }

    public ZeebeIntegrationTestHelper WithIdentity()
    {
        withIdentity = true;
        return this;
    }

    public async Task<IZeebeClient> SetupIntegrationTest()
    {
        TestcontainersSettings.Logger = LoggerFactory.CreateLogger<ZeebeIntegrationTestHelper>();

        if (withIdentity)
        {
            var network = new NetworkBuilder()
                .WithName(Guid.NewGuid().ToString("D"))
                .Build();

            postgresContainer = CreatePostgresContainer(network);
            await postgresContainer.StartAsync();
            keycloakContainer = CreateKeyCloakContainer(network);
            await keycloakContainer.StartAsync();

            identityContainer = CreateIdentityContainer(network);
            await identityContainer.StartAsync();
            zeebeContainer = CreateZeebeContainer(network);
        }
        else
        {
            zeebeContainer = CreateZeebeContainer();
        }

        await zeebeContainer.StartAsync();

        if (withIdentity)
        {
            client = CreateAuthenticatedZeebeClient();
        }
        else
        {
            client = CreateZeebeClient();
        }

        await AwaitBrokerReadiness();
        return client;
    }

    public async Task TearDownIntegrationTest()
    {
        client.Dispose();
        client = null;
        if (withIdentity)
        {
            await postgresContainer.StopAsync();
            postgresContainer = null;
            await keycloakContainer.StopAsync();
            keycloakContainer = null;
            await identityContainer.StopAsync();
            identityContainer = null;
        }

        await zeebeContainer.StopAsync();
        zeebeContainer = null;
    }

    private IContainer CreateZeebeContainer(INetwork network = null)
    {
        var containerBuilder = new ContainerBuilder()
            .WithImage(new DockerImage("camunda", "zeebe", version))
            .WithPortBinding(ZeebePort, true)
            .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
            .WithEnvironment("ZEEBE_BROKER_CLUSTER_PARTITIONSCOUNT", count.ToString());

        if (withIdentity)
        {
            containerBuilder = containerBuilder.WithEnvironment("ZEEBE_BROKER_GATEWAY_SECURITY_AUTHENTICATION_MODE",
                    "identity")
                .WithEnvironment(
                    "ZEEBE_BROKER_GATEWAY_SECURITY_AUTHENTICATION_IDENTITY_ISSUERBACKENDURL",
                    "http://integration-keycloak:8080/auth/realms/camunda-platform")
                .WithEnvironment("ZEEBE_BROKER_GATEWAY_SECURITY_AUTHENTICATION_IDENTITY_AUDIENCE",
                    "zeebe-api")
                .WithEnvironment("ZEEBE_BROKER_GATEWAY_SECURITY_ENABLED", "true")
                .WithEnvironment("ZEEBE_BROKER_GATEWAY_SECURITY_CERTIFICATECHAINPATH", "/security/chain.cert.pem")
                .WithEnvironment("ZEEBE_BROKER_GATEWAY_SECURITY_PRIVATEKEYPATH", "/security/private.key.pem")
                .WithResourceMapping(new DirectoryInfo("./Resources/Broker"), "/security")
                .WithNetwork(network);
        }

        containerBuilder = containerBuilder.WithAutoRemove(true);
        return containerBuilder.Build();
    }

    private IContainer CreatePostgresContainer(INetwork network)
    {
        var containerBuilder = new ContainerBuilder()
            .WithImage("postgres")
            .WithName("integration-postgres")
            .WithPortBinding(5432, true)
            .WithEnvironment("POSTGRES_DB", "bitnami_keycloak")
            .WithEnvironment("POSTGRES_USER", "bn_keycloak")
            .WithEnvironment("POSTGRES_PASSWORD", "#3]O?4RGj)DE7Z!9SA5")
            .WithNetwork(network)
            .WithAutoRemove(true)
            .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432));

        return containerBuilder.Build();
    }

    private IContainer CreateIdentityContainer(INetwork network)
    {
        var containerBuilder = new ContainerBuilder()
            .WithImage(new DockerImage("camunda", "identity", version)) // identity and zeebe will have the same version
            .WithName("integration-identity")
            .WithPortBinding(IdentityPort, true)
            .WithEnvironment("SERVER_PORT", IdentityPort.ToString())
            .WithEnvironment("IDENTITY_RETRY_DELAY_SECONDS", "30")
            .WithEnvironment("KEYCLOAK_URL", "http://integration-keycloak:8080/auth")
            .WithEnvironment("IDENTITY_AUTH_PROVIDER_BACKEND_URL",
                "http://integration-keycloak:8080/auth/realms/camunda-platform")
            .WithEnvironment("IDENTITY_DATABASE_HOST", "integration-postgres")
            .WithEnvironment("IDENTITY_DATABASE_PORT", "5432")
            .WithEnvironment("IDENTITY_DATABASE_NAME", "bitnami_keycloak")
            .WithEnvironment("IDENTITY_DATABASE_USERNAME", "bn_keycloak")
            .WithEnvironment("IDENTITY_DATABASE_PASSWORD", "#3]O?4RGj)DE7Z!9SA5")
            .WithEnvironment("KEYCLOAK_INIT_ZEEBE_NAME", "zeebe")
            .WithEnvironment("KEYCLOAK_CLIENTS_0_NAME", "zeebe")
            .WithEnvironment("KEYCLOAK_CLIENTS_0_ID", "zeebe")
            .WithEnvironment("KEYCLOAK_CLIENTS_0_SECRET", "sddh123865WUS)(1%!")
            .WithEnvironment("KEYCLOAK_CLIENTS_0_TYPE", "M2M")
            .WithEnvironment("KEYCLOAK_CLIENTS_0_PERMISSIONS_0_RESOURCE_SERVER_ID", "zeebe-api")
            .WithEnvironment("KEYCLOAK_CLIENTS_0_PERMISSIONS_0_DEFINITION", "write:*")
            .WithEnvironment("RESOURCE_PERMISSIONS_ENABLED", "false")
            .WithAutoRemove(true)
            .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
            .WithNetwork(network);

        return containerBuilder.Build();
    }

    private IContainer CreateKeyCloakContainer(INetwork network)
    {
        var containerBuilder = new ContainerBuilder()
            .WithImage(new DockerImage("bitnami", "keycloak", "21.1.2"))
            .WithName("integration-keycloak")
            .WithPortBinding("8080", true)
            .WithEnvironment("KEYCLOAK_HTTP_RELATIVE_PATH", "/auth")
            .WithEnvironment("KEYCLOAK_DATABASE_HOST", "integration-postgres")
            .WithEnvironment("KEYCLOAK_DATABASE_PASSWORD", "#3]O?4RGj)DE7Z!9SA5")
            .WithEnvironment("KEYCLOAK_ADMIN_USER", "admin")
            .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
            .WithNetwork(network)
            .WithAutoRemove(true)
            .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(request =>
                request.ForPort(8080).ForPath("/auth").ForStatusCode(HttpStatusCode.OK)));

        return containerBuilder.Build();
    }

    public IZeebeClient CreateZeebeClient()
    {
        var loggerFactory = LoggerFactory;
        var host = zeebeContainer.Hostname + ":" + zeebeContainer.GetMappedPublicPort(ZeebePort);

        return ZeebeClient.Builder()
            .UseLoggerFactory(loggerFactory)
            .UseGatewayAddress(host)
            .UsePlainText()
            .Build();
    }

    public IZeebeClient CreateAuthenticatedZeebeClient()
    {
        var loggerFactory = LoggerFactory;
        var host = zeebeContainer.Hostname + ":" + zeebeContainer.GetMappedPublicPort(ZeebePort);

        return ZeebeClient.Builder()
            .UseLoggerFactory(loggerFactory)
            .UseGatewayAddress(host)
            .UseTransportEncryption()
            .AllowUntrustedCertificates()
            .UseAccessTokenSupplier(
                new CamundaCloudTokenProviderBuilder()
                    .UseAuthServer(
                        $"http://{keycloakContainer.Hostname}:{keycloakContainer.GetMappedPublicPort(KeycloakPort)}/auth/realms/camunda-platform/protocol/openid-connect/token")
                    .UseClientId("zeebe")
                    .UseClientSecret("sddh123865WUS)(1%!")
                    .UseAudience(audience).Build()).Build();
    }

    private async Task AwaitBrokerReadiness()
    {
        var zeebeClient = withIdentity ? (ZeebeClient)CreateAuthenticatedZeebeClient() : (ZeebeClient)client;
        await zeebeClient.Connect();
        var topologyErrorLogger = LoggerFactory.CreateLogger<ZeebeIntegrationTestHelper>();
        var ready = false;
        var retries = 0;
        var maxCount = 1_000_000;
        bool continueLoop;
        do
        {
            try
            {
                var topology = await zeebeClient.TopologyRequest().Send(TimeSpan.FromSeconds(1));
                ready = topology.Brokers[0].Partitions.Count >= count;
                topologyErrorLogger.LogInformation("Requested topology [retries {Retries}], got '{Topology}'", retries,
                    topology);
            }
            catch (Exception e)
            {
                topologyErrorLogger.LogError(e, "Exception in sending topology");
                // retry
            }

            continueLoop = !ready && maxCount > retries++;
            if (continueLoop)
            {
                await Task.Delay(1 * 1000);
            }
        }
        while (continueLoop);
    }
}