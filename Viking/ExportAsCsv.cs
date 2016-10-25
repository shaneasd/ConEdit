using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using Conversation;
using System.IO;
using System.Windows.Forms;

namespace Viking
{
    struct CsvData
    {
        public string Character;
        public string Script;
        public string Subtitle;
        public string Direction;
        public string Language;
        public string Context;
        public string Notes;
        public string Audio;

        public static Id<NodeTypeTemp> PLAYER_SPEECH = Id<NodeTypeTemp>.FromGuid(Guid.Parse("da1dfd60-28e4-48f6-93e6-298f3a68b67c"));
        public static Id<NodeTypeTemp> NPC_SPEECH = Id<NodeTypeTemp>.FromGuid(Guid.Parse("196dc521-8336-4714-93b7-77ac09b3abd7"));
        public static Id<NodeTypeTemp> RADIO_SPEECH = Id<NodeTypeTemp>.FromGuid(Guid.Parse("836eeb03-2730-4830-b192-3fc9ce41503e"));
        public static Id<Parameter> SPEECH_CHARACTER = Id<Parameter>.FromGuid(Guid.Parse("af08c7f7-33e9-4429-9e1f-cd786a73041b"));
        public static Id<Parameter> SPEECH_SCRIPT = Id<Parameter>.FromGuid(Guid.Parse("0154fe0e-5f53-4657-acae-b983ea9030a0"));
        public static Id<Parameter> SPEECH_SUBTITLE = Id<Parameter>.FromGuid(Guid.Parse("8987655a-92fe-4eca-8c50-8769a7edcf04"));
        public static Id<Parameter> SPEECH_DIRECTION = Id<Parameter>.FromGuid(Guid.Parse("98401421-e713-4014-8489-b8675c566179"));
        public static Id<Parameter> SPEECH_AUDIO = Id<Parameter>.FromGuid(Guid.Parse("d081e3ec-91f2-4ec1-ab20-a4a1b01162b7"));
        public static Id<Parameter> SPEECH_LANGUAGE = Id<Parameter>.FromGuid(Guid.Parse("803d384c-edc1-45e3-9bfe-88f99384d86d"));

        public static Id<NodeTypeTemp> OPTION = Id<NodeTypeTemp>.FromGuid(Guid.Parse("86524441-8da7-4e19-9ff3-c8df67e09f8f"));
        public static Id<Parameter> OPTION_SCRIPT = Id<Parameter>.FromGuid(Guid.Parse("e8b15360-c500-434e-856c-bd1090c1b4a2"));
        public static Id<Parameter> OPTION_SUBTITLES = Id<Parameter>.FromGuid(Guid.Parse("b9950a3d-14d9-46d0-94dd-4217ed1573ad"));
        public static Id<Parameter> OPTION_DIRECTION = Id<Parameter>.FromGuid(Guid.Parse("a4f77304-57b1-4e25-849a-3e199f0f1795"));
        public static Id<Parameter> OPTION_AUDIO = Id<Parameter>.FromGuid(Guid.Parse("266217e5-8504-4576-a5c8-428c86c9b73a"));
        public static Id<Parameter> OPTION_LANGUAGE = Id<Parameter>.FromGuid(Guid.Parse("ea771838-a6bb-45d5-bf94-e29fde56e284"));

        public static Id<NodeTypeTemp> CONVERSATIONINFO = Id<NodeTypeTemp>.FromGuid(Guid.Parse("d5974ffe-777b-419c-b9bc-bde980cb99a6"));
        public static Id<Parameter> CONVERSATIONINFO_CONTEXT = Id<Parameter>.FromGuid(Guid.Parse("6940a618-5905-4e81-a59b-281d92a90782"));
        public static Id<Parameter> CONVERSATIONINFO_NOTES = Id<Parameter>.FromGuid(Guid.Parse("cb4a4ac9-a5e9-444f-a7b0-b8f15e31e77a"));

