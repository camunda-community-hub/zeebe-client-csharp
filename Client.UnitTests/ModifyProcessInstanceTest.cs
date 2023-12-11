using System.Threading.Tasks;
using GatewayProtocol;
using NUnit.Framework;

namespace Zeebe.Client;

[TestFixture]
public class ModifyProcessInstanceTest : BaseZeebeTest
{
    [Test]
    public async Task ShouldSendRequestAsExpected()
    {
        // given
        var expectedRequest = new ModifyProcessInstanceRequest
        {
            ActivateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.ActivateInstruction
                {
                    ElementId = "test"
                }
            },
            TerminateInstructions =
            {
                new ModifyProcessInstanceRequest.Types.TerminateInstruction()
            }
        };

        // when
        await ZeebeClient.NewModifyProcessInstanceCommand(processInstanceKey: 0)
            .AddInstructionToActivate(elementId: "test", ancestorElementInstanceKey: 0)
            .AddInstructionToTerminate(elementInstanceKey: 0)
            .Send();

        // then
        var request = TestService.Requests[typeof(ModifyProcessInstanceRequest)][0];
        Assert.AreEqual(expectedRequest, request);
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