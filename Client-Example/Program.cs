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
using Zeebe.Client;

namespace ClientExample
{
    internal class MainClass
    {
        public static void Main(string[] args)
        {
            var client = ZeebeClient.NewZeebeClient("localhost:26500");

            client.NewWorker()
                  .JobType("foo")
                  .Handler((jobClient, job) =>
                  {
                      var jobKey = job.Key;
                      Console.WriteLine("Handle job: " + jobKey);

                      if (jobKey % 2 == 0)
                      {
                          jobClient.NewCompleteJobCommand(jobKey).Payload("{\"foo\":2}").Send();
                      }
                      else
                      {
                          jobClient.NewFailCommand(jobKey).Retries(job.Retries - 1).ErrorMessage("Example fail").Send();
                      }
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
