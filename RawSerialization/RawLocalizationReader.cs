using Conversation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RawSerialization
{
    public class RawLocalizationReader
    {
        [Serializable]
        public class ReadException : Exception
        {
            public ReadException() : base()
            {
            }

            public ReadException(string message) : base(message)
            {
            }

            public ReadException(string message, Exception inner) : base(message, inner)
            {
            }

            protected ReadException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }

        public static RawLocalizationReader Read(Stream stream)
        {
            return new RawLocalizationReader(stream);
        }

        private Dictionary<Id<LocalizedText>, string> m_stringsCache;

        private RawLocalizationReader(Stream stream)
        {
            using (BinaryReader r = new BinaryReader(stream, Encoding.UTF8, true))
            {
                uint version = r.ReadUInt32();
                if (version != 0)
                    throw new ReadException("Unexpected version");
                uint count = r.ReadUInt32();
                m_stringsCache = new Dictionary<Id<LocalizedText>, string>((int)count);

                for (uint i = 0; i < count; i++)
                {
                    var id = Id<LocalizedText>.FromGuid(new Guid(r.ReadBytes(16)));
                    m_stringsCache[id] = r.ReadString();
                    //m_stringsCache[id] = Encoding.UTF8.GetString(buffer, 0, (int)length);
                }
            }
        }

        public string Localize(Id<LocalizedText> key)
        {
            return m_stringsCache[key];
        }
    }
}
