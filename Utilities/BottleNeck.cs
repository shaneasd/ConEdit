using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class BottleneckUtil
    {
        public static Func<T, Result> Bottleneck<T, Result>(this Func<T, Result> function)
        {
            object sync = new object();
            return x =>
            {
                lock (sync)
                {
                    return function(x);
                }
            };
        }
    }
}
