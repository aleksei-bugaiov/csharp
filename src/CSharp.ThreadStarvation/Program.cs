using System;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.ThreadStarvation
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine(Environment.ProcessorCount);

            ThreadPool.SetMinThreads(8, 8);

            Task.Factory.StartNew(Producer, TaskCreationOptions.None);

            Console.ReadLine();
        }

        private static void Producer()
        {
            while (true)
            {
                Process();

                Thread.Sleep(50);
            }
        }

        private static async Task Process()
        {
            await Task.Yield();

            var tcs = new TaskCompletionSource<bool>();

            Task.Run(() =>
            {
                Thread.Sleep(1000);
                tcs.SetResult(true);
            });

            tcs.Task.Wait();

            Console.WriteLine($"Ended: {DateTime.Now.ToLongTimeString()}");
        }
    }
}