        public static CsvData GetOptionData(ConversationNode<INodeGui> node, ConversationNode<INodeGui> conversationInfo, Func<Id<LocalizedText>, string> localize)
        {
            return new CsvData()
            {
                Character = "Player",
                Script = node.Data.Parameters.Single(p => p.Id == CsvData.OPTION_SCRIPT).DisplayValue(localize),
                Subtitle = node.Data.Parameters.Single(p => p.Id == CsvData.OPTION_SUBTITLES).DisplayValue(localize),
                Direction = node.Data.Parameters.Single(p => p.Id == CsvData.OPTION_DIRECTION).DisplayValue(localize),
                Language = node.Data.Parameters.Single(p => p.Id == CsvData.OPTION_LANGUAGE).DisplayValue(localize),
                Context = conversationInfo != null ? conversationInfo.Data.Parameters.Single(p => p.Id == CsvData.CONVERSATIONINFO_CONTEXT).DisplayValue(localize) : "",
                Notes = conversationInfo != null ? conversationInfo.Data.Parameters.Single(p => p.Id == CsvData.CONVERSATIONINFO_NOTES).DisplayValue(localize) : "",
                Audio = node.Data.Parameters.Single(p => p.Id == CsvData.OPTION_AUDIO).ValueAsString(),
            };
        }

        public static CsvData GetSpeechData(ConversationNode<INodeGui> node, ConversationNode<INodeGui> conversationInfo, Func<Id<LocalizedText>, string> localize)
        {
            return new CsvData()
            {
                Character = node.Data.Parameters.Single(p => p.Id == CsvData.SPEECH_CHARACTER).DisplayValue(localize),
                Script = node.Data.Parameters.Single(p => p.Id == CsvData.SPEECH_SCRIPT).DisplayValue(localize),
                Subtitle = node.Data.Parameters.Single(p => p.Id == CsvData.SPEECH_SUBTITLE).DisplayValue(localize),
                Direction = node.Data.Parameters.Single(p => p.Id == CsvData.SPEECH_DIRECTION).DisplayValue(localize),
                Language = node.Data.Parameters.Single(p => p.Id == CsvData.SPEECH_LANGUAGE).DisplayValue(localize),
                Context = conversationInfo != null ? conversationInfo.Data.Parameters.Single(p => p.Id == CsvData.CONVERSATIONINFO_CONTEXT).DisplayValue(localize) : "",
                Notes = conversationInfo != null ? conversationInfo.Data.Parameters.Single(p => p.Id == CsvData.CONVERSATIONINFO_NOTES).DisplayValue(localize) : "",
                Audio = node.Data.Parameters.Single(p => p.Id == CsvData.SPEECH_AUDIO).ValueAsString(),
            };
        }

        public static CsvData GetPlayerSpeechData(ConversationNode<INodeGui> node, ConversationNode<INodeGui> conversationInfo, Func<Id<LocalizedText>, string> localize)
        {
            return new CsvData()
            {
                Character = "Player",
                Script = node.Data.Parameters.Single(p => p.Id == CsvData.SPEECH_SCRIPT).DisplayValue(localize),
                Subtitle = node.Data.Parameters.Single(p => p.Id == CsvData.SPEECH_SUBTITLE).DisplayValue(localize),
                Direction = node.Data.Parameters.Single(p => p.Id == CsvData.SPEECH_DIRECTION).DisplayValue(localize),
                Language = node.Data.Parameters.Single(p => p.Id == CsvData.SPEECH_LANGUAGE).DisplayValue(localize),
                Context = conversationInfo != null ? conversationInfo.Data.Parameters.Single(p => p.Id == CsvData.CONVERSATIONINFO_CONTEXT).DisplayValue(localize) : "",
                Notes = conversationInfo != null ? conversationInfo.Data.Parameters.Single(p => p.Id == CsvData.CONVERSATIONINFO_NOTES).DisplayValue(localize) : "",
                Audio = node.Data.Parameters.Single(p => p.Id == CsvData.SPEECH_AUDIO).ValueAsString(),
            };
        }

        public static void WriteTitle(string separator, StreamWriter sw, bool includeName)
        {
            var data = string.Join(separator, new[] { "Character", "Script", "Subtitle", "Direction", "VO Language", "Context", "Notes" });
            if (includeName)
                data = "File;" + data;
            sw.WriteLine(data);
        }

        public void Write(string separator, string name, StreamWriter sw)
        {
            var data = string.Join(separator, new[] { Character, Script, Subtitle, Direction, Language, Context, Notes, Audio });
            if (name != null)
                data = name + separator + data;
            sw.WriteLine(data);
        }
    }

    public abstract class ExportAsSeparatedStrings : IConversationContextMenuItem
    {
        private Func<Id<LocalizedText>, string> m_localize;

