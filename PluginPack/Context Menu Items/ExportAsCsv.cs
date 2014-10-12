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
    public class ExportAsCsv : IConversationContextMenuItem
    {
        private Func<ID<LocalizedText>, string> m_localize;

        public ExportAsCsv(Func<ID<LocalizedText>, string> localize)
        {
            m_localize = localize;
        }

        public string Name
        {
            get { return "Export As Csv"; }
        }

        ID<NodeTypeTemp> PLAYER_SPEECH = ID<NodeTypeTemp>.FromGuid(Guid.Parse("da1dfd60-28e4-48f6-93e6-298f3a68b67c"));
        ID<NodeTypeTemp> NPC_SPEECH = ID<NodeTypeTemp>.FromGuid(Guid.Parse("196dc521-8336-4714-93b7-77ac09b3abd7"));
        ID<NodeTypeTemp> RADIO_SPEECH = ID<NodeTypeTemp>.FromGuid(Guid.Parse("836eeb03-2730-4830-b192-3fc9ce41503e"));
        ID<Parameter> SPEECH_CHARACTER = ID<Parameter>.FromGuid(Guid.Parse("af08c7f7-33e9-4429-9e1f-cd786a73041b"));
        ID<Parameter> SPEECH_SCRIPT = ID<Parameter>.FromGuid(Guid.Parse("0154fe0e-5f53-4657-acae-b983ea9030a0"));
        ID<Parameter> SPEECH_SUBTITLE = ID<Parameter>.FromGuid(Guid.Parse("8987655a-92fe-4eca-8c50-8769a7edcf04"));
        ID<Parameter> SPEECH_DIRECTION = ID<Parameter>.FromGuid(Guid.Parse("98401421-e713-4014-8489-b8675c566179"));

        ID<NodeTypeTemp> OPTION = ID<NodeTypeTemp>.FromGuid(Guid.Parse("86524441-8da7-4e19-9ff3-c8df67e09f8f"));
        ID<Parameter> OPTION_SCRIPT = ID<Parameter>.FromGuid(Guid.Parse("e8b15360-c500-434e-856c-bd1090c1b4a2"));
        ID<Parameter> OPTION_SUBTITLES = ID<Parameter>.FromGuid(Guid.Parse("b9950a3d-14d9-46d0-94dd-4217ed1573ad"));
        ID<Parameter> OPTION_DIRECTION = ID<Parameter>.FromGuid(Guid.Parse("a4f77304-57b1-4e25-849a-3e199f0f1795"));

        CsvData GetSpeechData(ConversationNode<INodeGUI> node)
        {
            return new CsvData()
            {
                Character = node.Parameters.Single(p => p.Id == SPEECH_CHARACTER).DisplayValue(m_localize),
                Script = node.Parameters.Single(p => p.Id == SPEECH_SCRIPT).DisplayValue(m_localize),
                Subtitle = node.Parameters.Single(p => p.Id == SPEECH_SUBTITLE).DisplayValue(m_localize),
                Direction = node.Parameters.Single(p => p.Id == SPEECH_DIRECTION).DisplayValue(m_localize),
            };
        }

        CsvData GetOptionData(ConversationNode<INodeGUI> node)
        {
            return new CsvData()
            {
                Character = "",
                Script = node.Parameters.Single(p => p.Id == OPTION_SCRIPT).DisplayValue(m_localize),
                Subtitle = node.Parameters.Single(p => p.Id == OPTION_SUBTITLES).DisplayValue(m_localize),
                Direction = node.Parameters.Single(p => p.Id == OPTION_DIRECTION).DisplayValue(m_localize),
            };
        }

        struct CsvData
        {
            public string Character;
            public string Script;
            public string Subtitle;
            public string Direction;
        }

        public void Execute(IConversationFile conversation)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.DefaultExt = "csv";
                sfd.AddExtension = true;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var nodes = conversation.Nodes;

                    var playerSpeechNodes = conversation.Nodes.Where(n => n.Type == PLAYER_SPEECH);
                    var npcSpeechNodes = conversation.Nodes.Where(n => n.Type == NPC_SPEECH);
                    var radioSpeechNodes = conversation.Nodes.Where(n => n.Type == RADIO_SPEECH);
                    var optionNodes = conversation.Nodes.Where(n => n.Type == OPTION);

                    IEnumerable<CsvData> rows = playerSpeechNodes.Select(GetSpeechData)
                                                .Concat(npcSpeechNodes.Select(GetSpeechData))
                                                .Concat(radioSpeechNodes.Select(GetSpeechData))
                                                .Concat(optionNodes.Select(GetOptionData));

                    using (var stream = sfd.OpenFile())
                    {
                        using (StreamWriter sw = new StreamWriter(stream))
                        {
                            sw.WriteLine("Character, Script, Subtitle, Direction");
                            foreach (CsvData row in rows)
                            {
                                sw.WriteLine(String.Format("{0}, {1}, {2}, {3}", row.Character, row.Script, row.Subtitle, row.Direction));
                            }
                        }
                    }
                }
            }
        }
    }

    public class ExportAsSsv : IConversationContextMenuItem
    {
        private Func<ID<LocalizedText>, string> m_localize;

        public ExportAsSsv(Func<ID<LocalizedText>, string> localize)
        {
            m_localize = localize;
        }

        public string Name
        {
            get { return "Export As ssv"; }
        }

        ID<NodeTypeTemp> PLAYER_SPEECH = ID<NodeTypeTemp>.FromGuid(Guid.Parse("da1dfd60-28e4-48f6-93e6-298f3a68b67c"));
        ID<NodeTypeTemp> NPC_SPEECH = ID<NodeTypeTemp>.FromGuid(Guid.Parse("196dc521-8336-4714-93b7-77ac09b3abd7"));
        ID<NodeTypeTemp> RADIO_SPEECH = ID<NodeTypeTemp>.FromGuid(Guid.Parse("836eeb03-2730-4830-b192-3fc9ce41503e"));
        ID<Parameter> SPEECH_CHARACTER = ID<Parameter>.FromGuid(Guid.Parse("af08c7f7-33e9-4429-9e1f-cd786a73041b"));
        ID<Parameter> SPEECH_SCRIPT = ID<Parameter>.FromGuid(Guid.Parse("0154fe0e-5f53-4657-acae-b983ea9030a0"));
        ID<Parameter> SPEECH_SUBTITLE = ID<Parameter>.FromGuid(Guid.Parse("8987655a-92fe-4eca-8c50-8769a7edcf04"));
        ID<Parameter> SPEECH_DIRECTION = ID<Parameter>.FromGuid(Guid.Parse("98401421-e713-4014-8489-b8675c566179"));

        ID<NodeTypeTemp> OPTION = ID<NodeTypeTemp>.FromGuid(Guid.Parse("86524441-8da7-4e19-9ff3-c8df67e09f8f"));
        ID<Parameter> OPTION_SCRIPT = ID<Parameter>.FromGuid(Guid.Parse("e8b15360-c500-434e-856c-bd1090c1b4a2"));
        ID<Parameter> OPTION_SUBTITLES = ID<Parameter>.FromGuid(Guid.Parse("b9950a3d-14d9-46d0-94dd-4217ed1573ad"));
        ID<Parameter> OPTION_DIRECTION = ID<Parameter>.FromGuid(Guid.Parse("a4f77304-57b1-4e25-849a-3e199f0f1795"));

        CsvData GetSpeechData(ConversationNode<INodeGUI> node)
        {
            return new CsvData()
            {
                Character = node.Parameters.Single(p => p.Id == SPEECH_CHARACTER).DisplayValue(m_localize),
                Script = node.Parameters.Single(p => p.Id == SPEECH_SCRIPT).DisplayValue(m_localize),
                Subtitle = node.Parameters.Single(p => p.Id == SPEECH_SUBTITLE).DisplayValue(m_localize),
                Direction = node.Parameters.Single(p => p.Id == SPEECH_DIRECTION).DisplayValue(m_localize),
            };
        }

        CsvData GetPlayerSpeechData(ConversationNode<INodeGUI> node)
        {
            return new CsvData()
            {
                Character = "Player",
                Script = node.Parameters.Single(p => p.Id == SPEECH_SCRIPT).DisplayValue(m_localize),
                Subtitle = node.Parameters.Single(p => p.Id == SPEECH_SUBTITLE).DisplayValue(m_localize),
                Direction = node.Parameters.Single(p => p.Id == SPEECH_DIRECTION).DisplayValue(m_localize),
            };
        }

        CsvData GetOptionData(ConversationNode<INodeGUI> node)
        {
            return new CsvData()
            {
                Character = "",
                Script = node.Parameters.Single(p => p.Id == OPTION_SCRIPT).DisplayValue(m_localize),
                Subtitle = node.Parameters.Single(p => p.Id == OPTION_SUBTITLES).DisplayValue(m_localize),
                Direction = node.Parameters.Single(p => p.Id == OPTION_DIRECTION).DisplayValue(m_localize),
            };
        }

        struct CsvData
        {
            public string Character;
            public string Script;
            public string Subtitle;
            public string Direction;
        }

        public void Execute(IConversationFile conversation)
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
                            WriteTitle(sw, false);
                            WriteConversation(conversation, sw, false);
                        }
                    }
                }
            }
        }

        public void WriteConversation(IConversationFile conversation, StreamWriter sw, bool includeName)
        {
            var nodes = conversation.Nodes;

            var playerSpeechNodes = conversation.Nodes.Where(n => n.Type == PLAYER_SPEECH);
            var npcSpeechNodes = conversation.Nodes.Where(n => n.Type == NPC_SPEECH);
            var radioSpeechNodes = conversation.Nodes.Where(n => n.Type == RADIO_SPEECH);
            var optionNodes = conversation.Nodes.Where(n => n.Type == OPTION);

            IEnumerable<CsvData> rows = playerSpeechNodes.Select(GetPlayerSpeechData)
                                        .Concat(npcSpeechNodes.Select(GetSpeechData))
                                        .Concat(radioSpeechNodes.Select(GetSpeechData))
                                        .Concat(optionNodes.Select(GetOptionData));

            foreach (CsvData row in rows)
            {
                var data = String.Format("{0}; {1}; {2}; {3}", row.Character, row.Script, row.Subtitle, row.Direction);
                if (includeName)
                    data = conversation.File.File.Name + "; " + data;
                sw.WriteLine(data);
            }
        }

        public void WriteTitle(StreamWriter sw, bool includeName)
        {
            var data = "Character; Script; Subtitle; Direction";
            if (includeName)
                data = "File; " + data;
            sw.WriteLine(data);
        }
    }
}
