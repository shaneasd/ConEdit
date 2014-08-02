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
    public partial class MyComboBoxControl : UserControl
    {
        //private MyComboBox<Guid> m_comboBox;
        public MyComboBoxControl()
        {
            InitializeComponent();

            //m_comboBox = new MyComboBox<Guid>(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), true);
            //m_comboBox.SetupCallbacks();
            //m_comboBox.RequestedAreaChanged += () =>
            //{
            //    MinimumSize = new Size(0, (int)m_comboBox.RequestedArea.Height);
            //    Size = m_comboBox.RequestedArea.ToSize();
            //    Refresh();
            //};
            //m_comboBox.Colors.BorderPen = Colors.ControlBorder;
            //m_comboBox.Renderer = Colors.ContextMenu;
        }
    }
}
