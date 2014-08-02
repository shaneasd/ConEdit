using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utilities;
using Conversation;

namespace ConversationEditor
{
    using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI>;

    public partial class ErrorList2 : UserControl
    {
        public ErrorList2()
        {
            InitializeComponent();
        }

        public void SetErrors(IEnumerable<ConversationError<ConversationNode>> errors)
        {
            dataGridView1.Rows.Clear();
            if (errors.Any())
            {
                foreach (var error in errors)
                    dataGridView1.Rows.Add(error);
            }
            else
                dataGridView1.Rows.Add("No errors");
        }

        ConversationError<ConversationNode> m_current = null;
        IEnumerator<ConversationNode> m_node = null;

        public event Action<ConversationNode> HightlightNode;

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            var next = dataGridView1.Rows[e.RowIndex].Cells[0].Value as ConversationError<ConversationNode>;
            if (next != null)
            {
                if (next != m_current)
                {
                    m_current = next;
                    m_node = m_current.Nodes.GetEnumerator();
                }

                if (m_node.MoveNext())
                {
                    HightlightNode.Execute(m_node.Current);
                }
                else
                {
                    m_node = m_current.Nodes.GetEnumerator();
                    if (m_node.MoveNext())
                    {
                        HightlightNode.Execute(m_node.Current);
                    }
                }
            }
        }
    }
}
