using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using Conversation;
using System.Drawing;
using System.IO;
using Utilities;

namespace PluginPack
{
    public class IconRendererFactory : NodeUI.IFactory
    {
        private static readonly IconRendererFactory m_instance = new IconRendererFactory();
        public static IconRendererFactory Instance { get { return m_instance; } }

        public bool WillRender(ID<NodeTypeTemp> nodeType)
        {
            return true;
        }

        public string DisplayName
        {
            get { return "Icon Renderer"; }
        }

        public INodeGUI GetRenderer(ConversationNode<INodeGUI> n, PointF p, Func<ID<LocalizedText>, string> localizer, Func<IDataSource> datasource)
        {
            return new IconRenderer(n, p, localizer);
        }

        static Guid m_guid = Guid.Parse("d629f0cb-58e5-4123-aab5-89617dabab31");
        public Guid Guid
        {
            get { return m_guid; }
        }
    }

    public class IconRenderer : NodeUI, IDisposable
    {
        string m_name;
        Bitmap m_image;

        public IconRenderer(ConversationNode<ConversationEditor.INodeGUI> node, PointF p, Func<ID<LocalizedText>, string> localizer) :
            base(node, p)
        {
            m_localizer = localizer;
            SetIcon();
        }

        private void SetName()
        {
            m_name = "Node name not found";
            string parameterName = NameConfig.TryGet(Node.Config);
            if (parameterName != null)
            {
                var stringParameters = Node.Parameters.OfType<IStringParameter>().Where(p => p.Name == parameterName);
                if (stringParameters.Any())
                {
                    m_name = stringParameters.First().Value;
                }
                else
                {
                    var localizedStringParameters = Node.Parameters.OfType<ILocalizedStringParameter>().Where(p => p.Name == parameterName);
                    if (localizedStringParameters.Any())
                    {
                        m_name = m_localizer(localizedStringParameters.First().Value);
                    }
                }
            }
        }

        private void SetIcon()
        {
            m_image = null;
            string icon = IconConfig.TryGet(Node.Config);
            if (icon != null)
            {
                try
                {
                    if (File.Exists(icon))
                        m_image = new Bitmap(icon);
                }
                catch (Exception) //Can't be any more specific msdn?
                {
                }
            }
            if (m_image == null)
            {
                m_image = new Bitmap(30, 30);
                using (Graphics g = Graphics.FromImage(m_image))
                    g.FillRectangle(Brushes.Orange, new Rectangle(0, 0, m_image.Width, m_image.Height));
            }
        }

        public override string DisplayName
        {
            get { return IconRendererFactory.Instance.DisplayName; }
        }

        protected override void InnerDraw(System.Drawing.Graphics g, bool selected)
        {
            SetName();

            SizeF textSize = g.MeasureString(m_name, Font);
            g.DrawImage(m_image, (int)(Area.X + (Area.Width - m_image.Width) / 2), (int)Area.Y, m_image.Width, m_image.Height);
            g.DrawString(m_name, Font, Brushes.Black, new PointF(Area.X + (Area.Width - textSize.Width) / 2, Area.Y + m_image.Height + NAME_ICON_SPACING));
        }

        protected override SizeF CalculateArea(System.Drawing.Graphics g)
        {
            SetName();
            SizeF textSize = g.MeasureString(m_name, Font);
            return new SizeF(Math.Max(textSize.Width, m_image.Width), textSize.Height + NAME_ICON_SPACING + m_image.Height);
        }

        static readonly Font Font = SystemFonts.DefaultFont;
        const float NAME_ICON_SPACING = 2;
        private Func<ID<LocalizedText>, string> m_localizer;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_image != null)
                    m_image.Dispose();
            }
        }
    }
}
