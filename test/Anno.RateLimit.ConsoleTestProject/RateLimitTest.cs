using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Anno.RateLimit;

namespace Anno.RateLimit.ConsoleTestProject
{
    /// <summary>
    /// 限流测试
    /// </summary>
    public class RateLimitTest
    {
        /// <summary>
        /// 限流测试
        /// </summary>
        public void Handle()
        {
            int x = 0;
            /*
             * 令牌桶 限流1秒 500  桶容量 200
             * */
            var service = LimitingFactory.Build(TimeSpan.FromSeconds(1), LimitingType.TokenBucket, 500, 200);
            List<Task> tasks = new List<Task>();
            Console.Write("请输入并发请求线程数：");
            long.TryParse(Console.ReadLine(), out long th);
            for (int i = 0; i < th; i++)
            {
                var t = Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        var result = service.Request();
                        //如果返回true，说明可以进行业务处理，否则需要继续等待
                        if (result)
                        {
                            //Console.WriteLine($"{DateTime.Now}--{Task.CurrentId}---ok");
                            //业务处理......
                            Interlocked.Increment(ref x);
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                    }
                }, TaskCreationOptions.LongRunning);
                tasks.Add(t);
            }
            while (true)
            {
                Console.WriteLine(x);
                Thread.Sleep(1000);
            }
            Task.WaitAll(tasks.ToArray());
        }
    }
}
