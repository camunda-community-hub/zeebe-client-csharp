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
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Commands;

namespace Zeebe.Client
{
    [TestFixture]
    public class CompleteJobTest : BaseZeebeTest
    {
        [Test]
        public async Task ShouldSendRequestAsExpected()
        {
            // given
            const string variables = "{\"foo\":23}";
            const int jobKey = 255;
            var expectedRequest = new CompleteJobRequest
            {
                JobKey = jobKey,
                Variables = variables
            };

            // when
            await ZeebeClient.NewCompleteJobCommand(jobKey).Variables(variables).Send();

            // then
            var actualRequest = TestService.Requests[0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }

        [Test]
        public void ShouldTimeoutRequest()
        {
            // given
            const string variables = "{\"foo\":23}";
            const int jobKey = 255;

            // when
            var task = ZeebeClient.NewCompleteJobCommand(jobKey).Variables(variables).Send(TimeSpan.Zero);
            var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
            var rpcException = (RpcException) aggregateException.InnerExceptions[0];

            // then
            Assert.AreEqual(Grpc.Core.StatusCode.DeadlineExceeded, rpcException.Status.StatusCode);
        }

        [Test]
        public async Task ShouldUseActivatedJobToComplete()
        {
            // given
            const string variables = "{\"foo\":23}";
            const int jobKey = 255;

            var grpcActivatedJob = new ActivatedJob();
            grpcActivatedJob.Key = jobKey;
            var activatedJob = new Impl.Responses.ActivatedJob(grpcActivatedJob);
            var expectedRequest = new CompleteJobRequest
            {
                JobKey = jobKey,
                Variables = variables
            };

            // when
            await ZeebeClient.NewCompleteJobCommand(activatedJob).Variables(variables).Send();

            // then
            var actualRequest = TestService.Requests[0];

            Assert.AreEqual(expectedRequest, actualRequest);
        }
    }
}
