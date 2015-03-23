using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Xml.Linq;
using System.IO;
using Utilities;

namespace Conversation.Serialization
{
    public class XmlLocalization
    {
        const string XML_VERSION = "1.0";
        const string ROOT = "Root";

        public class ClientDeserializer : IDeserializer<Dictionary<ID<LocalizedText>, string>>
        {
            public Dictionary<ID<LocalizedText>, string> Read(Stream stream)
            {
                Dictionary<ID<LocalizedText>, string> result = new Dictionary<ID<LocalizedText>, string>();
                XDocument doc = XDocument.Load(stream);
                XElement root = doc.Root;
                if (root.Attribute("xmlversion").Value != XML_VERSION)
                    throw new Exception("Unknown xml version in " + stream.ToString());
                var nodes = root.Elements("Localize");
                foreach (var node in nodes)
                {
                    var id = ID<LocalizedText>.Parse(node.Attribute("id").Value);
                    string data = node.Value;
                    result[id] = data;
                }
                return result;
            }
        }

        public class Deserializer : IDeserializer<LocalizerData>
        {
            public LocalizerData Read(Stream stream)
            {
                LocalizerData result = new LocalizerData();
                XDocument doc = XDocument.Load(stream);
                XElement root = doc.Root;
                if (root.Attribute("xmlversion").Value != XML_VERSION)
                    throw new Exception("Unknown xml version in " + stream.ToString());
                var nodes = root.Elements("Localize");
                foreach (var node in nodes)
                {
                    var id = ID<LocalizedText>.Parse(node.Attribute("id").Value);
                    DateTime localized = new DateTime(long.Parse(node.Attribute("localized").Value));
                    string data = node.Value;
                    result.m_data[id] = new LocalizationElement(localized, data);
                }
                return result;
            }
        }

        public class Serializer : ISerializer<LocalizerData>
        {
            private Func<ID<LocalizedText>, bool> m_idUsed;
            private Func<IEnumerable<ID<LocalizedText>>> m_usedGuids;
            private Func<string, bool> m_shouldClean;
            private Func<string, bool> m_shouldExpand;
            private Func<ID<LocalizedText>, string> m_bestLocalization;
            private string m_file;

            public Serializer(Func<ID<LocalizedText>, bool> guidUsed, Func<IEnumerable<ID<LocalizedText>>> usedGuids, Func<string, bool> shouldClean, Func<string, bool> shouldExpand, Func<ID<LocalizedText>, string> bestLocalization, string file)
            {
                m_idUsed = guidUsed;
                m_usedGuids = usedGuids;
                m_shouldClean = shouldClean;
                m_shouldExpand = shouldExpand;
                m_bestLocalization = bestLocalization;
                m_file = file;
            }

            public void Write(LocalizerData data, Stream stream)
            {
                var unused = data.m_data.Where(kvp => !m_idUsed(kvp.Key));
                var used = data.m_data.Where(kvp => m_idUsed(kvp.Key));
                var missing = m_usedGuids().Except(used.Select(kvp => kvp.Key));

                XElement root = new XElement(ROOT, new XAttribute("xmlversion", XML_VERSION));
                XDocument doc = new XDocument(root);

                SortedList<ID<LocalizedText>, XElement> elements = new SortedList<ID<LocalizedText>, XElement>();

                foreach (var kvp in used)
                {
                    elements.Add(kvp.Key, new XElement("Localize", new XAttribute("id", kvp.Key.Serialized()), new XAttribute("localized", kvp.Value.Localized.Ticks.ToString()), kvp.Value.Text));
                }

                if (unused.Any())
                {
                    if (!m_shouldClean(m_file))
                    {
                        foreach (var kvp in unused)
                        {
                            elements.Add(kvp.Key, new XElement("Localize", new XAttribute("id", kvp.Key.Serialized()), new XAttribute("localized", kvp.Value.Localized.Ticks.ToString()), kvp.Value.Text));
                        }
                    }
                }

                if (missing.Any())
                {
                    if (m_shouldExpand(m_file))
                    {
                        foreach (var guid in missing)
                        {
                            var bestLocalization = m_bestLocalization(guid);
                            if (bestLocalization != null)
                                elements.Add(guid, new XElement("Localize", new XAttribute("id", guid.Serialized()), new XAttribute("localized", "0"), bestLocalization));
                        }
                    }
                }

                foreach (var element in elements.Values)
                    root.Add(element);

                stream.Position = 0;
                stream.SetLength(0);
                doc.Save(stream);
                stream.Flush();
            }
        }
    }

    public class LocalizerData
    {
        public Dictionary<ID<LocalizedText>, LocalizationElement> m_data = new Dictionary<ID<LocalizedText>, LocalizationElement>();
    }
}
