using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public static class Identities<T>
    {
        public static bool True(T ignore) { return true; }
    }
}
