/*
* Copyright © 2017 camunda services GmbH (info@camunda.com)
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*     http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using System;
using System.IO;
using System.Text;
using GatewayProtocol;

namespace Zeebe.Client.Api.Commands
{
    public interface IDeployWorkflowCommandStep1
    {

        /**
         * Add the given resource to the deployment.
         *
         * @param resourceBytes the workflow resource as byte array
         * @param resourceName the name of the resource (e.g. "workflow.bpmn")
         * @return the builder for this command. Call {@link #send()} to complete the command and send it
         *     to the broker.
         */
        IDeployWorkflowCommandBuilderStep2 AddResourceBytes(byte[] resourceBytes, String resourceName);

        /**
         * Add the given resource to the deployment.
         *
         * @param resourceString the workflow resource as String
         * @param charset the charset of the String
         * @param resourceName the name of the resource (e.g. "workflow.bpmn")
         * @return the builder for this command. Call {@link #send()} to complete the command and send it
         *     to the broker.
         */
        IDeployWorkflowCommandBuilderStep2 AddResourceString(
            String resourceString, Encoding encoding, String resourceName);

        /**
         * Add the given resource to the deployment.
         *
         * @param resourceString the workflow resource as UTF-8-encoded String
         * @param resourceName the name of the resource (e.g. "workflow.bpmn")
         * @return the builder for this command. Call {@link #send()} to complete the command and send it
         *     to the broker.
         */
        IDeployWorkflowCommandBuilderStep2 AddResourceStringUtf8(
            String resourceString, String resourceName);

        /**
         * Add the given resource to the deployment.
         *
         * @param resourceStream the workflow resource as stream
         * @param resourceName the name of the resource (e.g. "workflow.bpmn")
         * @return the builder for this command. Call {@link #send()} to complete the command and send it
         *     to the broker.
         */
        IDeployWorkflowCommandBuilderStep2 AddResourceStream(
            Stream resourceStream, String resourceName);

        /**
         * Add the given resource to the deployment.
         *
         * @param filename the absolute path of the workflow resource (e.g. "~/wf/workflow.bpmn")
         * @return the builder for this command. Call {@link #send()} to complete the command and send it
         *     to the broker.
         */
        IDeployWorkflowCommandBuilderStep2 AddResourceFile(String filename);
    }

    public interface IDeployWorkflowCommandBuilderStep2 : IDeployWorkflowCommandStep1, IFinalCommandStep<DeployWorkflowResponse>
    {
        // the place for new optional parameters
    }
}
