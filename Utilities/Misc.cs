using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Utilities
{
    public static class Misc
    {
        public static void Execute(this Action action)
        {
            action?.Invoke();
        }

        public static void Execute<T>(this Action<T> action, T t)
        {
            action?.Invoke(t);
        }

        public static void Execute<T1, T2>(this Action<T1, T2> action, T1 t1, T2 t2)
        {
            action?.Invoke(t1, t2);
        }

        public static void Execute<T1, T2, T3>(this Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
        {
            action?.Invoke(t1, t2, t3);
        }

        public static int GetDecimalPlaces(decimal value)
        {
            value = Math.Abs(value);
            value = decimal.Remainder(value, 1);
            int decimalPlaces = 0;
            while (value > 0)
            {
                decimalPlaces++;
                value = decimal.Remainder(value * 10, 1);
            }
            return decimalPlaces;
        }

        public static FileInfo FileInfo(this FileStream file)
        {
            return new FileInfo(file.Name);
        }
    }
}
