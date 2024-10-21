using GatewayProtocol;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses;

/// <inheritdoc />
public class ProcessInstanceResultResponse(CreateProcessInstanceWithResultResponse response)
    : IProcessInstanceResult
{
    /// <inheritdoc/>
    public long ProcessDefinitionKey { get; } = response.ProcessDefinitionKey;

    /// <inheritdoc/>
    public string BpmnProcessId { get; } = response.BpmnProcessId;

    /// <inheritdoc/>
    public int Version { get; } = response.Version;

    /// <inheritdoc/>
    public long ProcessInstanceKey { get; } = response.ProcessInstanceKey;

    /// <inheritdoc/>
    public string Variables { get; } = response.Variables;
}