using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using Conversation;
using System.Windows.Forms;
using System.IO;

namespace PluginPack
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

        public static ID<NodeTypeTemp> PLAYER_SPEECH = ID<NodeTypeTemp>.FromGuid(Guid.Parse("da1dfd60-28e4-48f6-93e6-298f3a68b67c"));
        public static ID<NodeTypeTemp> NPC_SPEECH = ID<NodeTypeTemp>.FromGuid(Guid.Parse("196dc521-8336-4714-93b7-77ac09b3abd7"));
        public static ID<NodeTypeTemp> RADIO_SPEECH = ID<NodeTypeTemp>.FromGuid(Guid.Parse("836eeb03-2730-4830-b192-3fc9ce41503e"));
        public static ID<Parameter> SPEECH_CHARACTER = ID<Parameter>.FromGuid(Guid.Parse("af08c7f7-33e9-4429-9e1f-cd786a73041b"));
        public static ID<Parameter> SPEECH_SCRIPT = ID<Parameter>.FromGuid(Guid.Parse("0154fe0e-5f53-4657-acae-b983ea9030a0"));
        public static ID<Parameter> SPEECH_SUBTITLE = ID<Parameter>.FromGuid(Guid.Parse("8987655a-92fe-4eca-8c50-8769a7edcf04"));
        public static ID<Parameter> SPEECH_DIRECTION = ID<Parameter>.FromGuid(Guid.Parse("98401421-e713-4014-8489-b8675c566179"));
        public static ID<Parameter> SPEECH_AUDIO = ID<Parameter>.FromGuid(Guid.Parse("d081e3ec-91f2-4ec1-ab20-a4a1b01162b7"));
        public static ID<Parameter> SPEECH_LANGUAGE = ID<Parameter>.FromGuid(Guid.Parse("803d384c-edc1-45e3-9bfe-88f99384d86d"));

        public static ID<NodeTypeTemp> OPTION = ID<NodeTypeTemp>.FromGuid(Guid.Parse("86524441-8da7-4e19-9ff3-c8df67e09f8f"));
        public static ID<Parameter> OPTION_SCRIPT = ID<Parameter>.FromGuid(Guid.Parse("e8b15360-c500-434e-856c-bd1090c1b4a2"));
        public static ID<Parameter> OPTION_SUBTITLES = ID<Parameter>.FromGuid(Guid.Parse("b9950a3d-14d9-46d0-94dd-4217ed1573ad"));
        public static ID<Parameter> OPTION_DIRECTION = ID<Parameter>.FromGuid(Guid.Parse("a4f77304-57b1-4e25-849a-3e199f0f1795"));
        public static ID<Parameter> OPTION_AUDIO = ID<Parameter>.FromGuid(Guid.Parse("266217e5-8504-4576-a5c8-428c86c9b73a"));
        public static ID<Parameter> OPTION_LANGUAGE = ID<Parameter>.FromGuid(Guid.Parse("ea771838-a6bb-45d5-bf94-e29fde56e284"));

        public static ID<NodeTypeTemp> CONVERSATIONINFO = ID<NodeTypeTemp>.FromGuid(Guid.Parse("d5974ffe-777b-419c-b9bc-bde980cb99a6"));
        public static ID<Parameter> CONVERSATIONINFO_CONTEXT = ID<Parameter>.FromGuid(Guid.Parse("6940a618-5905-4e81-a59b-281d92a90782"));
        public static ID<Parameter> CONVERSATIONINFO_NOTES = ID<Parameter>.FromGuid(Guid.Parse("cb4a4ac9-a5e9-444f-a7b0-b8f15e31e77a"));

        public static CsvData GetOptionData(ConversationNode<INodeGUI> node, ConversationNode<INodeGUI> conversationInfo, Func<ID<LocalizedText>, string> localize)
        {
            return new CsvData()
            {
                Character = "Player",
                Script = node.Parameters.Single(p => p.Id == CsvData.OPTION_SCRIPT).DisplayValue(localize),
                Subtitle = node.Parameters.Single(p => p.Id == CsvData.OPTION_SUBTITLES).DisplayValue(localize),
                Direction = node.Parameters.Single(p => p.Id == CsvData.OPTION_DIRECTION).DisplayValue(localize),
                Language = node.Parameters.Single(p => p.Id == CsvData.OPTION_LANGUAGE).DisplayValue(localize),
                Context = conversationInfo != null ? conversationInfo.Parameters.Single(p => p.Id == CsvData.CONVERSATIONINFO_CONTEXT).DisplayValue(localize) : "",
                Notes = conversationInfo != null ? conversationInfo.Parameters.Single(p => p.Id == CsvData.CONVERSATIONINFO_NOTES).DisplayValue(localize) : "",
                Audio = node.Parameters.Single(p => p.Id == CsvData.OPTION_AUDIO).DisplayValue(localize),
            };
        }

        public static CsvData GetSpeechData(ConversationNode<INodeGUI> node, ConversationNode<INodeGUI> conversationInfo, Func<ID<LocalizedText>, string> localize)
        {
            return new CsvData()
            {
                Character = node.Parameters.Single(p => p.Id == CsvData.SPEECH_CHARACTER).DisplayValue(localize),
                Script = node.Parameters.Single(p => p.Id == CsvData.SPEECH_SCRIPT).DisplayValue(localize),
                Subtitle = node.Parameters.Single(p => p.Id == CsvData.SPEECH_SUBTITLE).DisplayValue(localize),
                Direction = node.Parameters.Single(p => p.Id == CsvData.SPEECH_DIRECTION).DisplayValue(localize),
                Language = node.Parameters.Single(p => p.Id == CsvData.SPEECH_LANGUAGE).DisplayValue(localize),
                Context = conversationInfo != null ? conversationInfo.Parameters.Single(p => p.Id == CsvData.CONVERSATIONINFO_CONTEXT).DisplayValue(localize) : "",
                Notes = conversationInfo != null ? conversationInfo.Parameters.Single(p => p.Id == CsvData.CONVERSATIONINFO_NOTES).DisplayValue(localize) : "",
                Audio = node.Parameters.Single(p => p.Id == CsvData.SPEECH_AUDIO).DisplayValue(localize),
            };
        }

        public static CsvData GetPlayerSpeechData(ConversationNode<INodeGUI> node, ConversationNode<INodeGUI> conversationInfo, Func<ID<LocalizedText>, string> localize)
        {
            return new CsvData()
            {
                Character = "Player",
                Script = node.Parameters.Single(p => p.Id == CsvData.SPEECH_SCRIPT).DisplayValue(localize),
                Subtitle = node.Parameters.Single(p => p.Id == CsvData.SPEECH_SUBTITLE).DisplayValue(localize),
                Direction = node.Parameters.Single(p => p.Id == CsvData.SPEECH_DIRECTION).DisplayValue(localize),
                Language = node.Parameters.Single(p => p.Id == CsvData.SPEECH_LANGUAGE).DisplayValue(localize),
                Context = conversationInfo != null ? conversationInfo.Parameters.Single(p => p.Id == CsvData.CONVERSATIONINFO_CONTEXT).DisplayValue(localize) : "",
                Notes = conversationInfo != null ? conversationInfo.Parameters.Single(p => p.Id == CsvData.CONVERSATIONINFO_NOTES).DisplayValue(localize) : "",
                Audio = node.Parameters.Single(p => p.Id == CsvData.SPEECH_AUDIO).DisplayValue(localize),
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
        private Func<ID<LocalizedText>, string> m_localize;

        public ExportAsSeparatedStrings(Func<ID<LocalizedText>, string> localize)
        {
            m_localize = localize;
        }

        public abstract string Name { get; }

        public abstract string Separator { get; }

        public void Execute(IConversationFile conversation, IErrorCheckerUtilities util)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.DefaultExt = "ssv";
                sfd.AddExtension = true;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (var stream = sfd.OpenFile())
                    {
                        using (StreamWriter sw = new StreamWriter(stream))
                        {
                            CsvData.WriteTitle(Separator, sw, false);
                            WriteConversation(conversation, sw, false, util);
                        }
                    }
                }
            }
        }

        public void WriteConversation(IConversationFile conversation, StreamWriter sw, bool includeName, IErrorCheckerUtilities util)
        {
            var nodes = conversation.Nodes;

            Stack<IEditable> startNodes = new Stack<IEditable>(nodes.Where(n => Clandestine.Util.IsStartNode(n.Type, util)).Select(n => n.m_data));

            var conversationInfo = conversation.Nodes.Where(n => n.Type == CsvData.CONVERSATIONINFO).FirstOrDefault();
            var playerSpeechNodes = conversation.Nodes.Where(n => n.Type == CsvData.PLAYER_SPEECH).Select(n => new { Key = n, Value = CsvData.GetPlayerSpeechData(n, conversationInfo, m_localize) });
            var npcSpeechNodes = conversation.Nodes.Where(n => n.Type == CsvData.NPC_SPEECH).Select(n => new { Key = n, Value = CsvData.GetSpeechData(n, conversationInfo, m_localize) });
            var radioSpeechNodes = conversation.Nodes.Where(n => n.Type == CsvData.RADIO_SPEECH).Select(n => new { Key = n, Value = CsvData.GetSpeechData(n, conversationInfo, m_localize) });
            var optionNodes = conversation.Nodes.Where(n => n.Type == CsvData.OPTION).Select(n => new { Key = n, Value = CsvData.GetOptionData(n, conversationInfo, m_localize) });

            var allcontent = playerSpeechNodes.Concat(npcSpeechNodes).Concat(radioSpeechNodes).Concat(optionNodes).ToDictionary(kvp => kvp.Key.m_data, kvp => kvp.Value);
            HashSet<IEditable> processed = new HashSet<IEditable>();

            while (startNodes.Any())
            {
                var start = startNodes.Pop();

                if (!processed.Contains(start))
                {
                    foreach (var output in start.Connectors.Where(c => c.m_definition.Id == SpecialConnectors.Output.Id))
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
        }
    }

    public class ExportAsCsv : ExportAsSeparatedStrings
    {
        public ExportAsCsv(Func<ID<LocalizedText>, string> localize)
            : base(localize)
        {
        }

        public override string Name { get { return "Export As Csv"; } }

        public override string Separator { get { return ","; } }
    }

    public class ExportAsSsv : ExportAsSeparatedStrings
    {
        public ExportAsSsv(Func<ID<LocalizedText>, string> localize)
            : base(localize)
        {
        }

        public override string Name { get { return "Export As ssv"; } }

        public override string Separator { get { return ";"; } }
    }
}
