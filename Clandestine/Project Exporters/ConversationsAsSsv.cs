using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using System.Windows.Forms;
using Conversation;
using Utilities;
using System.IO;

namespace Clandestine
{
    public class ConversationsAsSsv : IProjectExporter
    {
        public string Name => "Conversations as Semicolon Separated Values (Clandestine)";

        public void Export(IProject2 project, ConfigParameterString exportPath, Func<Id<LocalizedStringType>, Id<LocalizedText>, Tuple<string, DateTime>> localize, IErrorCheckerUtilities<IConversationNode> util)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (exportPath == null)
                throw new ArgumentNullException(nameof(exportPath));

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
                    FileStream stream = null;
                    try
                    {
                        stream = new FileStream(sfd.FileName, FileMode.OpenOrCreate, FileAccess.Write);
                        stream.SetLength(0);
                        using (var sw = new StreamWriter(stream))
                        {
                            stream = null;
                            ExportAsSsv ssv = new ExportAsSsv(localize);
                            CsvData.WriteTitle(";", sw, true);
                            foreach (var con in project.ConversationFilesCollection)
                                ssv.WriteConversation(con, sw, true, util);
                        }
                    }
                    finally
                    {
                        if (stream != null)
                            stream.Dispose();
                    }
                }
            }
        }
    }
}
