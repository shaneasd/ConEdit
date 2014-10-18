using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using System.Windows.Forms;
using Conversation;
using Utilities;
using System.IO;

namespace PluginPack
{
    class ConversationsAsSsv : IProjectExporter
    {
        public string Name
        {
            get { return "Conversation as Semicolon Separated Values"; }
        }

        public void Export(IProject project, ConfigParameterString exportPath, Func<ID<LocalizedText>, string> localize)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.DefaultExt = "*.ssv";
                sfd.AddExtension = true;
                sfd.CreatePrompt = false;
                sfd.InitialDirectory = exportPath.Value;
                sfd.OverwritePrompt = true;
                sfd.ValidateNames = true;
                sfd.Title = "Export to C# source file";
                sfd.DefaultExt = ".ssv";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    exportPath.Value = Path.GetDirectoryName(sfd.FileName);
                    using (var stream = new FileStream(sfd.FileName, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        stream.SetLength(0);
                        using (var sw = new StreamWriter(stream))
                        {
                            ExportAsSsv ssv = new ExportAsSsv(localize);
                            ssv.WriteTitle(sw, true);
                            foreach (var con in project.Conversations)
                                ssv.WriteConversation(con, sw, true);
                        }
                    }
                }
            }
        }
    }
}
