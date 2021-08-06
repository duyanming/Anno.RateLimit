using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anno.RateLimit
{
    /// <summary>
    /// 令牌桶
    /// </summary>
    public class TokenBucketLimitingService : ILimitingService
    {
        private LimitedQueue<object> limitedQueue = null;
        private CancellationTokenSource cancelToken;
        private Task task = null;
        private int maxTPS;
        private int limitSize;
        private TimeSpan timeSpan = new TimeSpan(0, 0, 1);//默认1秒,时间间隔内允许处理的 请求个数
        private object lckObj = new object();
        /// <summary>
        /// 令牌桶
        /// </summary>
        /// <param name="maxTPS">单位时间令牌数</param>
        /// <param name="limitSize">最大可用票据数(桶容量)</param>
        /// <param name="timeSpan">单位时间</param>
        public TokenBucketLimitingService(int maxTPS, int limitSize, TimeSpan timeSpan)
        {
            this.timeSpan = timeSpan;
            this.limitSize = limitSize;
            this.maxTPS = maxTPS;

            if (this.limitSize <= 0)
                this.limitSize = 100;
            if (this.maxTPS <= 0)
                this.maxTPS = 1;

            limitedQueue = new LimitedQueue<object>(limitSize);
            for (int i = 0; i < limitSize; i++)
            {
                limitedQueue.Enqueue(new object());
            }
            cancelToken = new CancellationTokenSource();
            task = Task.Factory.StartNew(new Action(TokenProcess), cancelToken.Token);
        }
        /// <summary>
        /// 令牌桶 1秒之内 TPS
        /// </summary>
        /// <param name="maxTPS">最大QPS（每秒向桶里面添加的令牌）</param>
        /// <param name="limitSize">最大可用票据数(桶容量)</param>
        public TokenBucketLimitingService(int maxTPS, int limitSize) : this(maxTPS, limitSize, TimeSpan.FromSeconds(1))
        {
        }
        /// <summary>
        /// 定时消息令牌
        /// </summary>
        private void TokenProcess()
        {
            int sleep = ((int)timeSpan.TotalMilliseconds) / maxTPS;
            if (sleep == 0)
            {
                sleep = 1;
                var perSecondNumber = maxTPS / ((int)timeSpan.TotalMilliseconds);
                while (cancelToken.Token.IsCancellationRequested == false)
                {
                    for (int i = 0; i < perSecondNumber; i++)
                    {
                        limitedQueue.Enqueue(new object());
                    }
                    Thread.Sleep(sleep);
                }
            }
            else
            {
                DateTime start = DateTime.Now;
                while (cancelToken.Token.IsCancellationRequested == false)
                {
                    limitedQueue.Enqueue(new object());
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

        public void Dispose()
        {
            cancelToken.Cancel();
        }

        /// <summary>
        /// 请求令牌
        /// </summary>
        /// <returns>true：获取成功，false：获取失败</returns>
        public bool Request()
        {
            if (limitedQueue.Count <= 0)
                return false;
            lock (lckObj)
            {
                if (limitedQueue.Count <= 0)
                    return false;

                object data = limitedQueue.Dequeue();
                if (data == null)
                    return false;
            }

            return true;
        }
    }
}
