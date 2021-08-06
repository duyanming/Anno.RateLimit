using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.RateLimit
{
    public class LimitingFactory
    {
        /// <summary>
        /// 创建限流服务对象
        /// </summary>
        /// <param name="limitingType">限流模型</param>
        /// <param name="maxQPS">最大QPS（每秒向桶里面添加的令牌）</param>
        /// <param name="limitSize">最大可用票据数(桶容量)</param>
        public static ILimitingService Build(LimitingType limitingType = LimitingType.TokenBucket, int maxQPS = 100, int limitSize = 100)
        {
            switch (limitingType)
            {
                case LimitingType.TokenBucket:
                default:
                    return new TokenBucketLimitingService(maxQPS, limitSize);
                case LimitingType.LeakageBucket:
                    return new LeakageBucketLimitingService(maxQPS, limitSize);
            }
        }
        /// <summary>
        /// 创建限流服务对象
        /// </summary>
        /// <param name="limitingType">限流模型</param>
        /// <param name="maxQPS">单位时间令牌数</param>
        /// <param name="limitSize">最大可用票据数(桶容量)</param>
        /// <param name="timeSpan">单位时间</param>
        public static ILimitingService Build(TimeSpan timeSpan, LimitingType limitingType = LimitingType.TokenBucket, int maxQPS = 100, int limitSize = 100)
        {
            switch (limitingType)
            {
                case LimitingType.TokenBucket:
                default:
                    return new TokenBucketLimitingService(maxQPS, limitSize, timeSpan);
                case LimitingType.LeakageBucket:
                    return new LeakageBucketLimitingService(maxQPS, limitSize, timeSpan);
            }
        }
    }
}
