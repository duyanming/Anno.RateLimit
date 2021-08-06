using System;

namespace Anno.RateLimit.ConsoleTestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("测试结束---------------------Start");
            new RateLimitTest().Handle();
            Console.WriteLine("测试结束---------------------End");
            Console.ReadLine();
        }
    }
}
