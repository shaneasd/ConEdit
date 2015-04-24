using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public static class MyMath
    {
        public static float Ceiling(float val)
        {
            return (float)Math.Ceiling(val);
        }

        /// <param name="i">must be non-negative</param>
        public static bool IsPowerOf2(int i)
        {
            return i > 0 && ((~i + 1) & i) == i;
        }
    }
}
