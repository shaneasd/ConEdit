using Conversation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawSerialization
{
    public class RawLocalizationReader
    {
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
                    throw new Exception("Unexpected version");
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
