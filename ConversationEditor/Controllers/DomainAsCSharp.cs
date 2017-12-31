using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Conversation;
using Conversation.Serialization;
using Utilities;
using System.Collections.Generic;

namespace ConversationEditor
{
    internal class DomainAsCSharp : IProjectExporter
    {
        public static Dictionary<ParameterType, string> BasicTypeMap()
        {
            var typeMap = new Dictionary<ParameterType, string>();
            typeMap[BaseTypeInteger.ParameterType] = "Int32";
            typeMap[BaseTypeDecimal.ParameterType] = "Decimal";
            typeMap[StringParameter.ParameterType] = "String";
            typeMap[BaseTypeLocalizedString.ParameterType] = "RuntimeConversation.LocalizedString"; //TODO: LOC: CODE GENERATION: Doesn't properly deal with user defined localized strings
            typeMap[BooleanParameter.ParameterType] = "Boolean";
            typeMap[AudioParameter.ParameterType] = "RuntimeConversation.Audio";
            return typeMap;
        }

        public void Export(IProject2 project, ConfigParameterString exportPath, Func<Id<LocalizedStringType>, Id<LocalizedText>, Tuple<string, DateTime>> localize, IErrorCheckerUtilities<IConversationNode> util)
        {
            DomainData builtIn = new DomainData();
            builtIn.Connectors.Add(SpecialConnectors.Input);
            builtIn.Connectors.Add(SpecialConnectors.Output);
            var data = project.DomainFilesCollection.Select(d => d.Data).Concat(builtIn.Only());
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
                    string @namespace = project.File.File.Name;
                    @namespace = Path.ChangeExtension(@namespace, null);
                    @namespace = new string(@namespace.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
                    if (@namespace.Length == 0)
                        @namespace = "MyNamespace";
                    while (char.IsDigit(@namespace.First()))
                    {
                        @namespace = @namespace.Substring(1);
                        if (@namespace.Length == 0)
                            @namespace = "MyNamespace";
                    }
                    CSDomainSerializer<INodeGui, NodeUIData, ConversationEditorData> s = new CSDomainSerializer<INodeGui, NodeUIData, ConversationEditorData>(BasicTypeMap(), @namespace);
                    using (var stream = new FileStream(sfd.FileName, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        stream.SetLength(0);
                        s.Write(data, stream);
                    }
                }
            }
        }

        public string Name => "Domain as C#";
    }

}
