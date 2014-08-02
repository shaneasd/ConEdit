using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utilities;

namespace ConversationEditor
{
    public partial class MyTextBoxControl : UserControl
    {
        MyTextBox m_textBox;
        public MyTextBoxControl()
        {
            InitializeComponent();

            m_textBox = new MyTextBox( drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), MyTextBox.InputFormEnum.Text);
            m_textBox.Colors.BorderPen = ColorScheme.ControlBorder;
            m_textBox.RequestedAreaChanged += () =>
            {
                Size = m_textBox.RequestedArea.ToSize();
                MinimumSize = new Size(0, Size.Height);
            };
            //m_textBox.EnterPressed += () => Ok.Execute();
            //m_textBox.SpecialEnter = true;
            MyTextBox.SetupCallbacks(drawWindow1, m_textBox);
        }
    }
}
