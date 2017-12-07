using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using Utilities;
using Conversation;

namespace ConversationEditor
{
    //using TData = Tuple<IEnumerable<string>, IEnumerable<string>, IEnumerable<string>>;

    internal static class XMLProject
    {
        public const string XML_VERSION = "1.1";
        public const string ROOT = "Root";

        public class Serializer : ISerializer<Project.TData>
        {
            XElement MakeLocalizerSetElement(Project.TData.LocalizerSetData localizerSet)
            {
                var result = new XElement("LocalizationSet",
                                          new XAttribute("name", localizerSet.Name),
                                          new XAttribute("guid", localizerSet.Id.Serialized()));
                foreach (var mapping in localizerSet.Sources)
                {

                    result.Add(new XElement("Source", new XAttribute("type", mapping.Key.Serialized()), new XAttribute("file", mapping.Value.Serialized())));
                }
                return result;
            }

            public void Write(Project.TData data, Stream stream)
            {
                var conversations = data.Conversations.Select(x => new XElement("Conversation", new XAttribute("path", x.Path.RelativePath), new XAttribute("guid", x.Id.Serialized())));
                var localizers = data.Localizations.Select(x => new XElement("Localization", new XAttribute("path", x.Path.RelativePath), new XAttribute("guid", x.Id.Serialized())));
                var localizerSets = data.LocalizationSets.Select(MakeLocalizerSetElement);
                var domains = data.Domains.Select(x => new XElement("Domain", new XAttribute("path", x.Path.RelativePath), new XAttribute("guid", x.Id.Serialized())));
                var audios = data.Audios.Select(x => new XElement("Audio", new XAttribute("path", x.Path.RelativePath), new XAttribute("guid", x.Id.Serialized())));
                var root = new XElement(ROOT, (new XAttribute("xmlversion", XML_VERSION)).Only().Concat<object>(conversations).Concat(localizers).Concat(domains).Concat(audios).Concat(localizerSets).ToArray());
                XDocument doc = new XDocument(root);
                stream.SetLength(0);
                doc.Save(stream);
            }
        }

        public class Deserializer : IDeserializer<Project.TData>
        {
            private DirectoryInfo m_origin;

            public Deserializer(DirectoryInfo origin)
            {
                m_origin = origin;
            }

            public Project.TData Read(Stream projectFile)
            {
                projectFile.Position = 0;
                XDocument doc = XDocument.Load(projectFile);
                var root = doc.Root;
                string encounteredVersion = root.Attribute("xmlversion").Value;
                if (encounteredVersion != "1.1")
                    throw new DeserializerVersionMismatchException("1.1", encounteredVersion);
                var conversations = root.Elements("Conversation");
                IEnumerable<Project.TData.FileIdAndPath> conversationPaths = conversations.Select(n => new Project.TData.FileIdAndPath((Id<FileInProject>.Parse(n.Attribute("guid").Value)), DocumentPath.FromPath(n.Attribute("path").Value, m_origin)));
                var localizers = root.Elements("Localization");
                IEnumerable<Project.TData.FileIdAndPath> localizerPaths = localizers.Select(n => new Project.TData.FileIdAndPath((Id<FileInProject>.Parse(n.Attribute("guid").Value)), DocumentPath.FromPath(n.Attribute("path").Value, m_origin)));
                var domains = root.Elements("Domain");
                IEnumerable<Project.TData.FileIdAndPath> domainPaths = domains.Select(n => new Project.TData.FileIdAndPath((Id<FileInProject>.Parse(n.Attribute("guid").Value)), DocumentPath.FromPath(n.Attribute("path").Value, m_origin)));
                var audios = root.Elements("Audio");
                IEnumerable<Project.TData.FileIdAndPath> audioPaths = audios.Select(n => new Project.TData.FileIdAndPath((Id<FileInProject>.Parse(n.Attribute("guid").Value)), DocumentPath.FromPath(n.Attribute("path").Value, m_origin)));
                var localizationSets = root.Elements("LocalizationSet");
                List<Project.TData.LocalizerSetData> localizationMappings = new List<Project.TData.LocalizerSetData>();
                foreach (var set in localizationSets)
                {
                    string name = set.Attribute("name").Value;
                    Id<Project.TData.LocalizerSetData> id = Id<Project.TData.LocalizerSetData>.Parse(set.Attribute("guid").Value);
                    IReadOnlyDictionary<Id<LocalizedStringType>, Id<FileInProject>> sources = set.Elements("Source").ToDictionary(e => Id<LocalizedStringType>.Parse(e.Attribute("type").Value), e => Id<FileInProject>.Parse(e.Attribute("file").Value));
                    localizationMappings.Add(new Project.TData.LocalizerSetData(id, name, sources));
                }

                return new Project.TData(conversationPaths, domainPaths, localizerPaths, audioPaths, localizationMappings);
            }
        }

        public class SerializerDeserializer : ISerializerDeserializer<Project.TData>
        {
            Serializer m_serializer;
            Deserializer m_deserializer;

            public SerializerDeserializer(DirectoryInfo origin)
            {
                m_serializer = new Serializer();
                m_deserializer = new Deserializer(origin);
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
