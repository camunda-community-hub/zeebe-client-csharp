using GatewayProtocol;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class ProcessInstanceResponse : IProcessInstanceResponse
    {
        /// <inheritdoc/>
        public long ProcessDefinitionKey { get; }
        /// <inheritdoc/>
        public string BpmnProcessId { get; }
        /// <inheritdoc/>
        public int Version { get; }
        /// <inheritdoc/>
        public long ProcessInstanceKey { get; }

        public ProcessInstanceResponse(CreateProcessInstanceResponse response)
        {
            ProcessDefinitionKey = response.ProcessDefinitionKey;
            BpmnProcessId = response.BpmnProcessId;
            Version = response.Version;
            ProcessInstanceKey = response.ProcessInstanceKey;
        }
    }
}
