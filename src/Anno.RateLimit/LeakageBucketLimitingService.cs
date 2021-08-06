using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Anno.RateLimit
{
    /// <summary>
    /// 漏桶
    /// </summary>
    public class LeakageBucketLimitingService : ILimitingService
    {
        private LimitedQueue<object> limitedQueue = null;
        private CancellationTokenSource cancelToken;
        private Task task = null;
        private int maxTPS;
        private int limitSize;
        private TimeSpan timeSpan = new TimeSpan(0, 0, 1);//默认1秒,时间间隔内允许处理的 请求个数
        private object lckObj = new object();
        /// <summary>
        /// 漏桶
        /// </summary>
        /// <param name="maxTPS">单位时间令牌数</param>
        /// <param name="limitSize">最大可用票据数(桶容量)</param>
        /// <param name="timeSpan">单位时间</param>
        public LeakageBucketLimitingService(int maxTPS, int limitSize, TimeSpan timeSpan)
        {
            this.timeSpan = timeSpan;
            this.limitSize = limitSize;
            this.maxTPS = maxTPS;

            if (this.limitSize <= 0)
                this.limitSize = 100;
            if (this.maxTPS <= 0)
                this.maxTPS = 1;

            limitedQueue = new LimitedQueue<object>(limitSize);
            cancelToken = new CancellationTokenSource();
            task = Task.Factory.StartNew(new Action(TokenProcess), cancelToken.Token);
        }
        /// <summary>
        /// 漏桶 1秒之内 TPS
        /// </summary>
        /// <param name="maxTPS">最大QPS（每秒向桶里面添加的令牌）</param>
        /// <param name="limitSize">最大可用票据数(桶容量)</param>
        public LeakageBucketLimitingService(int maxTPS, int limitSize) : this(maxTPS, limitSize, TimeSpan.FromSeconds(1))
        {

        }
        private void TokenProcess()
        {
            int sleep = ((int)timeSpan.TotalMilliseconds) / maxTPS;
            if (sleep == 0)
            {
                sleep = 1;
                var perSecondNumber = maxTPS / ((int)timeSpan.TotalMilliseconds);
                while (cancelToken.Token.IsCancellationRequested == false)
                {
                    try
                    {
                        for (int i = 0; i < perSecondNumber; i++)
                        {
                            if (limitedQueue.Count > 0)
                            {
                                lock (lckObj)
                                {
                                    if (limitedQueue.Count > 0)
                                        limitedQueue.Dequeue();
                                }
                            }
                        }
                        Thread.Sleep(sleep);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                DateTime start = DateTime.Now;
                while (cancelToken.Token.IsCancellationRequested == false)
                {
                    try
                    {

                        if (limitedQueue.Count > 0)
                        {
                            lock (lckObj)
                            {
                                if (limitedQueue.Count > 0)
                                    limitedQueue.Dequeue();
                            }
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        if (DateTime.Now - start < TimeSpan.FromMilliseconds(sleep))
                        {
                            int newSleep = sleep - (int)(DateTime.Now - start).TotalMilliseconds;
                            if (newSleep > 1)
                                Thread.Sleep(newSleep - 1); //做一下时间上的补偿
                        }
                        start = DateTime.Now;
                    }
                }
            }
        }

        public void Dispose()
        {
            cancelToken.Cancel();
        }

        public bool Request()
        {
            return limitedQueue.Enqueue(new object());
        }
    }
}
