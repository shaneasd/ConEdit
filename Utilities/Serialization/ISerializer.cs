using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace Utilities
{
    public class DeserializerVersionMismatchException : Exception
    {
        public DeserializerVersionMismatchException(string expectedVersions, string encounteredVersion)
        {
            ExpectedVersions = expectedVersions;
            EncounteredVersion = encounteredVersion;
        }

        public string EncounteredVersion { get; }
        public string ExpectedVersions { get; }
        public override string Message
        {
            get
            {
                return "File version " + EncounteredVersion + " cannot be read. Expected versions are: " + ExpectedVersions;
            }
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
