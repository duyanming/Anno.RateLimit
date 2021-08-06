using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.RateLimit
{
    public class LimitedQueue<T> : Queue<T>
    {
        private int limit = 0;
        public const string QueueFulled = "TTP-StreamLimiting-1001";

        public int Limit
        {
            get { return limit; }
            set { limit = value; }
        }

        public LimitedQueue()
            : this(0)
        { }

        public LimitedQueue(int limit)
            : base(limit)
        {
            this.Limit = limit;
        }

        public new bool Enqueue(T item)
        {
            if (limit > 0 && this.Count >= this.Limit)
            {
                return false;
            }
            base.Enqueue(item);
            return true;
        }
    }
}
