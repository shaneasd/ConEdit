using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Utilities
{
    static class AssemblyUtils
    {
        public static IEnumerable<Type> TypesAssignableFrom<T>(this Assembly assembly)
        {
            return assembly.GetTypes().Where(t => typeof(T).IsAssignableFrom(t));
        }
    }
}
