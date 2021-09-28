using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KeyDbMultithreadTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello KeyDb test!");

            await DoRedisTest();
            await Task.Delay(-1);
        }


        private static List<ChildTest> GenerateData()
        {
            Stopwatch sw = Stopwatch.StartNew();
            List<ChildTest> data = new();
            for (int i = 0; i < 1000000; i++)
            {
                data.Add(new ChildTest()
                {
                    Id = i,
                    Name = "First Last " + i,
                    Address = "My Address " + i,
                    DataStrings = new List<string>
                        {
                            "Datastring 1",
                            "Datastring 2",
                            "DataString 3"
                        },
                    Timestamp = DateTime.Now.AddSeconds(i)
                });
            }
            sw.Stop();
            return data;
        }
        private static async Task DoRedisTest()
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            IDatabase db = redis.GetDatabase();
            string hashName = "testhash";

            var data = GenerateData();
            var hashEntries = data.Select(d => new HashEntry(d.Id.ToString(), JsonConvert.SerializeObject(d))).ToArray();
            

            for (int i = 0; i < 1000; i++)
            {
                await db.HashSetAsync(hashName, hashEntries.Skip(i * 1000).Take(1000).ToArray());
                Debug.WriteLine($"Insert {i}");
            }

            bool c = true;
            ThreadPool.QueueUserWorkItem(async (state) =>
            {
                Thread.Sleep(1000);
                ConnectionMultiplexer redis2 = ConnectionMultiplexer.Connect("localhost");
                IDatabase db2 = redis.GetDatabase();

                Debug.WriteLine($"{DateTime.Now:HH:mm:ss.fff}\tStarting long query");
                Stopwatch sw = Stopwatch.StartNew();
                _ = await db2.HashGetAllAsync(hashName);
                sw.Stop();
                Debug.WriteLine($"{DateTime.Now:HH:mm:ss.fff}\tLong query done in " + sw.ElapsedMilliseconds);
                c = false;
            });

            ThreadPool.QueueUserWorkItem(async (state) =>
            {
                ConnectionMultiplexer redis3 = ConnectionMultiplexer.Connect("localhost");
                IDatabase db3 = redis.GetDatabase();

                Stopwatch sw = new Stopwatch();

                for (int i = 0; i < 1000; i++)
                {
                    sw.Restart();
                    await db3.HashGetAsync(hashName, i.ToString());
                    sw.Stop();
                    Debug.WriteLine($"{DateTime.Now:HH:mm:ss.fff}\tHashGetAsync took " + sw.ElapsedMilliseconds);
                    Thread.Sleep(20);
                }
            });

        }
    }
}
