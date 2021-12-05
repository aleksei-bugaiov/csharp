using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace CSharp.Redis
{
    internal class Program
    {
        private const int workerCount = 8;
        private const int completionCount = 8;

        private const string connectionString = "127.0.0.1:6379";
        private const int connectionMultiplexersPoolSize = 2;
        private const int tasksCount = (int)5e5;
        private const string redisKey = "key";

        private class RedisData
        {
            public string A = Guid.NewGuid().ToString();
            public string AA = Guid.NewGuid().ToString();
            public string AAA = Guid.NewGuid().ToString();
            public string AAAA = Guid.NewGuid().ToString();
            public string AAAAA = Guid.NewGuid().ToString();
            public string AAAAAA = Guid.NewGuid().ToString();
            public string AAAAAAA = Guid.NewGuid().ToString();
            public string AAAAAAAA = Guid.NewGuid().ToString();
            public string AAAAAAAAA = Guid.NewGuid().ToString();
            public string AAAAAAAAAA = Guid.NewGuid().ToString();
            public string AAAAAAAAAAA = Guid.NewGuid().ToString();
            public string AAAAAAAAAAAA = Guid.NewGuid().ToString();
        }

        private static void Main()
        {
            // Set up redis pool and thread pool
            ThreadPool.SetMinThreads(workerCount, completionCount);

            var configurationOptions = ConfigurationOptions.Parse(connectionString);
            var connectionMultiplexersPool = new ConnectionMultiplexer[connectionMultiplexersPoolSize];
            for (int i = 0; i < connectionMultiplexersPool.Length; i++)
            {
                var connectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);
                connectionMultiplexersPool[i] = connectionMultiplexer;
            }
            connectionMultiplexersPool[0].GetDatabase().StringSet(redisKey, JsonConvert.SerializeObject(new RedisData()));

            // Test init
            var tasks = new Task[tasksCount];
            var sw = new Stopwatch();
            var finished = 0;
            var sem = new Semaphore(1, 1);
            sw.Start();

            for (int i = 0; i < tasksCount; i++)
            {
                var localI = i;
                tasks[i] = Task.Run(async () =>
                {
                    try
                    {
                        JsonConvert.DeserializeObject<RedisData>(await connectionMultiplexersPool[localI % connectionMultiplexersPoolSize].GetDatabase().StringGetAsync(redisKey));
                        Interlocked.Increment(ref finished);
                    }
                    catch (Exception e)
                    {
                        sem.WaitOne();
                        Console.WriteLine(e.Message);
                        Console.WriteLine($"Failed. Total time: {sw.ElapsedMilliseconds.ToString()}Ms, Finished: {finished}, Rate(Req/Ms): {(double)finished / sw.ElapsedMilliseconds}");
                        Environment.Exit(1);
                    }
                });
            }

            Task.WaitAll(tasks);
            Console.WriteLine($"Success. Total time: {sw.ElapsedMilliseconds.ToString()}ms, Finished: {finished}, Rate(req/ms): {(double)finished / sw.ElapsedMilliseconds}");
        }
    }
}
