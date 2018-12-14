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
    public interface ICompleteJobCommandStep1 : IFinalCommandStep<ICompleteJobResponse>
    {
        /**
        * Set the payload to complete the job with.
        *
        * @param payload the payload (JSON) as String
        * @return the builder for this command. Call {@link #send()} to complete the command and send it
        *     to the broker.
        */
        ICompleteJobCommandStep1 Payload(string payload);
    }
}
