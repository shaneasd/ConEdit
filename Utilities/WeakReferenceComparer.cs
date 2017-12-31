using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class WeakReferenceComparer<T> : IEqualityComparer<WeakReference<T>> where T : class
    {
        public bool Equals(WeakReference<T> x, WeakReference<T> y)
        {
            bool xAlive = x.TryGetTarget(out T xVal);
            bool yAlive = y.TryGetTarget(out T yVal);
            if (xAlive && yAlive)
                return object.Equals(xVal, yVal);
            else
                return false;
        }

        public int GetHashCode(WeakReference<T> obj)
        {
            bool xAlive = obj.TryGetTarget(out T xVal);
            if (xAlive)
                return xVal.GetHashCode();
            else
                return obj.GetHashCode(); //In this scenario x is not equal to anything (including itself) so we want to minimise the chances of matching hash codes (without resorting to randomness)
        }
    }
}
