using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utilities;

namespace ConversationEditor.GUIs.Default_Parameter_Editors
{
    public partial class MyLocalizedStringEditor : Control
    {
        MyTextBox m_textBox;

        public MyLocalizedStringEditor()
        {
            InitializeComponent();

            m_textBox = new MyTextBox(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height));
            m_textBox.Colors = new MyTextBox.ColorOptions();
            m_textBox.RequestedAreaChanged += () => this.Size = m_textBox.RequestedArea.ToSize();
            m_textBox.Text = "Test String";

            drawWindow1.MouseDown += (a, args) => m_textBox.MouseDown(args);
            drawWindow1.MouseUp += (a, args) => m_textBox.MouseUp(args);
            drawWindow1.MouseMove += (a, args) => m_textBox.MouseMove(args);
            drawWindow1.MouseClick += (a, args) => m_textBox.MouseClick(args);
            drawWindow1.KeyPress += (a, args) => m_textBox.KeyPress(args);
            drawWindow1.KeyDown += (a, args) => m_textBox.KeyDown(args);
            drawWindow1.Paint += (a, args) => m_textBox.Draw(args.Graphics);
            drawWindow1.GotFocus += (a, args) => m_textBox.GotFocus();
            drawWindow1.LostFocus += (a, args) => m_textBox.LostFocus();
        }

        public override string Text
        { 
            get { return m_textBox.Text; }
            set { m_textBox.Text = value; } 
        } 
    }
}
