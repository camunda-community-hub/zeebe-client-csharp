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
using System.IO;
using System.Text;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands;

public interface IDeployResourceCommandStep1
{
    /// <summary>
    /// Add the given resource to the deployment.
    /// </summary>
    /// <param name="resourceBytes">the resource as byte array.</param>
    /// <param name="resourceName">the name of the resource (e.g. "process.bpmn").</param>
    /// <returns>the builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.</returns>
    IDeployResourceCommandBuilderStep2 AddResourceBytes(byte[] resourceBytes, string resourceName);

    /// <summary>
    /// Add the given resource to the deployment.
    /// </summary>
    /// <param name="resourceString">the resource as String.</param>
    /// <param name="encoding">the charset of the String.</param>
    /// <param name="resourceName">the name of the resource (e.g. "process.bpmn").</param>
    /// <returns>the builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.</returns>
    IDeployResourceCommandBuilderStep2 AddResourceString(
        string resourceString, Encoding encoding, string resourceName);

    /// <summary>
    /// Add the given resource to the deployment.
    /// </summary>
    /// <param name="resourceString">the resource as UTF-8-encoded String.</param>
    /// <param name="resourceName">the name of the resource (e.g. "process.bpmn").</param>
    /// <returns>the builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.</returns>
    IDeployResourceCommandBuilderStep2 AddResourceStringUtf8(
        string resourceString, string resourceName);

    /// <summary>
    /// Add the given resource to the deployment.
    /// </summary>
    /// <param name="resourceStream">the resource as stream.</param>
    /// <param name="resourceName">the name of the resource (e.g. "process.bpmn").</param>
    /// <returns>the builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.
    /// </returns>
    IDeployResourceCommandBuilderStep2 AddResourceStream(
        Stream resourceStream, string resourceName);

    /// <summary>
    /// Add the given resource to the deployment.
    /// </summary>
    /// <param name="filename">the absolute path of the resource (e.g. "~/wf/process.bpmn").</param>
    /// <returns>the builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.</returns>
    IDeployResourceCommandBuilderStep2 AddResourceFile(string filename);
}

public interface IDeployResourceCommandBuilderStep2 : IDeployResourceCommandStep1,
    ITenantIdCommandStep<IDeployResourceCommandBuilderStep2>, IFinalCommandWithRetryStep<IDeployResourceResponse>
{
    // the place for new optional parameters
}