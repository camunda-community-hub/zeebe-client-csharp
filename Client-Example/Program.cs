//
//  Copyright 2018  camunda services gmbh
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
using GatewayProtocol;
using Google.Protobuf.Collections;
using Grpc.Core;
using Zeebe.Client;
using Zeebe.Client.Impl;

namespace ClientExample
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var server = new Server();
            server.Ports.Add(new ServerPort("localhost", 26500, ServerCredentials.Insecure));

            var testService = new GatewayTestService();
            var serviceDefinition = Gateway.BindService(testService);
            server.Services.Add(serviceDefinition);
            server.Start();
            
            var client = new ZeebeClient("localhost:26500");

            // given
            
            var expectedRequest = new ActivateJobsRequest
            {
                Timeout = 123L, Amount = 1, Type = "foo", Worker = "jobWorker"
            };
            
            var expectedResponse = new ActivateJobsResponse
            {
                Jobs = 
                { 
                    new ActivatedJob{Key = 1, JobHeaders = new JobHeaders()},
                    new ActivatedJob{Key = 2, JobHeaders = new JobHeaders()},
                    new ActivatedJob{Key = 3, JobHeaders = new JobHeaders()}
                }
            };
            
            testService.AddRequestHandler(typeof(ActivateJobsRequest), request => expectedResponse);


            client.JobClient()
                  .Worker()
                  .JobType("foo")
                  .Handler((jobClient, job) =>
                  {
                        Console.WriteLine("Handle job: ");
                        Console.WriteLine(job.Key);
                  })
                  .Limit(5)
                  .Name("csharpWorker")
                  .PollInterval(TimeSpan.FromMilliseconds(100))
                  .Timeout(100)
                  .Open();

            while (true)
            {
            }
        }
    }
}
