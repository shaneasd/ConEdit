using System;
using Conversation;
using ConversationEditor;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace Viking
{
    internal class ExportLocalizationLines : IConversationContextMenuItem
    {
        private Func<Id<LocalizedText>, Tuple<string, DateTime>> localizer;

        public ExportLocalizationLines(Func<Id<LocalizedText>, Tuple<string, DateTime>> localizer)
        {
            this.localizer = localizer;
        }

        public string Name
        {
            get
            {
                return "Export Localization Lines";
            }
        }

        public void Execute(IConversationFile conversation, IErrorCheckerUtilities<IConversationNode> util)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.DefaultExt = "*.ssv";
                sfd.AddExtension = true;
                sfd.CreatePrompt = false;
                sfd.OverwritePrompt = true;
                sfd.ValidateNames = true;
                sfd.Title = "Export to C# source file";
                sfd.DefaultExt = ".ssv";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var sw = new StreamWriter(sfd.FileName, false))
                    {
                        foreach (var node in conversation.Nodes)
                        {
                            foreach (var parameter in node.Parameters.OfType<ILocalizedStringParameter>())
                            {
                                var id = parameter.Value;
                                sw.WriteLine("<Localize id =\"" + id.Serialized() + "\" localized =\"" + localizer(id).Item2.Ticks.ToString() + "\">" + localizer(id).Item1 + "</Localize>");
                            }
                        }
                    }
                }
            }
        }
    }
}