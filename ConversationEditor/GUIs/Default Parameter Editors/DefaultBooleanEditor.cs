using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Conversation;
using System.Drawing.Drawing2D;
using Utilities;

namespace ConversationEditor
{
    public partial class DefaultBooleanEditor : UserControl, IParameterEditor<DefaultBooleanEditor>
    {
        public class Factory : IParameterEditorFactory
        {
            public static readonly Guid GUID = Guid.Parse("87253284-4ade-4a7d-9043-81f3b58f1ba5");

            public bool WillEdit(ID<ParameterType> type, WillEdit willEdit)
            {
                return type == BaseTypeBoolean.PARAMETER_TYPE;
            }

            public string Name
            {
                get { return "Default Boolean Editor"; }
            }

            public Guid Guid
            {
                get { return GUID; }
            }

            public IParameterEditor<Control> Make()
            {
                return new DefaultBooleanEditor();
            }
        }

        public DefaultBooleanEditor()
        {
            InitializeComponent();

            drawWindow1.Paint += new PaintEventHandler(drawWindow1_Paint);
            drawWindow1.MouseClick += new MouseEventHandler(drawWindow1_MouseClick);
        }

        public bool Checked { get; set; }

        void drawWindow1_MouseClick(object sender, MouseEventArgs e)
        {
            using (var g = drawWindow1.CreateGraphics())
            {
                if (BoxRectangle(g).Contains(e.Location))
                {
                    Checked = !Checked;
                }
            }
            Invalidate(true);
        }

        string DisplayText
        {
            get
            {
                return Checked ? "True" : "False";
            }
        }

        public Rectangle BoxRectangle(Graphics g)
        {
            return new Rectangle(7, (int)((drawWindow1.Height - boxSize.Height) / 2), (int)boxSize.Width, (int)boxSize.Height);
        }

        int gap = 4;
        SizeF boxSize = new SizeF(15, 15);

        void drawWindow1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            SizeF textSize = e.Graphics.MeasureString("False", Font);
            Rectangle boxRectangle = BoxRectangle(e.Graphics);
            Point textLocation = new Point((int)boxRectangle.X + (int)boxSize.Width + gap, (int)(drawWindow1.Height - textSize.Height) / 2);
            e.Graphics.DrawRectangle(ColorScheme.ControlBorder, Rectangle.FromLTRB(0, 0, drawWindow1.Width - 1, drawWindow1.Height - 1));
            DrawCheckBox(e.Graphics, boxRectangle, Checked);
            e.Graphics.DrawString(DisplayText, Font, ColorScheme.ForegroundBrush, textLocation);
        }

        public static void DrawCheckBox(Graphics g, Rectangle boxRectangle, bool check)
        {
            g.DrawRectangle(ColorScheme.ForegroundPen, boxRectangle);
            if (check)
                g.FillRectangle(ColorScheme.ForegroundBrush, new Rectangle(boxRectangle.X + 2, boxRectangle.Y + 2, boxRectangle.Width - 3, boxRectangle.Height - 3));
        }

        IBooleanParameter m_parameter;
        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as IBooleanParameter;
            if (!data.Parameter.Corrupted)
                Checked = m_parameter.Value;
        }

        public DefaultBooleanEditor AsControl
        {
            get { return this; }
        }

        public UpdateParameterData UpdateParameterAction()
        {
            return m_parameter.SetValueAction(Checked);
        }

        public bool IsValid()
        {
            return true;
        }

        public event Action Ok
        {
            add { }
            remove { }
        }
    }
}
