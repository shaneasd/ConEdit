using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using Utilities;

namespace ConversationEditor
{
    //using TData = Tuple<IEnumerable<string>, IEnumerable<string>, IEnumerable<string>>;

    internal static class XMLProject
    {
        public const string XML_VERSION = "1.0";
        public const string ROOT = "Root";

        public class Serializer : ISerializer<Project.TData>
        {
            public void Write(Project.TData data, Stream stream)
            {
                var conversations = data.Conversations.Select(c => new XElement("Conversation", new XAttribute("path", c)));
                var localizers = data.Localizations.Select(l => new XElement("Localization", new XAttribute("path", l)));
                var domains = data.Domains.Select(d => new XElement("Domain", new XAttribute("path", d)));
                var audios = data.Audios.Select(a => new XElement("Audio", new XAttribute("path", a)));
                var root = new XElement(ROOT, (new XAttribute("xmlversion", XML_VERSION)).Only().Concat<object>(conversations).Concat(localizers).Concat(domains).Concat(audios).ToArray());
                XDocument doc = new XDocument(root);
                stream.SetLength(0);
                doc.Save(stream);
            }
        }

        public class Deserializer : IDeserializer<Project.TData>
        {
            public Project.TData Read(Stream projectFile)
            {
                projectFile.Position = 0;
                XDocument doc = XDocument.Load(projectFile);
                var root = doc.Root;
                var conversations = root.Elements("Conversation");
                IEnumerable<string> conversationPaths = conversations.Select(n => n.Attribute("path").Value);
                var localizers = root.Elements("Localization");
                IEnumerable<string> localizerPaths = localizers.Select(n => n.Attribute("path").Value);
                var domains = root.Elements("Domain");
                IEnumerable<string> domainPaths = domains.Select(n => n.Attribute("path").Value);
                var audios = root.Elements("Audio");
                IEnumerable<string> audioPaths = audios.Select(n => n.Attribute("path").Value);

                return new Project.TData(conversationPaths, domainPaths, localizerPaths, audioPaths);
            }
        }

        public class SerializerDeserializer : ISerializerDeserializer<Project.TData>
        {
            Serializer m_serializer;
            Deserializer m_deserializer;

            public SerializerDeserializer()
            {
                m_serializer = new Serializer();
                m_deserializer = new Deserializer();
            }

            public void Write(Project.TData data, Stream stream)
            {
                m_serializer.Write(data, stream);
            }

            public Project.TData Read(Stream stream)
            {
                return m_deserializer.Read(stream);
            }
        }
    }
}
