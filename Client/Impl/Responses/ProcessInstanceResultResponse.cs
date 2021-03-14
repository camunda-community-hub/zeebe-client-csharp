using GatewayProtocol;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    /// <inheritdoc />
    public class ProcessInstanceResultResponse : IProcessInstanceResult
    {
        /// <inheritdoc/>
        public long ProcessDefinitionKey { get; }
        /// <inheritdoc/>
        public string BpmnProcessId { get; }
        /// <inheritdoc/>
        public int Version { get; }
        /// <inheritdoc/>
        public long ProcessInstanceKey { get; }
        /// <inheritdoc/>
        public string Variables { get; }

        public ProcessInstanceResultResponse(CreateProcessInstanceWithResultResponse response)
        {
            ProcessDefinitionKey = response.ProcessDefinitionKey;
            BpmnProcessId = response.BpmnProcessId;
            Version = response.Version;
            ProcessInstanceKey = response.ProcessInstanceKey;
            Variables = response.Variables;
        }
    }
}
