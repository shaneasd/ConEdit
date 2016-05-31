using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    /// <summary>
    /// Boolean passed by reference
    /// </summary>
    public class BoolRef
    {
        public bool Value
        {
            get; set;
        }

        public static bool operator true(BoolRef x)
        {
            return x.Value;
        }
        public static bool operator false(BoolRef x)
        {
            return !x.Value;
        }

        public static implicit operator BoolRef(bool x)
        {
            return new BoolRef { Value = x };
        }

        public static bool operator &(BoolRef x, bool y)
        {
            return x.Value & y;
        }

        public static bool operator |(BoolRef x, bool y)
        {
            return x.Value | y;
        }

        public static bool operator !(BoolRef x)
        {
            return !x.Value;
        }
    }
}
