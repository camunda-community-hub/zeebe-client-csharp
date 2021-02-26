//
//    Copyright (c) 2018 camunda services GmbH (info@camunda.com)
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GatewayProtocol;
using Google.Protobuf;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;

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
            var text = File.ReadAllText(filename);
            AddResourceStringUtf8(text, filename);
            return this;
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

        public async Task<IDeployResponse> Send(TimeSpan? timeout = null)
        {
            var asyncReply = gatewayClient.DeployWorkflowAsync(request, deadline: timeout?.FromUtcNow());
            var response = await asyncReply.ResponseAsync;
            return new DeployResponse(response);
        }

        private void AddWorkflow(ByteString resource, string resourceName)
        {
            var requestObject = new WorkflowRequestObject
            {
                Definition = resource,
                Name = resourceName,
                Type = WorkflowRequestObject.Types.ResourceType.File
            };

            request.Workflows.Add(requestObject);
        }
    }
}
