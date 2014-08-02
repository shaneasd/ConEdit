using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ConversationEditor.GUIs.TypeEditors;
using Conversation;

namespace ConversationEditor
{
    public partial class DomainEditor : UserControl
    {
        private DomainFile m_domain;
        public IDataSource DataSource;
        public DomainEditor()
        {
            InitializeComponent();

            BackColor = Colors.Background;
            ForeColor = Colors.Foreground;

            greyScrollBar1.Minimum = 0;

            panel1.Paint += new PaintEventHandler(panel1_Paint);

            innerPanel.Size = new Size(panel1.Width - 2, 1);
            innerPanel.Location = new Point(1, 1);
        }

        void panel1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Colors.ControlBorder, new Rectangle(0, 0, panel1.Width - 1, panel1.Height - 1));
        }

        public DomainFile Domain
        {
            get { return m_domain; }
            set
            {
                m_domain = value;
                int tabIndex = int.MaxValue;
                innerPanel.Controls.Clear();
                foreach (var dynamicenumtype in value.Data.DynamicEnumerations)
                    innerPanel.Controls.Add(new DynamicEnumTypeEditor() { Dock = DockStyle.Top, Data = dynamicenumtype, TabIndex=tabIndex-- });
                foreach (var enumerationtype in value.Data.Enumerations)
                    innerPanel.Controls.Add(new EnumerationTypeEditor() { Dock = DockStyle.Top, Data = enumerationtype, TabIndex = tabIndex--, Padding = new Padding(3, 3, 3, 3) });
                foreach (var decimalType in value.Data.Decimals)
                    innerPanel.Controls.Add(new DecimalTypeEditor() { Dock = DockStyle.Top, Data = decimalType, TabIndex = tabIndex-- });
                foreach (var integerType in value.Data.Integers)
                    innerPanel.Controls.Add(new IntegerTypeEditor() { Dock = DockStyle.Top, Data = integerType, TabIndex = tabIndex-- });
                foreach (var nodeType in value.Data.NodeTypes)
                    innerPanel.Controls.Add(new NodeTypeEditor() { Dock = DockStyle.Top, DataSource = DataSource, Data = nodeType, TabIndex = tabIndex-- });

                UpdateInnerPanel();

                m_domain.Modify(m_domain.Data.NodeTypes, m_domain.Data.DynamicEnumerations, m_domain.Data.Enumerations, m_domain.Data.Decimals, m_domain.Data.Integers, m_domain.Data.Nodes);
            }
        }

        private void UpdateInnerPanel()
        {
            innerPanel.Height = innerPanel.Controls.OfType<Control>().Sum(c => c.Height) + 2;
            greyScrollBar1.Maximum = Math.Max(0, (innerPanel.Height - panel1.Height));
            greyScrollBar1.PercentageCovered = (float)panel1.Height / (float)innerPanel.Height;
            greyScrollBar1.Refresh();
        }

        private void greyScrollBar1_Scrolled()
        {
            innerPanel.Top = -(int)greyScrollBar1.Value;
        }

        private void btnAddDynamicEnum_Click(object sender, EventArgs e)
        {
            innerPanel.Controls.Add(new DynamicEnumTypeEditor() { Dock = DockStyle.Top, Data = new DynamicEnumerationData { Name = "New Dynamic Enumeration" } });
            UpdateInnerPanel();
        }
    }
}
