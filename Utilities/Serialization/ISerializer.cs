using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Runtime.Serialization;

namespace Utilities
{
    [Serializable]
    public class DeserializerVersionMismatchException : Exception
    {
        private static string MakeMessage(string expectedVersions, string encounteredVersion)
        {
            return "File version " + encounteredVersion + " cannot be read. Expected versions are: " + expectedVersions;
        }

        public DeserializerVersionMismatchException() : base()
        {
        }

        public DeserializerVersionMismatchException(string message) : base(message)
        {
        }

        public DeserializerVersionMismatchException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DeserializerVersionMismatchException(SerializationInfo info, StreamingContext context)
        {
        }

        public DeserializerVersionMismatchException(string expectedVersions, string encounteredVersion) : base(MakeMessage(expectedVersions, encounteredVersion))
        {
        }
    }

    public interface ISerializer<in T>
    {
        void Write(T data, Stream stream);
    }

    /// <summary>
    /// Read data from stream to generate the returned object.
    /// May throw DeserializerVersionMismatchException to indicate that the version of data in the stream is incompatible with this reader
    /// </summary>
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
