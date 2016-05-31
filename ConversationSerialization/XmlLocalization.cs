﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Xml.Linq;
using System.IO;
using Utilities;
using System.Globalization;

namespace Conversation.Serialization
{
    public static class XmlLocalization
    {
        const string XML_VERSION = "1.0";
        const string ROOT = "Root";

        /// <summary>
        /// Deserialize only the localization data required by a conversation player (i.e the mapping of id to text)
        /// </summary>
        public class ClientDeserializer : IDeserializer<Dictionary<Id<LocalizedText>, string>>
        {
            public Dictionary<Id<LocalizedText>, string> Read(Stream stream)
            {
                Dictionary<Id<LocalizedText>, string> result = new Dictionary<Id<LocalizedText>, string>();
                XDocument doc = XDocument.Load(stream);
                XElement root = doc.Root;
                if (root.Attribute("xmlversion").Value != XML_VERSION)
                    throw new XmlVersionException("Unknown xml version in " + stream.ToString());
                var nodes = root.Elements("Localize");
                foreach (var node in nodes)
                {
                    var id = Id<LocalizedText>.Parse(node.Attribute("id").Value);
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
                    var id = Id<LocalizedText>.Parse(node.Attribute("id").Value);
                    DateTime localized = new DateTime(long.Parse(node.Attribute("localized").Value, CultureInfo.InvariantCulture));
                    string data = node.Value;
                    result.SetLocalized(id, new LocalizationElement(localized, data));
                }
                return result;
            }
        }

        public class Serializer : ISerializer<LocalizerData>
        {
            private Func<Id<LocalizedText>, bool> m_idUsed;
            private Func<IEnumerable<Id<LocalizedText>>> m_usedGuids;
            private Func<string, bool> m_shouldClean;
            private Func<string, bool> m_shouldExpand;
            private Func<Id<LocalizedText>, string> m_bestLocalization;
            private string m_file;

            public Serializer(Func<Id<LocalizedText>, bool> guidUsed, Func<IEnumerable<Id<LocalizedText>>> usedGuids, Func<string, bool> shouldClean, Func<string, bool> shouldExpand, Func<Id<LocalizedText>, string> bestLocalization, string file)
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
                var unused = data.AllLocalizations.Where(kvp => !m_idUsed(kvp.Key));
                var used = data.AllLocalizations.Where(kvp => m_idUsed(kvp.Key));
                var missing = m_usedGuids().Except(used.Select(kvp => kvp.Key));

                XElement root = new XElement(ROOT, new XAttribute("xmlversion", XML_VERSION));
                XDocument doc = new XDocument(root);

                SortedList<Id<LocalizedText>, XElement> elements = new SortedList<Id<LocalizedText>, XElement>();

                foreach (var kvp in used)
                {
                    elements.Add(kvp.Key, new XElement("Localize", new XAttribute("id", kvp.Key.Serialized()), new XAttribute("localized", kvp.Value.Localized.Ticks.ToString(CultureInfo.InvariantCulture)), kvp.Value.Text));
                }

                if (unused.Any())
                {
                    if (!m_shouldClean(m_file))
                    {
                        foreach (var kvp in unused)
                        {
                            elements.Add(kvp.Key, new XElement("Localize", new XAttribute("id", kvp.Key.Serialized()), new XAttribute("localized", kvp.Value.Localized.Ticks.ToString(CultureInfo.InvariantCulture)), kvp.Value.Text));
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
        public Dictionary<Id<LocalizedText>, LocalizationElement> m_data = new Dictionary<Id<LocalizedText>, LocalizationElement>();

        public bool CanLocalize(Id<LocalizedText> id)
        {
            return m_data.ContainsKey(id);
        }

        public LocalizationElement GetLocalized(Id<LocalizedText> id)
        {
            return m_data[id];
        }

        public void SetLocalized(Id<LocalizedText> id, LocalizationElement localized)
        {
            m_data[id] = localized;
        }

        public IEnumerable<KeyValuePair<Id<LocalizedText>, LocalizationElement>> AllLocalizations { get { return m_data; } }
    }
}
