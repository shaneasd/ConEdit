using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    //Same as .NET 4.5 version (hopefully). Not used anymore.
    class LegacyWeakReference<T> where T : class
    {
        WeakReference w;
        public LegacyWeakReference(T value)
        {
            w = new WeakReference(value);
        }
        public bool TryGetTarget(out T target)
        {
            if (!w.IsAlive)
            {
                target = null;
                return false;
            }
            else
            {
                try
                {
                    target = (T)w.Target;
                    return true;
                }
                catch (InvalidOperationException)
                {
                    target = null;
                    return false;
                }
            }
        }
    }

}
