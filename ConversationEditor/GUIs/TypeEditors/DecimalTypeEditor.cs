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
    public partial class DecimalTypeEditor : UserControl
    {
        MyTextBox m_name;
        MyNumericUpDown<decimal> m_min;
        MyNumericUpDown<decimal> m_max;
        MyNumericUpDown<decimal> m_default;
        float m_textHeight;
        float m_textWidth;
        float m_minLabelWidth;
        float m_maxLabelWidth;
        float m_defaultLabelWidth;
        public const string TEXT = "Decimal Type";
        string MIN_LABEL = "Minimum: ";
        string MAX_LABEL = "Maximum: ";
        string DEFAULT_LABEL = "Default: ";
        float NUMERIC_INPUT_WIDTH = 120;

        float m_nameHeight = 0;
        float m_numericsHeight = 22;

        private void ResetSize()
        {
            m_nameHeight = m_name.RequestedArea.Height;
            m_numericsHeight = new[] { m_min.RequestedArea.Height, m_max.RequestedArea.Height, m_default.RequestedArea.Height }.Max();
            drawWindow1.MinimumSize = new Size(0, (int)(m_nameHeight + 2 + m_numericsHeight));
            drawWindow1.Width = (int)MyMath.Ceiling(DefaultNumericArea().Right);
            drawWindow1.Height = (int)(m_nameHeight + 2 + m_numericsHeight);
        }

        public DecimalTypeEditor()
        {
            InitializeComponent();

            using (Graphics g = CreateGraphics())
            {
                var textSize = g.MeasureString(TEXT, Font);
                m_textHeight = textSize.Height;
                m_textWidth = textSize.Width;
                m_minLabelWidth = MyMath.Ceiling(g.MeasureString(MIN_LABEL, Font).Width);
                m_maxLabelWidth = MyMath.Ceiling(g.MeasureString(MAX_LABEL, Font).Width);
                m_defaultLabelWidth = MyMath.Ceiling(g.MeasureString(DEFAULT_LABEL, Font).Width);
            }

            m_name = new MyTextBox(drawWindow1, NameArea, MyTextBox.InputFormEnum.Text);
            m_name.Colors.BorderPen = Colors.ControlBorder;
            m_name.RequestedAreaChanged += ResetSize;

            m_min = new MyNumericUpDown<decimal>(drawWindow1, MinNumericArea, true);
            m_min.RequestedAreaChanged += ResetSize;
            m_min.Colors.BorderPen = Colors.ControlBorder;
            m_min.Minimum = decimal.MinValue;
            m_min.Maximum = decimal.MaxValue;

            m_max = new MyNumericUpDown<decimal>(drawWindow1, MaxNumericArea, true);
            m_max.RequestedAreaChanged += ResetSize;
            m_max.Colors.BorderPen = Colors.ControlBorder;
            m_max.Minimum = decimal.MinValue;
            m_max.Maximum = decimal.MaxValue;

            m_default = new MyNumericUpDown<decimal>(drawWindow1, DefaultNumericArea, true);
            m_default.RequestedAreaChanged += ResetSize;
            m_default.Colors.BorderPen = Colors.ControlBorder;
            m_default.Minimum = decimal.MinValue;
            m_default.Maximum = decimal.MaxValue;

            drawWindow1.MouseDown += (a, args) => drawWindow1.Focus(); //TODO: is this redundant?
            drawWindow1.Paint += (a, args) => Paint(args.Graphics);
            drawWindow1.GotFocus += (a, b) => { forwardTab.TabStop = false; backwardTab.TabStop = false; };
            forwardTab.GotFocus += (a, b) => { MyControls.ForwardFocus(); drawWindow1.Focus(); }; //Focus draw window so we dont keep giving focus to forwardTab
            backwardTab.GotFocus += (a, b) => { MyControls.BackwardFocus(); drawWindow1.Focus(); }; //Focus draw window so we dont keep giving focus to backwardTab
            this.Leave += (a, b) => { forwardTab.TabStop = true; backwardTab.TabStop = true; };

            forwardTab.Size = Size.Empty;
            forwardTab.Location = new Point(-1, -1);

            backwardTab.Size = Size.Empty;
            backwardTab.Location = new Point(-1, -1);

            MyControls = new ControlSet(m_name, m_min, m_max, m_default);
            MyControls.RegisterCallbacks(drawWindow1);

            ResetSize();
        }

        ControlSet MyControls;

        protected override bool ProcessTabKey(bool forward)
        {
            if (MyControls.ProcessTabKey(forward))
                return true;
            else
                return base.ProcessTabKey(forward);
        }

        DecimalData m_data;
        public DecimalData Data
        {
            get
            {
                return m_data;
            }
            set
            {
                m_data = value;
                m_name.Text = value.Name;
                m_min.Value = value.Min.GetValueOrDefault(decimal.MinValue);
                m_max.Value = value.Max.GetValueOrDefault(decimal.MaxValue);
                m_default.Value = value.Default.GetValueOrDefault(0m.Clamp(decimal.MinValue, decimal.MaxValue));
            }
        }

        RectangleF NameArea()
        {
            return RectangleF.FromLTRB(m_textWidth + 6, 0, m_textWidth + 6 + m_minLabelWidth + 3 + NUMERIC_INPUT_WIDTH + 3 + m_maxLabelWidth + NUMERIC_INPUT_WIDTH + 3 + m_defaultLabelWidth + 3 + NUMERIC_INPUT_WIDTH, m_nameHeight);
        }

        RectangleF MinNumericArea()
        {
            return new RectangleF(m_textWidth + 6 + m_minLabelWidth + 3, m_nameHeight + 2, NUMERIC_INPUT_WIDTH, m_numericsHeight);
        }

        RectangleF MaxNumericArea()
        {
            return new RectangleF(m_textWidth + 6 + m_minLabelWidth + 3 + NUMERIC_INPUT_WIDTH + 3 + m_maxLabelWidth, m_nameHeight + 2, NUMERIC_INPUT_WIDTH, m_numericsHeight);
        }

        RectangleF DefaultNumericArea()
        {
            return new RectangleF(m_textWidth + 6 + m_minLabelWidth + 3 + NUMERIC_INPUT_WIDTH + 3 + m_maxLabelWidth + NUMERIC_INPUT_WIDTH + 3 + m_defaultLabelWidth + 3, m_nameHeight + 2, NUMERIC_INPUT_WIDTH, m_numericsHeight);
        }

        private new void Paint(Graphics g)
        {
            g.DrawString(TEXT, Font, Colors.ForegroundBrush, new PointF(3, (drawWindow1.Height - m_textHeight) / 2));
            g.DrawString(MIN_LABEL, Font, Colors.ForegroundBrush, new PointF(m_textWidth + 6, m_nameHeight + 2 + (m_numericsHeight - m_textHeight) / 2));
            g.DrawString(MAX_LABEL, Font, Colors.ForegroundBrush, new PointF(m_textWidth + 6 + m_minLabelWidth + 3 + NUMERIC_INPUT_WIDTH + 3, m_nameHeight + 2 + (m_numericsHeight - m_textHeight) / 2));
            g.DrawString(DEFAULT_LABEL, Font, Colors.ForegroundBrush, new PointF(m_textWidth + 6 + m_minLabelWidth + 3 + NUMERIC_INPUT_WIDTH + 3 + m_maxLabelWidth + NUMERIC_INPUT_WIDTH + 3, m_nameHeight + 2 + (m_numericsHeight - m_textHeight) / 2));
            g.DrawRectangle(Colors.ControlBorder, new Rectangle(0, 0, drawWindow1.Width - 1, drawWindow1.Height - 1));
        }
    }
}
