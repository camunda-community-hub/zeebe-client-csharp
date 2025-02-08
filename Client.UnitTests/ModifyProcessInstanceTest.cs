using System.Collections.Generic;
using System.Threading.Tasks;
using GatewayProtocol;
using NUnit.Framework;

namespace Zeebe.Client;

[TestFixture]
public class ModifyProcessInstanceTest : BaseZeebeTest
{
    private const long TestProcessInstanceKey = 1234567890;
    private const long TestAncestorElementInstanceKey = 2345678900;
    private const long TestElementInstanceKey = 3456789000;
    private const string TestElementId = "element1";
    private const string TestVariables = "variable1";
    private const string TestScopeId = "scope1";

    [Test]
    public async Task ShouldSendRequestAsExpectedWithActivateElement()
    {
        // given
        var expectedRequest = new ModifyProcessInstanceRequest
        {
            ProcessInstanceKey = TestProcessInstanceKey,
            ActivateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.ActivateInstruction
                {
                    ElementId = TestElementId,
                    AncestorElementInstanceKey = 0
                }
            }
        };

        // when
        _ = await ZeebeClient.NewModifyProcessInstanceCommand(TestProcessInstanceKey)
        .ActivateElement(TestElementId)
        .Send();

        // then
        var request = TestService.Requests[typeof(ModifyProcessInstanceRequest)][0];
        var typedRequest = request as ModifyProcessInstanceRequest;
        Assert.NotNull(typedRequest);
        Assert.AreEqual(expectedRequest, typedRequest);
    }

    [Test]
    public async Task ShouldSendRequestAsExpectedWithActivateElementWithAncestor()
    {
        // given
        var expectedRequest = new ModifyProcessInstanceRequest
        {
            ProcessInstanceKey = TestProcessInstanceKey,
            ActivateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.ActivateInstruction
                {
                    ElementId = TestElementId,
                    AncestorElementInstanceKey = TestAncestorElementInstanceKey
                }
            }
        };

        // when
        _ = await ZeebeClient.NewModifyProcessInstanceCommand(TestProcessInstanceKey)
        .ActivateElement(TestElementId, TestAncestorElementInstanceKey)
        .Send();

        // then
        var request = TestService.Requests[typeof(ModifyProcessInstanceRequest)][0];
        var typedRequest = request as ModifyProcessInstanceRequest;
        Assert.NotNull(typedRequest);
        Assert.AreEqual(expectedRequest, typedRequest);
    }

    [Test]
    public async Task ShouldSendRequestAsExpectedWithTerminateElement()
    {
        // given
        var expectedRequest = new ModifyProcessInstanceRequest
        {
            ProcessInstanceKey = TestProcessInstanceKey,
            TerminateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.TerminateInstruction
                {
                    ElementInstanceKey = TestElementInstanceKey
                }
            }
        };

        // when
        _ = await ZeebeClient.NewModifyProcessInstanceCommand(TestProcessInstanceKey)
        .TerminateElement(TestElementInstanceKey)
        .Send();

        // then
        var request = TestService.Requests[typeof(ModifyProcessInstanceRequest)][0];
        var typedRequest = request as ModifyProcessInstanceRequest;
        Assert.NotNull(typedRequest);
        Assert.AreEqual(expectedRequest, typedRequest);
    }

    [Test]
    public async Task ShouldSendRequestWithActivateAndTerminate()
    {
        // given
        var expectedRequest = new ModifyProcessInstanceRequest
        {
            ProcessInstanceKey = TestProcessInstanceKey,
            ActivateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.ActivateInstruction
                {
                    ElementId = TestElementId,
                    AncestorElementInstanceKey = TestAncestorElementInstanceKey
                }
            },
            TerminateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.TerminateInstruction
                {
                    ElementInstanceKey = TestElementInstanceKey
                }
            }
        };

        // when
        _ = await ZeebeClient.NewModifyProcessInstanceCommand(TestProcessInstanceKey)
        .ActivateElement(TestElementId, TestAncestorElementInstanceKey)
        .And()
        .TerminateElement(TestElementInstanceKey)
        .Send();

        // then
        var request = TestService.Requests[typeof(ModifyProcessInstanceRequest)][0];
        var typedRequest = request as ModifyProcessInstanceRequest;
        Assert.NotNull(typedRequest);
        Assert.AreEqual(expectedRequest, typedRequest);
    }

    [Test]
    public async Task ShouldSendRequestWithActivateAndTerminateMultiple()
    {
        // given
        var expectedRequest = new ModifyProcessInstanceRequest
        {
            ProcessInstanceKey = TestProcessInstanceKey,
            ActivateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.ActivateInstruction
                {
                    ElementId = TestElementId,
                    AncestorElementInstanceKey = TestAncestorElementInstanceKey
                },
                new ModifyProcessInstanceRequest.Types.ActivateInstruction
                {
                    ElementId = TestElementId + "2",
                    AncestorElementInstanceKey = TestAncestorElementInstanceKey + 1
                }
            },
            TerminateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.TerminateInstruction
                {
                    ElementInstanceKey = TestElementInstanceKey
                },
                new ModifyProcessInstanceRequest.Types.TerminateInstruction
                {
                    ElementInstanceKey = TestElementInstanceKey + 1
                }
            }
        };

        // when
        _ = await ZeebeClient.NewModifyProcessInstanceCommand(TestProcessInstanceKey)
        .ActivateElement(TestElementId, TestAncestorElementInstanceKey)
        .And()
        .TerminateElement(TestElementInstanceKey)
        .And()
        .ActivateElement(TestElementId + "2",
            TestAncestorElementInstanceKey + 1)
        .And()
        .TerminateElement(TestElementInstanceKey + 1)
        .Send();

        // then
        var request = TestService.Requests[typeof(ModifyProcessInstanceRequest)][0];
        var typedRequest = request as ModifyProcessInstanceRequest;
        Assert.NotNull(typedRequest);
        Assert.AreEqual(expectedRequest, typedRequest);
    }

    [Test]
    public async Task ShouldSendRequestWithActivateElementIdAndVariableInstructions()
    {
        // given
        var expectedRequest = new ModifyProcessInstanceRequest
        {
            ProcessInstanceKey = TestProcessInstanceKey,
            ActivateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.ActivateInstruction
                {
                    ElementId = TestElementId,
                    AncestorElementInstanceKey = 0,
                    VariableInstructions =
                    {
                        new List<ModifyProcessInstanceRequest.Types.VariableInstruction>
                        {
                            new () { Variables = TestVariables, ScopeId = TestScopeId }
                        }
                    }
                }
            }
        };

        // when
        _ = await ZeebeClient.NewModifyProcessInstanceCommand(TestProcessInstanceKey)
        .ActivateElement(TestElementId)
        .WithVariables(TestVariables, TestScopeId)
        .Send();

        // then
        var request = TestService.Requests[typeof(ModifyProcessInstanceRequest)][0];
        var typedRequest = request as ModifyProcessInstanceRequest;
        Assert.NotNull(typedRequest);
        Assert.AreEqual(expectedRequest, typedRequest);
    }

    [Test]
    public async Task ShouldSendRequestWithActivateElementIdVariableAndTerminateInstructions()
    {
        // given
        var expectedRequest = new ModifyProcessInstanceRequest
        {
            ProcessInstanceKey = TestProcessInstanceKey,
            ActivateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.ActivateInstruction
                {
                    ElementId = TestElementId,
                    AncestorElementInstanceKey = TestAncestorElementInstanceKey,
                    VariableInstructions =
                    {
                        new List<ModifyProcessInstanceRequest.Types.VariableInstruction>
                        {
                            new () { Variables = TestVariables, ScopeId = TestScopeId }
                        }
                    }
                }
            },
            TerminateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.TerminateInstruction
                {
                    ElementInstanceKey = TestElementInstanceKey
                }
            }
        };

        // when
        _ = await ZeebeClient.NewModifyProcessInstanceCommand(TestProcessInstanceKey)
        .ActivateElement(TestElementId, TestAncestorElementInstanceKey)
        .WithVariables(TestVariables, TestScopeId)
        .And()
        .TerminateElement(TestElementInstanceKey)
        .Send();

        // then
        var request = TestService.Requests[typeof(ModifyProcessInstanceRequest)][0];
        var typedRequest = request as ModifyProcessInstanceRequest;
        Assert.NotNull(typedRequest);
        Assert.AreEqual(expectedRequest, typedRequest);
    }
}