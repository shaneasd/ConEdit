using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ConversationEditor;
using Conversation;
using System.Reflection;
using System.IO;
using Utilities;

namespace PluginPack
{
    public partial class FancyCharacterEditor : UserControl, IParameterEditor<FancyCharacterEditor>, IFocusProvider
    {
        public class Factory : IParameterEditorFactory
        {
            public bool WillEdit(ID<ParameterType> type, WillEdit willEdit)
            {
                return willEdit.IsEnum(type);
            }

            public string Name
            {
                get { return "Fancy Character Editor"; }
            }

            public Guid Guid
            {
                get { return Guid.Parse("20873974-f9de-4fc8-a024-bef48e4c6280"); }
            }

            public IParameterEditor<Control> Make()
            {
                return new FancyCharacterEditor();
            }
        }

        public MyControl LastFocused { get { return m_button; } set { } }

        public FancyCharacterEditor()
        {
            InitializeComponent();

            drawWindow1.Paint += drawWindow1_Paint;
            m_button = new GenericButton(() => drawWindow1.DisplayRectangle, (r, g) => DrawButton(r, g, false, m_value), LaunchEditor);
            m_button.RegisterCallbacks(this, drawWindow1);
            m_editor = new FancyEnumForm();
        }

        FancyEnumForm m_editor;

        void LaunchEditor()
        {
            m_editor.Show();
        }

        void drawWindow1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Black, new Rectangle(e.ClipRectangle.Location, new Size(e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1)));
            //DrawButton(e.ClipRectangle, e.Graphics, false, m_parameter.Value);
        }

        public static Bitmap JONAS;
        public static Bitmap ALI;
        public static Bitmap DANIEL;
        public static Bitmap JUAN;

        static FancyCharacterEditor()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("PluginPack.Resources.jonas.png"))
                JONAS = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("PluginPack.Resources.ali.png"))
                ALI = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("PluginPack.Resources.daniel.png"))
                DANIEL = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("PluginPack.Resources.juan.png"))
                JUAN = new Bitmap(stream);
        }

        public void DrawButton(RectangleF area, Graphics g, bool selected, Guid value)
        {
            float size = area.Height - 4;
            float imageX = area.Width - area.Height + 2;
            Bitmap[] images = new[] { JONAS, ALI, DANIEL, JUAN };
            System.Security.Cryptography.MD5Cng hash = new System.Security.Cryptography.MD5Cng();
            Bitmap image = images[hash.ComputeHash(value.ToByteArray())[0] & 3];
            g.DrawImage(image, RectangleF.FromLTRB(area.Left + imageX, area.Top + 2, area.Right - 2, area.Bottom - 2));
            TextRenderer.DrawText(g, m_parameter.GetName(value), Font, new Point((int)area.X + 2, (int)area.Y + ((int)area.Height - Font.Height) / 2), Color.White);
        }

        GenericButton m_button;

        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as IEnumParameter;
            m_value = m_parameter.Value;
        }

        public FancyCharacterEditor AsControl
        {
            get { return this; }
        }

        public UpdateParameterData UpdateParameterAction()
        {
            throw new NotImplementedException();
        }

        public bool IsValid()
        {
            throw new NotImplementedException();
        }

        public event Action Ok;
        private IEnumParameter m_parameter;
        private Guid m_value;
    }
}