        protected ExportAsSeparatedStrings(Func<Id<LocalizedText>, string> localize)
        {
            m_localize = localize;
        }

        public abstract string Name { get; }

        public abstract string Separator { get; }

        public void Execute(IConversationFile conversation, IErrorCheckerUtilities<IConversationNode> util)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.DefaultExt = "ssv";
                sfd.AddExtension = true;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    Stream stream = null;
                    try
                    {
                        stream = sfd.OpenFile();
                        using (StreamWriter sw = new StreamWriter(stream))
                        {
                            stream = null;
                            CsvData.WriteTitle(Separator, sw, false);
                            WriteConversation(conversation, sw, false, util);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Makes sense that WriteConversation takes an IConversation")]
        public void WriteConversation(IConversationFile conversation, StreamWriter sw, bool includeName, IErrorCheckerUtilities<IConversationNode> util)
        {
            if (conversation == null)
                throw new ArgumentNullException(nameof(conversation));

            var nodes = conversation.Nodes;

            Stack<IConversationNodeData> startNodes = new Stack<IConversationNodeData>(nodes.Where(n => Viking.Util.IsStartNode(n.Data.NodeTypeId, util)).Select(n => n.Data));

            var conversationInfo = conversation.Nodes.Where(n => n.Data.NodeTypeId == CsvData.CONVERSATIONINFO).FirstOrDefault();
            var playerSpeechNodes = conversation.Nodes.Where(n => n.Data.NodeTypeId == CsvData.PLAYER_SPEECH).Select(n => new { Key = n, Value = CsvData.GetPlayerSpeechData(n, conversationInfo, m_localize) });
            var npcSpeechNodes = conversation.Nodes.Where(n => n.Data.NodeTypeId == CsvData.NPC_SPEECH).Select(n => new { Key = n, Value = CsvData.GetSpeechData(n, conversationInfo, m_localize) });
            var radioSpeechNodes = conversation.Nodes.Where(n => n.Data.NodeTypeId == CsvData.RADIO_SPEECH).Select(n => new { Key = n, Value = CsvData.GetSpeechData(n, conversationInfo, m_localize) });
            var optionNodes = conversation.Nodes.Where(n => n.Data.NodeTypeId == CsvData.OPTION).Select(n => new { Key = n, Value = CsvData.GetOptionData(n, conversationInfo, m_localize) });

            var allcontent = playerSpeechNodes.Concat(npcSpeechNodes).Concat(radioSpeechNodes).Concat(optionNodes).ToDictionary(kvp => kvp.Key.Data, kvp => kvp.Value);
            HashSet<IConversationNodeData> processed = new HashSet<IConversationNodeData>();

            //Depth first iterate through the conversation starting at start nodes
            while (startNodes.Any())
            {
                var start = startNodes.Pop();

                if (!processed.Contains(start))
                {
                    foreach (var output in start.Connectors.Where(c => c.Definition.Id == SpecialConnectors.Output.Id))
                    {
                        foreach (var connection in output.Connections)
                        {
                            var connected = connection.Parent;
                            startNodes.Push(connected);
                        }
                    }

                    if (allcontent.ContainsKey(start))
                    {
                        var row = allcontent[start];
                        row.Write(Separator, includeName ? conversation.File.File.Name : null, sw);
                        allcontent.Remove(start);
                    }
                }

                processed.Add(start);
            }

            //Pick up any leftover nodes that are not connected to a start node
            foreach (var node in nodes.Select(n => n.Data))
            {
                if (!processed.Contains(node))
                {
                    if (allcontent.ContainsKey(node))
                    {
                        var row = allcontent[node];
                        row.Write(Separator, includeName ? conversation.File.File.Name : null, sw);
                        allcontent.Remove(node);
                    }
                    processed.Add(node);
                }
            }
        }
    }

    public class ExportAsCsv : ExportAsSeparatedStrings
    {
        public ExportAsCsv(Func<Id<LocalizedText>, string> localize)
            : base(localize)
        {
        }

        public override string Name { get { return "Export As Csv"; } }

        public override string Separator { get { return ","; } }
    }

    public class ExportAsSsv : ExportAsSeparatedStrings
    {
        public ExportAsSsv(Func<Id<LocalizedText>, string> localize)
            : base(localize)
        {
        }

        public override string Name { get { return "Export As ssv"; } }

        public override string Separator { get { return ";"; } }
    }
}
