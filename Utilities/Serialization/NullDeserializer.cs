using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class NullDeserializer<T> : IDeserializerXml<T>, IDeserializer<T>
    {
        public static NullDeserializer<T> Instance { get; } = new NullDeserializer<T>();

        public T Read(System.Xml.Linq.XElement node)
        {
            return default;
        }

        public T Read(System.IO.Stream stream)
        {
            return default;
        }
    }
}
