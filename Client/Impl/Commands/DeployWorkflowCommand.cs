using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GatewayProtocol;
using Google.Protobuf;
using Zeebe.Client.Api.Commands;

namespace Zeebe.Client.Impl.Commands
{
    public class DeployWorkflowCommand : IDeployWorkflowCommandBuilderStep2
    {
        private readonly Gateway.GatewayClient gatewayClient;
        private readonly DeployWorkflowRequest request;

        public DeployWorkflowCommand(Gateway.GatewayClient client)
        {
            gatewayClient = client;
            request = new DeployWorkflowRequest();
        }

        public IDeployWorkflowCommandBuilderStep2 AddResourceBytes(byte[] resourceBytes, string resourceName)
        {
            AddWorkflow(ByteString.CopyFrom(resourceBytes), resourceName);

            return this;
        }

        public IDeployWorkflowCommandBuilderStep2 AddResourceFile(string filename)
        {
            throw new NotImplementedException();
        }

        public IDeployWorkflowCommandBuilderStep2 AddResourceStream(Stream resourceStream, string resourceName)
        {
            AddWorkflow(ByteString.FromStream(resourceStream), resourceName);
            return this;
        }

        public IDeployWorkflowCommandBuilderStep2 AddResourceString(string resourceString, Encoding encoding, string resourceName)
        {
            AddWorkflow(ByteString.CopyFrom(resourceString, encoding), resourceName);
            return this;
        }

        public IDeployWorkflowCommandBuilderStep2 AddResourceStringUtf8(string resourceString, string resourceName)
        {
            AddWorkflow(ByteString.CopyFromUtf8(resourceString), resourceName);
            return this;
        }

        public async Task<DeployWorkflowResponse> Send()
        {
            var response = gatewayClient.DeployWorkflowAsync(request);

            return await response.ResponseAsync;
        }

        private void AddWorkflow(ByteString resource, string resourceName)
        {
            var requestObject = new WorkflowRequestObject
            {
                Definition = resource,
                Name = resourceName,
                Type = WorkflowRequestObject.Types.ResourceType.Bpmn
            };

            request.Workflows.Add(requestObject);
        }

    }
}
