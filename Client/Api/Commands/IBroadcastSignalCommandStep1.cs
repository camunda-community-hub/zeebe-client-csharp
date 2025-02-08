﻿//
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
using Zeebe.Client.Api.Responses;
namespace Zeebe.Client.Api.Commands
{
    public interface IBroadcastSignalCommandStep1
    {
        /// <summary>
        /// Set the name of the signal.
        /// </summary>
        /// <param name="signalName"> the name of the signal.</param>
        /// <returns>the builder for this command.</returns>
        IBroadcastSignalCommandStep2 SignalName(string signalName);
    }

    public interface IBroadcastSignalCommandStep2 : IFinalCommandWithRetryStep<IBroadcastSignalResponse>
    {
        /// <summary>
        /// Set the variables of the signal.
        /// </summary>
        ///
        /// <param name="variables">the variables (JSON) as String.</param>
        /// <returns>the builder for this command. Call <see cref="IFinalCommandStep{T}.Send"/> to complete the command and send
        ///     it to the broker.</returns>
        IBroadcastSignalCommandStep2 Variables(string variables);
    }
}