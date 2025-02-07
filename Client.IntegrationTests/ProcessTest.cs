using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Zeebe.Client;

namespace Client.IntegrationTests;

[TestFixture]
public class ProcessTest
{
    [OneTimeSetUp]
    public async Task Setup()
    {
        zeebeClient = await testHelper.SetupIntegrationTest();
    }

    [OneTimeTearDown]
    public async Task Stop()
    {
        await testHelper.TearDownIntegrationTest();
    }

    private static readonly string DemoProcessPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "simpleProcess.bpmn");

    private readonly ZeebeIntegrationTestHelper testHelper = ZeebeIntegrationTestHelper.Latest();
    private IZeebeClient zeebeClient;

    [Test]
    public async Task DeployProcessTest()
    {
        // given

        // when
        var deployResponse = await zeebeClient.NewDeployCommand()
            .AddResourceFile(DemoProcessPath)
            .Send();

        // then
        Assert.Greater(deployResponse.Key, 0);

        var deployedProcesses = deployResponse.Processes;
        Assert.AreEqual(1, deployedProcesses.Count);

        Assert.AreEqual(1, deployedProcesses[0].Version);
        Assert.Greater(deployedProcesses[0].ProcessDefinitionKey, 1);
        Assert.AreEqual("simpleProcess", deployedProcesses[0].BpmnProcessId);
        Assert.AreEqual(DemoProcessPath, deployedProcesses[0].ResourceName);
    }
}