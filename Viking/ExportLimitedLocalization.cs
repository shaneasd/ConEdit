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

namespace Viking
{
    class ExportLimitedLocalization : IFolderContextMenuItem
    {
        private Func<Id<LocalizedText>, Tuple<string, DateTime>> m_localizer;

        public ExportLimitedLocalization(Func<Id<LocalizedText>, Tuple<string, DateTime>> localizer)
        {
            m_localizer = localizer;
        }

        public string Name
        {
            get
            {
                return "Export Localization";
            }
        }

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
                foreach (var node in conversation.Nodes)
                {
                    foreach (var localized in node.Parameters.OfType<ILocalizedStringParameter>())
                    {
                        var key = localized.Value;
                        var data = m_localizer(key);
                        var value = data.Item1;
                        var date = data.Item2;
                        var element = new XElement("Localize", new XAttribute("id", key.Serialized()), new XAttribute("localized", date.Ticks.ToString(CultureInfo.InvariantCulture)), value);
                        root.Add(element);
                    }
                }
            }

            stream.Position = 0;
            stream.SetLength(0);
            doc.Save(stream);
            stream.Flush();
        }
    }
}
