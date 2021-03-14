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
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Google.Protobuf;
using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;

namespace Zeebe.Client.Impl.Commands
{
    public class DeployProcessCommand : IDeployProcessCommandBuilderStep2
    {
        private readonly Gateway.GatewayClient gatewayClient;
        private readonly DeployProcessRequest request;

        public DeployProcessCommand(Gateway.GatewayClient client)
        {
            gatewayClient = client;
            request = new DeployProcessRequest();
        }

        public IDeployProcessCommandBuilderStep2 AddResourceBytes(byte[] resourceBytes, string resourceName)
        {
            AddProcess(ByteString.CopyFrom(resourceBytes), resourceName);

            return this;
        }

        public IDeployProcessCommandBuilderStep2 AddResourceFile(string filename)
        {
            var text = File.ReadAllText(filename);
            AddResourceStringUtf8(text, filename);
            return this;
        }

        public IDeployProcessCommandBuilderStep2 AddResourceStream(Stream resourceStream, string resourceName)
        {
            AddProcess(ByteString.FromStream(resourceStream), resourceName);
            return this;
        }

        public IDeployProcessCommandBuilderStep2 AddResourceString(string resourceString, Encoding encoding, string resourceName)
        {
            AddProcess(ByteString.CopyFrom(resourceString, encoding), resourceName);
            return this;
        }

        public IDeployProcessCommandBuilderStep2 AddResourceStringUtf8(string resourceString, string resourceName)
        {
            AddProcess(ByteString.CopyFromUtf8(resourceString), resourceName);
            return this;
        }

        public async Task<IDeployResponse> Send(TimeSpan? timeout = null)
        {
            var asyncReply = gatewayClient.DeployProcessAsync(request, deadline: timeout?.FromUtcNow());
            var response = await asyncReply.ResponseAsync;
            return new DeployResponse(response);
        }

        public async Task<IDeployResponse> Send(CancellationToken token)
        {
            var asyncReply = gatewayClient.DeployProcessAsync(request, cancellationToken: token);
            var response = await asyncReply.ResponseAsync;
            return new DeployResponse(response);
        }

        private void AddProcess(ByteString resource, string resourceName)
        {
            var requestObject = new ProcessRequestObject
            {
                Definition = resource,
                Name = resourceName,
                Type = ProcessRequestObject.Types.ResourceType.File
            };

            request.Processes.Add(requestObject);
        }
    }
}
