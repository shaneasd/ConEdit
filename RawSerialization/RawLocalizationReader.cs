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

        struct Range
        {
            public uint start;
            public uint length;
        }

        private Dictionary<Id<LocalizedText>, string> m_stringsCache;

        private RawLocalizationReader(Stream stream)
        {
            using (BinaryReader r = new BinaryReader(stream, Encoding.UTF8, true))
            {
                uint count = r.ReadUInt32();

                Id<LocalizedText>[] ids = new Id<LocalizedText>[count];
                Range[] ranges = new Range[count];
                m_stringsCache = new Dictionary<Id<LocalizedText>, string>((int)count);
                uint maxLength = 0;
                for (uint i = 0; i < count; i++)
                {
                    ids[i] = Id<LocalizedText>.FromGuid(new Guid(r.ReadBytes(16)));
                    Range range;
                    range.start = r.ReadUInt32();
                    range.length = r.ReadUInt32();
                    ranges[i] = range;

                    maxLength = Math.Max(maxLength, range.length);
                }

                byte[] buffer = new byte[maxLength];

                for (uint i = 0; i < count; i++)
                {
                    stream.Position = ranges[i].start;
                    int length = (int)ranges[i].length;
                    r.Read(buffer, 0, length);
                    m_stringsCache[ids[i]] = Encoding.UTF8.GetString(buffer, 0, length);
                }
            }
        }

        public string Localize(Id<LocalizedText> key)
        {
            return m_stringsCache[key];
        }
    }
}
