using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;

namespace Client.IntegrationTests;

[TestFixture]
public class OAuthIntegrationTest
{
    private readonly ZeebeIntegrationTestHelper testHelper = ZeebeIntegrationTestHelper.Latest().WithIdentity();

    [OneTimeSetUp]
    public async Task Setup()
    {
        await testHelper.SetupIntegrationTest();
    }

    [OneTimeTearDown]
    public async Task Stop()
    {
        await testHelper.TearDownIntegrationTest();
    }

    [Test]
    public async Task ShouldSendRequestAndNotFailingWithAuthenticatedClient()
    {
        var authenticatedZeebeClient = testHelper.CreateAuthenticatedZeebeClient();
        var topology = await authenticatedZeebeClient.TopologyRequest().Send();
        var gatewayVersion = topology.GatewayVersion;
        Assert.AreEqual(ZeebeIntegrationTestHelper.LatestVersion, gatewayVersion);

        var topologyBrokers = topology.Brokers;
        Assert.AreEqual(1, topologyBrokers.Count);

        var topologyBroker = topologyBrokers[0];
        Assert.AreEqual(0, topologyBroker.NodeId);
    }

    [Test]
    public Task ShouldFailWithUnauthenticatedClient()
    {
        Assert.ThrowsAsync<RpcException>(code: async () =>
        {
            await testHelper.CreateZeebeClient().TopologyRequest().Send();
        });
        return Task.CompletedTask;
    }
}