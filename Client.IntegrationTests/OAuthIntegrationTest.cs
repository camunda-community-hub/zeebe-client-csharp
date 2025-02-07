using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;

namespace Client.IntegrationTests;

[TestFixture]
public class OAuthIntegrationTest
{
    [OneTimeSetUp]
    public async Task Setup()
    {
    _ = await testHelper.SetupIntegrationTest();
    }

    [OneTimeTearDown]
    public async Task Stop()
    {
        await testHelper.TearDownIntegrationTest();
    }

    private readonly ZeebeIntegrationTestHelper testHelper = ZeebeIntegrationTestHelper.Latest().WithIdentity();

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
    _ = Assert.ThrowsAsync<RpcException>(async () =>
    {
      _ = await testHelper.CreateZeebeClient().TopologyRequest().Send();
    });
    return Task.CompletedTask;
    }
}