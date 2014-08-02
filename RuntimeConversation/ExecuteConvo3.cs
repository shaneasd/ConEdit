using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using MyNamespace;
using System.IO;
using MyNamespace.Nodes;
using System.Speech.Synthesis;
using Utilities;
using System.Threading;
using Conversation;
using Conversation.Serialization;

namespace RuntimeConversation
{
    public partial class Form1 : Form, IProcessor<Node>
    {
        private Button button1;
        private GroupBox conversation;
        private GroupBox localizer;
        private TextBox textBox1;

        public Form1()
        {
            InitializeComponent();
            conversation.AllowDrop = true;
            conversation.DragEnter += new DragEventHandler(Form1_DragEnter);
            conversation.DragDrop += new DragEventHandler(Form1_DragDrop);
            localizer.AllowDrop = true;
            textBox1.Text = "";
        }

        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        Conversation m_convo;
        Dictionary<ID<LocalizedText>, string> m_localizer;

        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                m_convo = Read(file);
            }
        }

        private static Conversation Read(string file)
        {
            using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                Deserializer d = new Deserializer();
                return d.Read(stream);
            }
        }

        private Node Process(Node node)
        {
            return node.Process(this);
        }

        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.conversation = new System.Windows.Forms.GroupBox();
            this.localizer = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(332, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(70, 92);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // conversation
            // 
            this.conversation.Location = new System.Drawing.Point(12, 12);
            this.conversation.Name = "conversation";
            this.conversation.Size = new System.Drawing.Size(141, 92);
            this.conversation.TabIndex = 2;
            this.conversation.TabStop = false;
            this.conversation.Text = "conversation";
            // 
            // localizer
            // 
            this.localizer.Location = new System.Drawing.Point(159, 12);
            this.localizer.Name = "localizer";
            this.localizer.Size = new System.Drawing.Size(167, 92);
            this.localizer.TabIndex = 3;
            this.localizer.TabStop = false;
            this.localizer.Text = "localizer";
            this.localizer.DragDrop += new System.Windows.Forms.DragEventHandler(this.localizer_DragDrop);
            this.localizer.DragEnter += new System.Windows.Forms.DragEventHandler(this.localizer_DragEnter);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 110);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(390, 238);
            this.textBox1.TabIndex = 4;
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(411, 353);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.localizer);
            this.Controls.Add(this.conversation);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void localizer_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void localizer_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                var d = new XmlLocalization.ClientDeserializer();
                using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    m_localizer = d.Read(stream);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(a =>
            {
                Node next = m_convo.Nodes.OfType<Start>().SingleOrDefault();
                while (next != null)
                {
                    next = Process(next);
                }
            });
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Curious node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Bribed_Caught node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Bribed_Spotted node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Idle node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Futz node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Noise node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Attacking node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Enemy_Lost node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Reloading node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Weapon node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Crime node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Damage node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Carcass node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Disrepair node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Arresting node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Reinforcements node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.AI_Barks.AI_Grenade_Throw node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Animation.Trigger_Gesture node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Animation.Set_Emotion node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Animation.Set_Attitude node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Camera.Change_Camera node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Camera.Reset_Camera node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(Character_Info node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Condition.Is_Alive node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Condition.Character_Nearby node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Condition.Player_Character node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Condition.Player_Outfit node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Condition.Check_Integer node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Condition.Player_Inventory node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Condition.Player_Health node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Dev.TODO node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Dev.Error node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Dev.Description node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(Gun_Drawn node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Jumps.Jump_To node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Jumps.Jump_Target node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Metadata.Conversation_Info node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(Prompt_Option node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(Option node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Randomise.Random node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Randomise.Probability node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(NPC_Speech node)
        {
            var speaker = node.Speaker;
            var script = node.Script;
            string subtitle = m_localizer[ID<LocalizedText>.FromGuid(node.Subtitles.Id)];

            this.Invoke(new Action(() => { textBox1.Text += speaker + ": " + subtitle + "\n"; }));

            SpeechSynthesizer reader = new SpeechSynthesizer();
            var voices = reader.GetInstalledVoices().Evaluate();
            reader.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Adult, (int)speaker);
            reader.Speak(subtitle);

            return node.id179fd9edc5654fb2bf3ebc562c27c940.Connections.Single().Parent;
        }

        public Node ProcessNode(Player_Speech node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(Radio_Message node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(Start_Radio_Message node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(Start node)
        {
            return node.idb5b1fe0305e14058aecb4012ae91db1f.Connections.Single().Parent;
        }

        public Node ProcessNode(Terminator node)
        {
            return null;
        }

        public Node ProcessNode(MyNamespace.Nodes.Trigger.Begin_Patrol node)
        {
            MessageBox.Show(node.Character + " beginning patrol");
            return node.idd8522f4026fd45a9831ea61472cda4c3.Connections.Single().Parent;
        }

        public Node ProcessNode(MyNamespace.Nodes.Trigger.Console_Message node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Trigger.Prompt node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Trigger.Set_Local_Integer node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Trigger.Set_Local_Decimal node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Trigger.Set_Boolean node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Trigger.Set_Integer node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Trigger.Increment_Integer node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Trigger.Give_Item node)
        {
            throw new NotImplementedException();
        }

        public Node ProcessNode(MyNamespace.Nodes.Trigger.Change_Bindname node)
        {
            throw new NotImplementedException();
        }
    }
}
