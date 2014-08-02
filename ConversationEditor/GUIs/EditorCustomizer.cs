using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Conversation;
using System.Reflection;

namespace ConversationEditor
{
    public partial class EditorCustomizer : Form
    {
        public static IEnumerable<ParameterEditorChoice> DefaultEditors
        {
            get
            {
                yield return new ParameterEditorChoice(typeof(DefaultBooleanEditor));
                yield return new ParameterEditorChoice(typeof(DefaultDecimalEditor));
                yield return new ParameterEditorChoice(typeof(DefaultDynamicEnumEditor));
                yield return new ParameterEditorChoice(typeof(DefaultEnumEditor));
                yield return new ParameterEditorChoice(typeof(DefaultIntegerEditor));
                //yield return new ParameterEditorChoice(typeof(DefaultFilePathEditor));
                yield return new ParameterEditorChoice(typeof(DefaultStringEditor));
                yield return new ParameterEditorChoice(typeof(DefaultLocalizedStringEditor));
                yield return new ParameterEditorChoice(typeof(DefaultAudioEditor));
            }
        }

        private IDataSource m_datasource;
        private TreeNode m_defaults;

        public EditorCustomizer()
        {
            InitializeComponent();

            m_defaults = new TreeNode("Defaults") { Tag = Assembly.GetExecutingAssembly() };
            treeView1.Nodes.Add(m_defaults);

            foreach (var editor in DefaultEditors)
                AddTreeNode(editor, m_defaults);

            m_defaults.Expand();
        }

        private void AddTreeNode(ParameterEditorChoice editor, TreeNode parent)
        {
            var node = new TreeNode(editor.ToString()) { Tag = editor, NodeFont = DefaultFont };
            m_nodes[editor] = node;
            parent.Nodes.Add(node);
        }

        private void AddNode(ParameterEditorChoice editor)
        {
            var parent = treeView1.Nodes.OfType<TreeNode>().FirstOrDefault(n => (Assembly)n.Tag == editor.m_type.Assembly);
            if (parent == null)
            {
                parent = new TreeNode(editor.m_type.Assembly.GetName().Name) { Tag = editor.m_type.Assembly };
                treeView1.Nodes.Add(parent);
            }

            if (!parent.Nodes.OfType<TreeNode>().Any(n => n.Tag.Equals(editor)))
            {
                AddTreeNode(editor, parent);
            }
        }

        public void Init(TypeMapConfig<ID<ParameterType>, ParameterEditorChoice> config, IDataSource dataSource)
        {
            m_config = config;
            m_datasource = dataSource;
            dataGridView1.Rows.Clear();
            foreach (var kvp in m_datasource.ParameterTypes)
            {
                var typeid = kvp;
                if (!m_config.ContainsKey(typeid))
                    m_config[typeid] = DefaultEditor(typeid, WillEdit.Create(m_datasource));
                dataGridView1.Rows.Add(typeid, m_config[typeid].ToString());
                AddNode(m_config[typeid]);
            }

            prepSelection(0);
        }

        Dictionary<ParameterEditorChoice, TreeNode> m_nodes = new Dictionary<ParameterEditorChoice, TreeNode>();
        TypeMapConfig<ID<ParameterType>, ParameterEditorChoice> m_config;

        ID<ParameterType> m_selectedType;
        public static ParameterEditorChoice DefaultEditor(ID<ParameterType> type, WillEdit willEdit)
        {
            return DefaultEditors.FirstOrDefault(pec => pec.WillEdit(type, willEdit));
        }

        private void prepSelection(int rowIndex)
        {
            m_selectedType = ID<ParameterType>.Parse(dataGridView1.Rows[rowIndex].Cells[0].Value.ToString());
            treeView1.Select();
            foreach (var kvp in m_nodes)
            {
                if (kvp.Key.WillEdit(m_selectedType, WillEdit.Create(m_datasource)))
                    kvp.Value.NodeFont = DefaultFont;
                else
                    kvp.Value.NodeFont = new Font(DefaultFont, FontStyle.Strikeout);
            }
            treeView1.SelectedNode = m_nodes[m_config[m_selectedType]];
            treeView1.SelectedNode.EnsureVisible();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            prepSelection(e.RowIndex);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Level == 0)
                e.Cancel = true;
            else if (e.Node.Tag == null)
                throw new Exception(); //No associated editor
            else if (e.Node.NodeFont.Strikeout)
                e.Cancel = true;
        }

        private void LoadAssembly(string name)
        {
            var assembly = Assembly.LoadFile(name);
            var editors = assembly.GetExportedTypes().Where(t => t.GetInterfaces().Contains(typeof(IParameterEditor<Control>)));
            TreeNode node = treeView1.Nodes.Add(assembly.FullName);
            foreach (var ed in editors)
                AddTreeNode(new ParameterEditorChoice(ed), node);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string file in openFileDialog1.FileNames)
                    LoadAssembly(file);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var editor = e.Node.Tag as ParameterEditorChoice;
                m_config[m_selectedType] = editor;
                dataGridView1.SelectedRows[0].Cells[1].Value = editor;
            }
        }
    }
}
