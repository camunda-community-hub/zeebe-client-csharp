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
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands;

public interface IPublishMessageCommandStep1
{
    /// <summary>
    /// Set the name of the message.
    /// </summary>
    /// <param name="messageName"> the name of the message.</param>
    /// <returns>the builder for this command.</returns>
    IPublishMessageCommandStep2 MessageName(string messageName);
}

public interface IPublishMessageCommandStep2
{
    /// <summary>
    /// Set the value of the correlation key of the message.
    ///
    /// This value will be used together with the message name
    /// to find matching message subscriptions.
    /// </summary>
    /// <param name="correlationKey">the correlation key value of the message.</param>
    /// <returns>the builder for this command.</returns>
    IPublishMessageCommandStep3 CorrelationKey(string correlationKey);
}

public interface IPublishMessageCommandStep3 : IFinalCommandWithRetryStep<IPublishMessageResponse>
{
    /// <summary>
    /// Set the id of the message. The message is rejected if another message is already published
    /// with the same id, name and correlation-key.
    /// </summary>
    /// <param name="messageId">the id of the message.</param>
    /// <returns>the builder for this command.
    /// Call <see cref="IFinalCommandWithRetryStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to
    /// complete the command and send it to the broker.</returns>
    IPublishMessageCommandStep3 MessageId(string messageId);

    /// <summary>
    /// Set the time-to-live of the message. The message can only be correlated within the given
    /// time-to-live.
    ///
    /// <para>If the duration is zero or negative then the message can only be correlated to open
    /// subscriptions (e.g. to an entered message catch event).
    /// </para>
    /// </summary>
    ///
    /// <param name="timeToLive">the time-to-live of the message.</param>
    /// <returns>
    /// the builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.
    /// </returns>
    IPublishMessageCommandStep3 TimeToLive(TimeSpan timeToLive);

    /// <summary>
    /// Set the variables of the message.
    /// </summary>
    ///
    /// <param name="variables">the variables (JSON) as String.</param>
    /// <returns>the builder for this command.
    /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
    /// the command and send it to the broker.</returns>
    IPublishMessageCommandStep3 Variables(string variables);
}