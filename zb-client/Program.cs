using System;

namespace zbclient
{
    class MainClass
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //  GoString str;
            //  str.p = "0.0.0.0:51015";
            //  str.n = str.p.Length;
            ZeebeClient zeebeClient = new ZeebeClient("0.0.0.0:51015");

            zeebeClient.DefaultTopic
                       .JobClient("thisWorker", "myType")
                       .Poll(1);

            //String status = InitClient(str);
            //Console.WriteLine(status);
            //GoString str2;
            //str2.p = "hello";
            //str2.n = 5;
            //String xyz = CreateTopic(str2, 1, 1);
            //Console.WriteLine(xyz);
        }



        //[DllImport("libzbc-linux-amd64")]
        //private static extern String CreateTopic(GoString topicName, Int32 partitions, Int32 replicationFactor);


    }
}
