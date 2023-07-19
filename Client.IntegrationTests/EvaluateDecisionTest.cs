// 
//     Copyright (c) 2021 camunda services GmbH (info@camunda.com)
// 
//     Licensed under the Apache License, Version 2.0 (the "License");
//     you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
// 
//         http://www.apache.org/licenses/LICENSE-2.0
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.

using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Zeebe.Client;

namespace Client.IntegrationTests;

[TestFixture]
public class EvaluateDecisionTest
{
    private static readonly string DecisionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "dinnerDecisions.dmn");
    private static readonly string DevisionEvaluationVariables = "{\"season\":\"Fall\", \"guestCount\":12}";

    private readonly ZeebeIntegrationTestHelper testHelper = ZeebeIntegrationTestHelper.Latest();
    private IZeebeClient zeebeClient;

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

    [Test]
    public async Task ShouldEvaluateDecision()
    {
        // given
        var deployResponse = await zeebeClient.NewDeployCommand()
            .AddResourceFile(DecisionPath)
            .Send();
        var decisionKey = deployResponse.Decisions[0].DecisionKey;

        // when
        var evaluateDecisionResponse = await zeebeClient
            .NewEvaluateDecisionCommand()
            .DecisionKey(decisionKey)
            .Variables(DevisionEvaluationVariables)
            .Send();

        // then
        Assert.AreEqual(evaluateDecisionResponse.DecisionVersion, 1);
        Assert.AreEqual(decisionKey, evaluateDecisionResponse.DecisionKey);
        Assert.AreEqual("dish", evaluateDecisionResponse.DecisionId);
        Assert.Greater(evaluateDecisionResponse.DecisionKey, 1);
        // right now it seems the DMN engine returns an double quated string
        Assert.AreEqual("\"Stew\"", evaluateDecisionResponse.DecisionOutput);
    }
}