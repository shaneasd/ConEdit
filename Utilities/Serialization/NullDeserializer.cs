using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class NullDeserializer<T> : IDeserializerXml<T>, IDeserializer<T>
    {
        public static NullDeserializer<T> Instance = new NullDeserializer<T>();

        public T Read(System.Xml.Linq.XElement node)
        {
            return default(T);
        }

        public T Read(System.IO.Stream stream)
        {
            return default(T);
        }
    }

    public static class ConstantDeserializer
    {
        public static ConstantDeserializer<T> Make<T>(T value)
        {
            return new ConstantDeserializer<T>(value);
        }
    }

    public class ConstantDeserializer<T> : IDeserializerXml<T>, IDeserializer<T>
    {
        private T m_value;

        public ConstantDeserializer(T value)
        {
            m_value = value;
        }

        public T Read(System.Xml.Linq.XElement node)
        {
            return m_value;
        }

        public T Read(System.IO.Stream stream)
        {
            return m_value;
        }
    }
}
