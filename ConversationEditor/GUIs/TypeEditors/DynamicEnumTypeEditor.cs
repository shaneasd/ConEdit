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

namespace ConversationEditor.GUIs.TypeEditors
{
    public partial class DynamicEnumTypeEditor : UserControl
    {
        const string TITLE = " Dynamic Enumeration:";
        const int BUFFER = 5;
        float m_titleWidth;
        float m_titleHeight;

        MyTextBox m_textBox;
        public DynamicEnumTypeEditor()
        {
            InitializeComponent();

            using (var g = CreateGraphics())
            {
                var titleSize = g.MeasureString(TITLE, Font);
                m_titleWidth = titleSize.Width;
                m_titleHeight = titleSize.Height;
            }

            drawWindow1.Paint += new PaintEventHandler(drawWindow1_Paint);

            m_textBox = new MyTextBox(drawWindow1, () => new RectangleF(m_titleWidth + BUFFER, 0, drawWindow1.Width - m_titleWidth - BUFFER, drawWindow1.Height), MyTextBox.InputFormEnum.Text);
            MyTextBox.SetupCallbacks(drawWindow1, m_textBox);
            m_textBox.RequestedAreaChanged += () => Height = (int)m_textBox.RequestedArea.Height;
        }

        public DynamicEnumerationData Data
        {
            get { return new DynamicEnumerationData { Name = m_textBox.Text }; }
            set { m_textBox.Text = value.Name; }
        }

        void drawWindow1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawString(TITLE, Font, Colors.ForegroundBrush, 0, (drawWindow1.Height - m_titleHeight) / 2);
            e.Graphics.DrawRectangle(Colors.ControlBorder, new Rectangle(0, 0, drawWindow1.Width - 1, drawWindow1.Height - 1));
        }
    }
}
