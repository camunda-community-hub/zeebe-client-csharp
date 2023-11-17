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
using Zeebe.Client.Api.Misc;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Responses;
using DeployResourceResponse = Zeebe.Client.Impl.Responses.DeployResourceResponse;

namespace Zeebe.Client.Impl.Commands
{
    public class DeployResourceCommand : IDeployResourceCommandBuilderStep2
    {
        private readonly Gateway.GatewayClient gatewayClient;
        private readonly DeployResourceRequest request;
        private readonly IAsyncRetryStrategy asyncRetryStrategy;

        public DeployResourceCommand(Gateway.GatewayClient client, IAsyncRetryStrategy asyncRetryStrategy)
        {
            gatewayClient = client;
            this.asyncRetryStrategy = asyncRetryStrategy;
            request = new DeployResourceRequest();
        }

        public IDeployResourceCommandBuilderStep2 AddResourceBytes(byte[] resourceBytes, string resourceName)
        {
            AddResource(ByteString.CopyFrom(resourceBytes), resourceName);

            return this;
        }

        public IDeployResourceCommandBuilderStep2 AddResourceFile(string filename)
        {
            var text = File.ReadAllText(filename);
            AddResourceStringUtf8(text, filename);
            return this;
        }

        public IDeployResourceCommandBuilderStep2 AddResourceStream(Stream resourceStream, string resourceName)
        {
            AddResource(ByteString.FromStream(resourceStream), resourceName);
            return this;
        }

        public IDeployResourceCommandBuilderStep2 AddResourceString(string resourceString, Encoding encoding, string resourceName)
        {
            AddResource(ByteString.CopyFrom(resourceString, encoding), resourceName);
            return this;
        }

        public IDeployResourceCommandBuilderStep2 AddResourceStringUtf8(string resourceString, string resourceName)
        {
            AddResource(ByteString.CopyFromUtf8(resourceString), resourceName);
            return this;
        }

        public IDeployResourceCommandBuilderStep2 AddTenantId(string tenantId)
        {
            request.TenantId = tenantId;
            return this;
        }

        public async Task<IDeployResourceResponse> Send(TimeSpan? timeout = null, CancellationToken token = default)
        {
            var asyncReply = gatewayClient.DeployResourceAsync(request, deadline: timeout?.FromUtcNow(), cancellationToken: token);
            var response = await asyncReply.ResponseAsync;
            return new DeployResourceResponse(response);
        }

        public async Task<IDeployResourceResponse> Send(CancellationToken cancellationToken)
        {
            return await Send(token: cancellationToken);
        }

        public async Task<IDeployResourceResponse> SendWithRetry(TimeSpan? timespan = null, CancellationToken token = default)
        {
            return await asyncRetryStrategy.DoWithRetry(() => Send(timespan, token));
        }

        private void AddResource(ByteString resource, string resourceName)
        {
            var requestObject = new Resource
            {
                Name = resourceName,
                Content = resource
            };

            request.Resources.Add(requestObject);
        }
    }
}
