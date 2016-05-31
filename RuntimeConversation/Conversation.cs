using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Conversation;
using Conversation.Serialization;

namespace RuntimeConversation
{
    public partial class ConversationForm : Form
    {
        private Dictionary<Id<LocalizedText>, string> m_localizer;
        public string Localize(Id<LocalizedText> id)
        {
            if (m_localizer.ContainsKey(id))
                return m_localizer[id];
            return id.Serialized();
        }

        public string CharacterName(Viking.Types.Character character)
        {
            return Enum.GetName(typeof(Viking.Types.Character), character);
        }

        public string Personality(Viking.Types.Personality_Trait trait)
        {
            return Enum.GetName(typeof(Viking.Types.Personality_Trait), trait);
        }

        public ConversationForm(string conversation, string localization)
        {
            InitializeComponent();

            var d = new Viking.Deserializer();
            Conversation c;
            using (var stream = new FileStream(conversation, FileMode.Open))
            {
                c = d.Read(stream);
            }

            using (FileStream stream = new FileStream(localization, FileMode.Open, FileAccess.Read))
            {
                var asd = new XmlLocalization.ClientDeserializer();
                m_localizer = asd.Read(stream);
            }

            var start = c.Nodes.OfType<Viking.Nodes.Start>().Single();
            VikingProcessor processor = new VikingProcessor(Localize, CharacterName);


            processor.Approve += p =>
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Characters with " + string.Join(" or ", p.Select(Personality)) + " personality agree with this decision");
                Console.ForegroundColor = color;
            };
            processor.Disapprove += p =>
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Characters with " + string.Join(" or ", p.Select(Personality)) + " personality do not agree with this decision");
                Console.ForegroundColor = color;
            };
            processor.EnteringDialog += () => { Console.WriteLine("ENTERING DIALOGUE"); Console.ReadKey(); };
            processor.ExitingDialog += () => { Console.WriteLine("EXITING DIALOGUE"); Console.ReadKey(); };
            processor.PlaySpeech += (speaker, listener, speech) => { Console.WriteLine(speaker + " says to " + listener + " : " + speech); Console.ReadKey(); };

            Viking.Nodes.Node node = start;

            do
            {
                var next = node.Process(processor).ToArray();
                if (next.Count() > 1)
                {
                    Form f = new Form();
                    var ff = new FlowLayoutPanel();
                    ff.Dock = DockStyle.Fill;
                    f.Controls.Add(ff);
                    foreach (var option in next)
                    {
                        var o = option;
                        var b = new Button();
                        b.Text = option.Text;
                        b.Click += (x, y) => { f.Close(); node = o.Node; };
                        b.AutoSize = true;
                        b.Height = 100;
                        ff.Controls.Add(b);
                    }
                    f.ShowDialog();
                    //node = next.First().Node;
                }
                else if (next.Count() > 0)
                {
                    node = next.First().Node;
                }
                else
                {
                    node = null;
                }
            } while (node != null);
        }
    }
}
