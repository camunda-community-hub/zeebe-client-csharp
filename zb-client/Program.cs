using System;
using System.Threading;
using System.Threading.Tasks;

namespace zbclient
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            MainClass m = new MainClass();
            Task t = m.workingMethod();

            Console.WriteLine("After Working method in Main");

            t.ContinueWith((task, obj) => Console.WriteLine("Test"), TaskStatus.RanToCompletion);
            Thread.Sleep(1000);
        }

        public async Task workingMethod()
        {
            Console.WriteLine("Working method.");

            String result = await asyncMethod();

            Console.WriteLine("Printing" +
                              " result: " + result);
        }

        public async Task<String> asyncMethod()
        {
            await Task.Delay(100);
            return "1234";
        }
    }
}
