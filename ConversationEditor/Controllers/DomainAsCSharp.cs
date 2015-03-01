using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Conversation;
using Conversation.Serialization;
using Utilities;

namespace ConversationEditor.Controllers
{
    class DomainAsCSharp : IProjectExporter
    {
        public void Export(IProject project, ConfigParameterString exportPath, Func<ID<LocalizedText>, string> localize, IErrorCheckerUtilities util)
        {
            DomainData builtIn = new DomainData();
            builtIn.Connectors.Add(SpecialConnectors.Input);
            builtIn.Connectors.Add(SpecialConnectors.Output);
            var data = project.DomainFiles.Select(d => d.Data).Concat(builtIn.Only());
            using (var sfd = new SaveFileDialog())
            {
                sfd.DefaultExt = "*.cs";
                sfd.AddExtension = true;
                sfd.CreatePrompt = false;
                sfd.InitialDirectory = exportPath.Value;
                sfd.OverwritePrompt = true;
                sfd.ValidateNames = true;
                sfd.Title = "Export to C# source file";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    exportPath.Value = Path.GetDirectoryName(sfd.FileName);
                    CsDomain<INodeGUI, NodeUIData, ConversationEditorData>.Serializer s = new CsDomain<INodeGUI, NodeUIData, ConversationEditorData>.Serializer(BaseTypeSet.BasicTypeMap(), "MyNamespace");
                    using (var stream = new FileStream(sfd.FileName, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        stream.SetLength(0);
                        s.Write(data, stream);
                    }
                }
            }
        }

        public string Name
        {
            get { return "Domain as C#"; }
        }
    }

}
