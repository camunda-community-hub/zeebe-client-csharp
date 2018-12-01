using System;
using NUnit.Framework;
using GatewayProtocol;
using Zeebe.Client.Impl.Responses;
using Zeebe.Client.Api.Responses;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Zeebe.Client.Api.Subscription;


namespace Zeebe.Client
{
    [TestFixture]
    public class JobWorkerTest : BaseZeebeTest
    {
        [Test]
        public void ShouldSendRequestAsExpected()
        {
            // given
            var expectedRequest = new ActivateJobsRequest
            {
                Timeout = 123L, Amount = 1, Type = "foo", Worker = "jobWorker"
            };

            var expectedResponse = new ActivateJobsResponse
            {
                Jobs = 
                { 
                    new GatewayProtocol.ActivatedJob{Key = 1, JobHeaders = new GatewayProtocol.JobHeaders()},
                    new GatewayProtocol.ActivatedJob{Key = 2, JobHeaders = new GatewayProtocol.JobHeaders()},
                    new GatewayProtocol.ActivatedJob{Key = 3, JobHeaders = new GatewayProtocol.JobHeaders()}
                }
            };
                
            TestService.AddRequestHandler(typeof(ActivateJobsRequest), request => expectedResponse);

            // when
            var signal = new EventWaitHandle(false, EventResetMode.AutoReset);
            using (var jobWorker = ZeebeClient.NewWorker()
                .JobType("foo")
                .Handler((jobClient, job) => { signal.Set(); })
                .Limit(1)
                .Name("jobWorker")
                .Timeout(123L)
                .PollInterval(TimeSpan.FromMilliseconds(100))
                .Open())
            {
                
                // then
                Assert.True(jobWorker.IsOpen());
            
                signal.WaitOne();
            }
            
            var actualRequest = TestService.Requests[0];
            Assert.AreEqual(expectedRequest, actualRequest);
        }
    }
}