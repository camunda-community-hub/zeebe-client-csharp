using System;
using Zeebe;
using System.Collections.Generic;

namespace example
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            ZeebeClient zeebeClient = new ZeebeClient("0.0.0.0:51015");

            JobClient jobClient = zeebeClient.DefaultTopic
                                             .JobClient("thisWorker", "myType");

            IList<JobClient.Job> jobs = jobClient.Poll(1);

            Console.WriteLine("Polled {0} jobs", jobs.Count);
            foreach (JobClient.Job job in jobs)
            {
                Console.WriteLine("Complete Job {0}", job.JobKey);
                jobClient.Complete(job.JobKey, "{\"csharpPayload\":true}");
            }

            jobs = jobClient.Poll(1);

            Console.WriteLine("Polled {0} jobs", jobs.Count);
            foreach (JobClient.Job job in jobs)
            {
                Console.WriteLine("Fail Job {0}", job.JobKey);
                jobClient.Fail(job.JobKey);
            }

            while (true) ;
        }
    }
}
