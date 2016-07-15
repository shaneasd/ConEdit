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
using System.Reflection;
using System.IO;

namespace ConversationEditor
{
    public class DefaultBooleanEditorFactory : IParameterEditorFactory
    {
        public static readonly Guid StaticId = Guid.Parse("87253284-4ade-4a7d-9043-81f3b58f1ba5");

        public bool WillEdit(ParameterType type, WillEdit queries)
        {
            return type == BaseTypeBoolean.PARAMETER_TYPE;
        }

        public string Name
        {
            get { return "Default Boolean Editor"; }
        }

        public Guid Guid
        {
            get { return StaticId; }
        }

        public IParameterEditor<Control> Make(ColorScheme scheme)
        {
            var result = new DefaultBooleanEditor();
            result.Scheme = scheme;
            return result;
        }
    }

    internal partial class DefaultBooleanEditor : UserControl, IParameterEditor<DefaultBooleanEditor>
    {
        private static Bitmap ToggleButtonOff;
        private static Bitmap ToggleButtonOffHover;
        private static Bitmap ToggleButtonOffPressed;
        private static Bitmap ToggleButtonOn;
        private static Bitmap ToggleButtonOnHover;
        private static Bitmap ToggleButtonOnPressed;
        static DefaultBooleanEditor()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.ToggleButtonOff.png"))
                ToggleButtonOff = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.ToggleButtonOffHover.png"))
                ToggleButtonOffHover = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.ToggleButtonOffPressed.png"))
                ToggleButtonOffPressed = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.ToggleButtonOn.png"))
                ToggleButtonOn = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.ToggleButtonOnHover.png"))
                ToggleButtonOnHover = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.ToggleButtonOnPressed.png"))
                ToggleButtonOnPressed = new Bitmap(stream);
        }

        ColorScheme m_colorScheme;
        public ColorScheme Scheme
        {
            get { return m_colorScheme; }
            set
            {
                m_colorScheme = value;
                drawWindow1.ColorScheme = value;
            }
        }
        public DefaultBooleanEditor()
        {
            InitializeComponent();

            drawWindow1.Paint += new PaintEventHandler(drawWindow1_Paint);
            drawWindow1.MouseClick += new MouseEventHandler(drawWindow1_MouseClick);
            drawWindow1.MouseDown += new MouseEventHandler(drawWindow1_MouseDown);
            drawWindow1.MouseUp += new MouseEventHandler(drawWindow1_MouseUp);
            drawWindow1.MouseCaptureChanged += new EventHandler(drawWindow1_MouseCaptureChanged);
            drawWindow1.MouseMove += new MouseEventHandler(drawWindow1_MouseMove);
        }

        bool m_held = false;

        void drawWindow1_MouseCaptureChanged(object sender, EventArgs e)
        {
            m_held = false;
            m_hovered = false;
            Invalidate(true);
        }

        void drawWindow1_MouseUp(object sender, MouseEventArgs e)
        {
            m_held = false;
            Invalidate(true);
        }

        void drawWindow1_MouseDown(object sender, MouseEventArgs e)
        {
            m_held = true;
            Invalidate(true);
        }

        public bool Checked { get; set; }

        void drawWindow1_MouseClick(object sender, MouseEventArgs e)
        {
            if (BoxRectangle().Contains(e.Location))
            {
                Checked = !Checked;
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

        public Rectangle BoxRectangle()
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
            Rectangle boxRectangle = BoxRectangle();
            Point textLocation = new Point((int)boxRectangle.X + (int)boxSize.Width + gap, (int)(drawWindow1.Height - textSize.Height) / 2);
            e.Graphics.DrawRectangle(Scheme.ControlBorder, Rectangle.FromLTRB(0, 0, drawWindow1.Width - 1, drawWindow1.Height - 1));
            DrawCheckBox(e.Graphics, boxRectangle, Checked, m_held, m_hovered);
            e.Graphics.DrawString(DisplayText, Font, Scheme.ForegroundBrush, textLocation);
        }

        public static void DrawCheckBox(Graphics g, Rectangle boxRectangle, bool check, bool held, bool hovered)
        {
            Image image = check && held ? ToggleButtonOnPressed :
                          check && hovered ? ToggleButtonOnHover :
                          check ? ToggleButtonOn :
                          held ? ToggleButtonOffPressed :
                          hovered ? ToggleButtonOffHover :
                          ToggleButtonOff;
            g.DrawImage(image, boxRectangle.Left, boxRectangle.Top, image.Width, image.Height);
        }

        void drawWindow1_MouseMove(object sender, MouseEventArgs e)
        {
            Rectangle boxRectangle = BoxRectangle();
            if (boxRectangle.Contains(e.Location))
                m_hovered = true;
            else
                m_hovered = false; 
            Invalidate(true);
        }

        IBooleanParameter m_parameter;
        private bool m_hovered;
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

        public string IsValid()
        {
            return null;
        }

        public event Action Ok
        {
            add { }
            remove { }
        }
    }
}
