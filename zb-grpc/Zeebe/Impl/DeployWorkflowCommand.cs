using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GatewayProtocol;
using Google.Protobuf;

namespace Zeebe.Impl
{
    public class DeployWorkflowCommand : IDeployWorkflowCommandStep1, IDeployWorkflowCommandBuilderStep2
    {
        private Gateway.GatewayClient gatewayClient;
        private DeployWorkflowRequest request;

        public DeployWorkflowCommand(Gateway.GatewayClient client)
        {
            gatewayClient = client;
            request = new DeployWorkflowRequest();
        }

        public IDeployWorkflowCommandBuilderStep2 AddResourceBytes(byte[] resourceBytes, string resourceName)
        {
            WorkflowRequestObject requestObject = new WorkflowRequestObject();
            requestObject.Definition = ByteString.CopyFrom(resourceBytes);
            requestObject.Name = resourceName;
            requestObject.Type = WorkflowRequestObject.Types.ResourceType.Bpmn;

            request.Workflows.Add(requestObject);

            return this;
        }

        public IDeployWorkflowCommandBuilderStep2 AddResourceFile(string filename)
        {
            throw new NotImplementedException();
        }

        public IDeployWorkflowCommandBuilderStep2 AddResourceStream(Stream resourceStream, string resourceName)
        {
            WorkflowRequestObject requestObject = new WorkflowRequestObject();
            requestObject.Definition = ByteString.FromStream(resourceStream);
            requestObject.Name = resourceName;
            requestObject.Type = WorkflowRequestObject.Types.ResourceType.Bpmn;

            request.Workflows.Add(requestObject);

            return this;
            
        }

        public IDeployWorkflowCommandBuilderStep2 AddResourceString(string resourceString, Encoding encoding, string resourceName)
        {
            WorkflowRequestObject requestObject = new WorkflowRequestObject();
            requestObject.Definition = ByteString.CopyFrom(resourceString, encoding);
            requestObject.Name = resourceName;
            requestObject.Type = WorkflowRequestObject.Types.ResourceType.Bpmn;

            request.Workflows.Add(requestObject);
            
            return this;
        }

        public IDeployWorkflowCommandBuilderStep2 AddResourceStringUtf8(string resourceString, string resourceName)
        {
            WorkflowRequestObject requestObject = new WorkflowRequestObject();
            requestObject.Definition = ByteString.CopyFromUtf8(resourceString);
            requestObject.Name = resourceName;
            requestObject.Type = WorkflowRequestObject.Types.ResourceType.Bpmn;

            request.Workflows.Add(requestObject);
            return this;
        }

        public async Task<DeployWorkflowResponse> Send()
        {
            var response = gatewayClient.DeployWorkflowAsync(request);

            return await response.ResponseAsync;
        }
    }
}
