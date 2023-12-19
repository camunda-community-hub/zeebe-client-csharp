using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GatewayProtocol;
using NUnit.Framework;

namespace Zeebe.Client;

[TestFixture]
public class ModifyProcessInstanceTest : BaseZeebeTest
{
    private readonly long testProcessInstanceKey = 1234567890;
    private readonly long testAncestorElementInstanceKey = 2345678900;
    private readonly long testElementInstanceKey = 3456789000;
    private readonly string testElementId = "element1";
    private readonly string testVariables = "variable1";
    private readonly string testScopeId = "scope1";

    [Test]
    public async Task ShouldSendRequestAsExpectedWithElementIdOnly()
    {
        // given
        var expectedRequest = new ModifyProcessInstanceRequest
        {
            ProcessInstanceKey = testProcessInstanceKey,
            ActivateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.ActivateInstruction
                {
                    ElementId = testElementId,
                    AncestorElementInstanceKey = 0
                }
            },
            TerminateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.TerminateInstruction()
                {
                    ElementInstanceKey = testElementInstanceKey
                }
            }
        };

        // when
        await ZeebeClient.NewModifyProcessInstanceCommand(processInstanceKey: testProcessInstanceKey)
            .AddInstructionToActivate(elementId: testElementId)
            .AddInstructionToTerminate(elementInstanceKey: testElementInstanceKey)
            .Send();

        // then
        var request = TestService.Requests[typeof(ModifyProcessInstanceRequest)][0];
        var typedRequest = request as ModifyProcessInstanceRequest;
        Assert.NotNull(typedRequest);
        Assert.AreEqual(expectedRequest, typedRequest);
    }

    [Test]
    public async Task ShouldSendRequestAsExpectedWithElementIdAndAncestorElementInstanceKey()
    {
        // given
        var expectedRequest = new ModifyProcessInstanceRequest
        {
            ProcessInstanceKey = testProcessInstanceKey,
            ActivateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.ActivateInstruction
                {
                    ElementId = testElementId,
                    AncestorElementInstanceKey = testAncestorElementInstanceKey
                }
            },
            TerminateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.TerminateInstruction()
                {
                    ElementInstanceKey = testElementInstanceKey
                }
            }
        };

        // when
        await ZeebeClient.NewModifyProcessInstanceCommand(processInstanceKey: testProcessInstanceKey)
            .AddInstructionToActivate(elementId: testElementId, ancestorElementInstanceKey: testAncestorElementInstanceKey)
            .AddInstructionToTerminate(elementInstanceKey: testElementInstanceKey)
            .Send();

        // then
        var request = TestService.Requests[typeof(ModifyProcessInstanceRequest)][0];
        var typedRequest = request as ModifyProcessInstanceRequest;
        Assert.NotNull(typedRequest);
        Assert.AreEqual(expectedRequest, typedRequest);
    }

    [Test]
    public async Task ShouldSendRequestAsExpectedWithElementIdAndVariableInstructions()
    {
        // given
        var variableInstructions = new List<ModifyProcessInstanceRequest.Types.VariableInstruction>
        {
            new () { Variables = testVariables, ScopeId = testScopeId }
        };

        var expectedRequest = new ModifyProcessInstanceRequest
        {
            ProcessInstanceKey = testProcessInstanceKey,
            ActivateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.ActivateInstruction
                {
                    ElementId = testElementId,
                    AncestorElementInstanceKey = 0,
                    VariableInstructions = { variableInstructions }
                }
            },
            TerminateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.TerminateInstruction()
                {
                    ElementInstanceKey = testElementInstanceKey
                }
            }
        };

        // when
        await ZeebeClient.NewModifyProcessInstanceCommand(processInstanceKey: testProcessInstanceKey)
            .AddInstructionToActivate(elementId: testElementId, variableInstructions: variableInstructions)
            .AddInstructionToTerminate(elementInstanceKey: testElementInstanceKey)
            .Send();

        // then
        var request = TestService.Requests[typeof(ModifyProcessInstanceRequest)][0];
        var typedRequest = request as ModifyProcessInstanceRequest;
        Assert.NotNull(typedRequest);
        Assert.AreEqual(expectedRequest, typedRequest);
    }

    [Test]
    public async Task ShouldSendRequestAsExpectedWithElementIdAndAncestorElementInstanceKeyAndVariableInstructions()
    {
        // given
        var variableInstructions = new List<ModifyProcessInstanceRequest.Types.VariableInstruction>
        {
            new () { Variables = testVariables, ScopeId = testScopeId }
        };

        var expectedRequest = new ModifyProcessInstanceRequest
        {
            ProcessInstanceKey = testProcessInstanceKey,
            ActivateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.ActivateInstruction
                {
                    ElementId = testElementId,
                    AncestorElementInstanceKey = testAncestorElementInstanceKey,
                    VariableInstructions = { variableInstructions }
                }
            },
            TerminateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.TerminateInstruction()
                {
                     ElementInstanceKey = testElementInstanceKey
                }
            }
        };

        // when
        await ZeebeClient.NewModifyProcessInstanceCommand(processInstanceKey: testProcessInstanceKey)
            .AddInstructionToActivate(
                elementId: testElementId,
                ancestorElementInstanceKey: testAncestorElementInstanceKey,
                variableInstructions: variableInstructions)
            .AddInstructionToTerminate(elementInstanceKey: testElementInstanceKey)
            .Send();

        // then
        var request = TestService.Requests[typeof(ModifyProcessInstanceRequest)][0];
        var typedRequest = request as ModifyProcessInstanceRequest;
        Assert.NotNull(typedRequest);
        Assert.AreEqual(expectedRequest, typedRequest);
    }

    [Test]
    public async Task ShouldReceiveResponseAsExpected()
    {
        // when
        var modifyProcessInstanceResponse = await ZeebeClient.NewModifyProcessInstanceCommand(0)
            .Send();

        // then
        Assert.IsNotNull(modifyProcessInstanceResponse);
    }
}