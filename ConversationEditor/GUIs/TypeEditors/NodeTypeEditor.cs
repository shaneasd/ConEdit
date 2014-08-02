using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Conversation;
using Utilities;

namespace ConversationEditor.GUIs.TypeEditors
{
    public partial class NodeTypeEditor : UserControl
    {
        MyTextBox m_textBox;
        MyComboBox<Guid> m_comboBox;
        float m_textHeight;
        float m_textWidth;
        string TEXT = "Node Type:";

        public NodeTypeEditor()
        {
            InitializeComponent();

            using (Graphics g = CreateGraphics())
            {
                var textSize = g.MeasureString(TEXT, Font);
                m_textHeight = textSize.Height;
                m_textWidth = textSize.Width;
            }

            m_textBox = new MyTextBox(drawWindow1, TextBoxArea, MyTextBox.InputFormEnum.Text);
            m_textBox.Colors.BorderPen = Colors.ControlBorder;
            m_textBox.RequestedAreaChanged += () =>
            {
                Size = m_textBox.RequestedArea.ToSize();
                MinimumSize = new Size(0, Size.Height);
            };

            m_comboBox = new MyComboBox<Guid>(drawWindow1, ComboBoxArea, false);
            m_comboBox.SetupCallbacks();
            m_comboBox.RequestedAreaChanged += () =>
            {
                MinimumSize = new Size(0, (int)m_comboBox.RequestedArea.Height);
                Size = m_comboBox.RequestedArea.ToSize();
                Refresh();
            };
            m_comboBox.Colors.BorderPen = Colors.ControlBorder;
            m_comboBox.Renderer = Colors.ContextMenu;

            drawWindow1.MouseDown += (a, args) => m_textBox.MouseDown(args);
            drawWindow1.MouseUp += (a, args) => m_textBox.MouseUp(args);
            drawWindow1.MouseMove += (a, args) => m_textBox.MouseMove(args);
            drawWindow1.MouseClick += (a, args) => m_textBox.MouseClick(args);
            drawWindow1.KeyPress += (a, args) => m_textBox.KeyPress(args);
            drawWindow1.KeyDown += (a, args) => m_textBox.KeyDown(args);
            drawWindow1.Paint += (a, args) => m_textBox.Paint(args.Graphics);
            drawWindow1.GotFocus += (a, args) => m_textBox.GotFocus();
            drawWindow1.LostFocus += (a, args) => m_textBox.LostFocus();

            drawWindow1.MouseDown += (a, args) => m_comboBox.MouseDown(args);
            drawWindow1.MouseUp += (a, args) => m_comboBox.MouseUp(args);
            drawWindow1.MouseMove += (a, args) => m_comboBox.MouseMove(args);
            drawWindow1.MouseClick += (a, args) => m_comboBox.MouseClick(args);
            drawWindow1.KeyPress += (a, args) => m_comboBox.KeyPress(args);
            drawWindow1.KeyDown += (a, args) => m_comboBox.KeyDown(args);
            drawWindow1.Paint += (a, args) => m_comboBox.Paint(args.Graphics);
            drawWindow1.GotFocus += (a, args) => m_comboBox.GotFocus();
            drawWindow1.LostFocus += (a, args) => m_comboBox.LostFocus();
        }

        MyComboBox<Guid>.Item NoParentItem = new MyComboBox<Guid>.Item("NO PARENT", Guid.Empty);

        IDataSource m_datasource = null;
        public IDataSource DataSource
        {
            get { return m_datasource; }
            set
            {
                m_datasource = value;
                UpdateData();
            }
        }

        private NodeTypeData? m_data;
        public NodeTypeData Data
        {
            get { return m_data.Value; }
            set
            {
                m_data = value;
                UpdateData();
            }
        }

        private void UpdateData()
        {
            if (m_data != null && m_datasource != null)
            {
                var nodesExceptThis = m_datasource.Nodes.Collapse(n => n.ChildTypes, n => n.ChildTypes).Where(n => n.Guid != m_data.Value.Guid);

                m_textBox.Text = m_data.Value.Name;

                m_comboBox.Items.Clear();
                m_comboBox.Items.Add(NoParentItem);
                m_comboBox.Items.AddRange(nodesExceptThis.Select(type => new MyComboBox<Guid>.Item(type.Name, type.Guid)));

                var parent = nodesExceptThis.SingleOrDefault(t => t.Guid == m_data.Value.Parent);
                if (parent != null)
                    m_comboBox.SelectedItem = m_comboBox.Items.Single(i => i.Contents == parent.Guid);
                else
                    m_comboBox.SelectedItem = NoParentItem;
            }
        }

        const int CONTROL_WIDTHS = 120;

        private RectangleF TextBoxArea()
        {
            return new RectangleF(m_textWidth + 6, 0, CONTROL_WIDTHS, drawWindow1.Height);
        }

        private RectangleF ComboBoxArea()
        {
            return new RectangleF(m_textWidth + 6 + CONTROL_WIDTHS, 0, CONTROL_WIDTHS, drawWindow1.Height);
        }

        private void drawWindow1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawString(TEXT, Font, Colors.ForegroundBrush, new PointF(3, (drawWindow1.Height - m_textHeight) / 2));
        }
    }
}
