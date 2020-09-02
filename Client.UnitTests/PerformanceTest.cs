using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Zeebe.Client
{

    [TestFixture]
    public class PerformanceTest : BaseZeebeTest
    {
        private readonly string _demoProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "demo-process.bpmn");
        [Test]
        public void PerformanceTest21()
        {
            for (var i = 0; i < 10; i++)
            {
                var worker = ZeebeClient.NewWorker()
                    .JobType($"serviceTask{i}")
                    .Handler((client, job) => { })
                    .HandlerThreads(5)
                    .MaxJobsActive(10)
                    .PollInterval(TimeSpan.FromMilliseconds(100))
                    .Timeout(TimeSpan.FromSeconds(30))
                    .Open();

                Console.WriteLine($@"{DateTime.Now}: Created worker serviceTask{i}");
            }

            var workflows = ZeebeClient.NewDeployCommand()
                .AddResourceFile(_demoProcessPath)
                .Send()
                .GetAwaiter()
                .GetResult();

            Console.WriteLine($@"{DateTime.Now}: Loaded workflows {JsonConvert.SerializeObject(workflows)}");
        }


    }
}