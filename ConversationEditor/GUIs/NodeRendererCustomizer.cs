using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Conversation;
using Utilities;
using Utilities.UI;

namespace ConversationEditor
{
    internal class NodeRendererCustomizer : NodeCustomizer<NodeRendererChoice>
    {
        public NodeRendererCustomizer(IDataSource datasource, TypeMapConfig<Id<NodeTypeTemp>, NodeRendererChoice> config, PluginsConfig pluginsConfig)
            : base(datasource, config, pluginsConfig)
        {
        }

        protected override NodeRendererChoice Default(Id<NodeTypeTemp> guid)
        {
            return NodeRendererChoice.DefaultConversation(guid);
        }

        protected override IEnumerable<NodeRendererChoice> GetItemsFor(Id<NodeTypeTemp> guid)
        {
            return m_pluginsConfig.GetRenderersFor(guid);
        }
    }

    internal class NodeEditorCustomizer : NodeCustomizer<NodeEditorChoice>
    {
        public NodeEditorCustomizer(IDataSource datasource, TypeMapConfig<Id<NodeTypeTemp>, NodeEditorChoice> config, PluginsConfig pluginsConfig)
            : base(datasource, config, pluginsConfig)
        {
        }

        protected override NodeEditorChoice Default(Id<NodeTypeTemp> guid)
        {
            return NodeEditorChoice.Default(guid);
        }

        protected override IEnumerable<NodeEditorChoice> GetItemsFor(Id<NodeTypeTemp> guid)
        {
            return m_pluginsConfig.GetEditorsFor(guid);
        }
    }

    internal abstract partial class NodeCustomizer<TChoice> : Form
        where TChoice : TypeChoice
    {
        protected NodeCustomizer()
        {
            InitializeComponent();
        }

        IDataSource m_datasource;
        TypeMapConfig<Id<NodeTypeTemp>, TChoice> m_config;
        protected PluginsConfig m_pluginsConfig;

        protected NodeCustomizer(IDataSource datasource, TypeMapConfig<Id<NodeTypeTemp>, TChoice> config, PluginsConfig pluginsConfig)
            : this()
        {
            m_datasource = datasource;
            m_config = config;
            m_pluginsConfig = pluginsConfig;
        }

        void AddNodeType(INodeType nodeType, int indent)
        {
            foreach (var n in nodeType.Nodes)
            {
                AddNode(n, indent);
            }
            foreach (INodeType t in nodeType.ChildTypes)
                AddNodeType(t, indent + 1);
        }

        List<Action> m_save = new List<Action>();

        protected abstract IEnumerable<TChoice> GetItemsFor(Id<NodeTypeTemp> guid);

        void AddNode(INodeDataGenerator node, int indent)
        {
            //TODO: It would be a lot better if node renderer customization used a traditional combobox rather than a suggestion box

            Panel panel = new Panel();
            panel.Height = 30;
            panel.Dock = DockStyle.Top;

            const int BUFFER_WIDTH = 30;
            ComboBox c = new ComboBox();
            c.Dock = DockStyle.Right;
            c.Width = 200;

            foreach (TChoice renderChoice in GetItemsFor(node.Guid))
            {
                c.Items.Add(ToStringWrapper.Make(renderChoice, renderChoice.DisplayName));
            }
            var item = ToStringWrapper.Make(m_config[node.Guid], m_config[node.Guid].DisplayName);
            c.SelectedItem = item;
            if (c.Items.Count < 2)
                c.ForeColor = SystemColors.GrayText;

            panel.Controls.Add(c);
            panel.Controls.Add(new Label() { Text = node.Name, Dock = DockStyle.Left });
            panel.Controls.Add(new Panel() { Width = BUFFER_WIDTH * indent, Dock = DockStyle.Left });

            panel1.Controls.Add(panel);

            panel1.ScrollControlIntoView(panel);

            m_save.Add(() => { m_config[node.Guid] = (c.SelectedItem as ToStringWrapper<TChoice>).Value; });
        }

        protected abstract TChoice Default(Id<NodeTypeTemp> guid);

        private void NodeRendererCustomizer_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();

            IEnumerable<INodeDataGenerator> nodeGenerators = Collection.Collapse(m_datasource.Nodes, t => t.ChildTypes, t => t.Nodes);

            foreach (INodeDataGenerator nodeType in nodeGenerators)
            {
                if (!m_config.ContainsKey(nodeType.Guid))
                    m_config[nodeType.Guid] = Default(nodeType.Guid);
            }

            AddNodeType(m_datasource.Nodes, 0);

            this.ResumeLayout();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (var save in m_save)
                save();
            Close();
        }
    }
}
