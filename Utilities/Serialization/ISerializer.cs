using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace Utilities
{
    public interface ISerializer<in T>
    {
        void Write(T data, Stream stream);
    }

    public interface IDeserializer<out T>
    {
        T Read(Stream stream);
    }

    public interface ISerializerDeserializer<T> : ISerializer<T>, IDeserializer<T>
    {
    }

    public interface ISerializerXml<in T>
    {
        void Write(T data, XElement node);
    }

    public interface IDeserializerXml<out T>
    {
        T Read(XElement node);
    }

    public interface ISerializerDeserializerXml<in TIn, out TOut> : ISerializerXml<TIn>, IDeserializerXml<TOut>
    {
    }
}
