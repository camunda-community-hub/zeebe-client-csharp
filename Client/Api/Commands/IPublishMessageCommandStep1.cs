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

namespace Zeebe.Client.Api.Commands
{
    public interface IPublishMessageCommandStep1
    {
        /**
         * Set the name of the message.
         *
         * @param messageName the name of the message
         * @return the builder for this command
         */
        IPublishMessageCommandStep2 MessageName(string messageName);
    }

    public interface IPublishMessageCommandStep2
    {
        /**
         * Set the correlation-key of the message.
         *
         * @param correlationKey the correlation-key of the message
         * @return the builder for this command
         */
        IPublishMessageCommandStep3 CorrelationKey(string correlationKey);
    }

    public interface IPublishMessageCommandStep3 : IFinalCommandStep<IPublishMessageResponse>
    {
        /**
         * Set the id of the message. The message is rejected if another message is already published
         * with the same id, name and correlation-key.
         *
         * @param messageId the id of the message
         * @return the builder for this command. Call {@link #send()} to complete the command and send
         *     it to the broker.
         */
        IPublishMessageCommandStep3 MessageId(string messageId);

        /**
         * Set the time-to-live of the message. The message can only be correlated within the given
         * time-to-live.
         *
         * <p>If the duration is zero or negative then the message can only be correlated to open
         * subscriptions (e.g. to an entered message catch event).
         *
         * <p>If no duration is set then the default is used from the configuration.
         *
         * @param timeToLive the time-to-live of the message
         * @return the builder for this command. Call {@link #send()} to complete the command and send
         *     it to the broker.
         */
        IPublishMessageCommandStep3 TimeToLive(TimeSpan timeToLive);

        /**
         * Set the payload of the message.
         *
         * @param payload the payload (JSON) as String
         * @return the builder for this command. Call {@link #send()} to complete the command and send
         *     it to the broker.
         */
        IPublishMessageCommandStep3 Payload(string payload);
    }
}
