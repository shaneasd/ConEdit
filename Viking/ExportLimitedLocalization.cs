using ConversationEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Conversation;
using System.Windows.Forms;
using System.IO;
using Conversation.Serialization;
using System.Xml.Linq;
using System.Globalization;
using Utilities;

namespace Viking
{
    class ExportLimitedLocalization : IFolderContextMenuItem, IConversationContextMenuItem
    {
        private Func<Id<LocalizedStringType>, Id<LocalizedText>, Tuple<string, DateTime>> m_localizer;

        public ExportLimitedLocalization(Func<Id<LocalizedStringType>, Id<LocalizedText>, Tuple<string, DateTime>> localizer)
        {
            m_localizer = localizer;
        }

        public string Name => "Export Localization";

        public void Execute(IEnumerable<IConversationFile> conversations)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.DefaultExt = "xml";
                sfd.AddExtension = true;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    Stream stream = null;
                    try
                    {
                        stream = sfd.OpenFile();
                        WriteLocalizationFile(stream, conversations);
                    }
                    finally
                    {
                        if (stream != null)
                            stream.Dispose();
                    }
                }
            }
        }

        public void Execute(IConversationFile conversation, IErrorCheckerUtilities<IConversationNode> util)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.DefaultExt = "xml";
                sfd.AddExtension = true;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    Stream stream = null;
                    try
                    {
                        stream = sfd.OpenFile();
                        WriteLocalizationFile(stream, conversation.Only());
                    }
                    finally
                    {
                        if (stream != null)
                            stream.Dispose();
                    }
                }
            }
        }

        private void WriteLocalizationFile(Stream stream, IEnumerable<IConversationFile> conversations)
        {
            //XmlLocalization.Context context = new XmlLocalization.Context(a => true, Enumerable.Empty<Id<LocalizedText>>());
            //var serializer = new XmlLocalization.Serializer(() => context, a => false, a => false, a => { throw new NotImplementedException(); }, null);

            //LocalizerData data = new LocalizerData();

            const string XML_VERSION = "1.0";
            const string ROOT = "Root";

            XElement root = new XElement(ROOT, new XAttribute("xmlversion", XML_VERSION));
            XDocument doc = new XDocument(root);

            foreach (var conversation in conversations)
            {
                root.Add(new XComment(conversation.File.File.Name));
                var info = conversation.Nodes.SingleOrDefault(a => a.Data.NodeTypeId == Id<NodeTypeTemp>.Parse("d5974ffe-777b-419c-b9bc-bde980cb99a6"));
                if (info != null)
                {
                    var context = info.Data.Parameters.Where(p => p.Id == Id<Parameter>.Parse("6940a618-5905-4e81-a59b-281d92a90782")).Select(a => a as IStringParameter).SingleOrDefault();
                    if (context != null)
                    {
                        root.Add(new XComment(context.Value));
                    }
                }

                var startNodes = conversation.Nodes.Where(n => n.Data.NodeTypeId == Id<NodeTypeTemp>.Parse("656a48b2-324e-4484-a1b1-c3b91ad10c3e"));
                HashSet<IConversationNodeData> added = new HashSet<IConversationNodeData>();

                Action<IConversationNodeData> addNode = null;
                addNode = node =>
                {
                    if (!added.Contains(node))
                    {
                        added.Add(node);
                        string speaker = node.Parameters.Where(p => p.Id == Id<Parameter>.Parse("08da4734-e5a3-4dec-807e-29628ef4ba3e") || p.Id == Id<Parameter>.Parse("d6a6b382-43d0-44d3-b4e7-b9c9362a509b")).OfType<IDynamicEnumParameter>().Select(e => e.DisplayValue((a, b) => null)).SingleOrDefault() ?? "";

                        foreach (var localized in node.Parameters.OfType<ILocalizedStringParameter>())
                        {
                            var key = localized.Value;
                            var type = Id<LocalizedStringType>.FromGuid(localized.TypeId.Guid);
                            var data = m_localizer(type, key);
                            var value = data.Item1;
                            var date = data.Item2;
                            var element = new XElement("Localize", new XAttribute("id", key.Serialized()),
                                                                   new XAttribute("type", type.Serialized()),
                                                                   new XAttribute("localized", date.Ticks.ToString(CultureInfo.InvariantCulture)),
                                                                   new XAttribute("speaker", speaker),
                                                                   new XAttribute("type", node.Name),
                                                                   new XAttribute("parameter", localized.Name),
                                                                   value);
                            root.Add(element);
                        }
                        foreach (var output in node.Connectors.Where(c => c.Definition.Id == SpecialConnectors.Output.Id))
                        {
                            foreach (var connection in output.Connections)
                            {
                                addNode(connection.Parent);
                            }
                        }
                    }
                };

                foreach (var start in startNodes)
                {
                    addNode(start.Data);
                }

                foreach (var node in conversation.Nodes.Select(n => n.Data).Where(n => !added.Contains(n)))
                {
                    addNode(node);
                }

                //foreach (var node in conversation.Nodes)
                //{
                //    string speaker = node.Data.Parameters.Where(p => p.Id == Id<Parameter>.Parse("08da4734-e5a3-4dec-807e-29628ef4ba3e") || p.Id == Id<Parameter>.Parse("d6a6b382-43d0-44d3-b4e7-b9c9362a509b")).OfType<IDynamicEnumParameter>().Select(e => e.DisplayValue(a => null)).SingleOrDefault() ?? "";

                //    foreach (var localized in node.Data.Parameters.OfType<ILocalizedStringParameter>())
                //    {
                //        var key = localized.Value;
                //        var data = m_localizer(key);
                //        var value = data.Item1;
                //        var date = data.Item2;
                //        var element = new XElement("Localize", new XAttribute("id", key.Serialized()),
                //                                               new XAttribute("localized", date.Ticks.ToString(CultureInfo.InvariantCulture)),
                //                                               new XAttribute("speaker", speaker),
                //                                               new XAttribute("type", node.Data.Name),
                //                                               new XAttribute("parameter", localized.Name),
                //                                               value);
                //        root.Add(element);
                //    }
                //}
            }

            stream.Position = 0;
            stream.SetLength(0);
            doc.Save(stream);
            stream.Flush();
        }
    }
}
