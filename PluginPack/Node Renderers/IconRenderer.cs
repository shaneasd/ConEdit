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
        private static readonly IconRendererFactory s_instance = new IconRendererFactory();
        public static IconRendererFactory Instance => s_instance;

        public bool WillRender(Id<NodeTypeTemp> nodeType)
        {
            return true;
        }

        public string DisplayName => "Icon Renderer";

        public INodeGui GetRenderer(ConversationNode<INodeGui> n, PointF p, Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localizer, Func<IDataSource> datasource)
        {
            return new IconRenderer(n, p, localizer);
        }

        public Guid Guid { get; } = Guid.Parse("d629f0cb-58e5-4123-aab5-89617dabab31");
    }

    public class IconRenderer : NodeUI
    {
        string m_name;
        Bitmap m_image;

        public IconRenderer(ConversationNode<ConversationEditor.INodeGui> node, PointF p, Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localizer) :
            base(node, p)
        {
            m_localizer = localizer;
            SetIcon();
        }

        private void SetName()
        {
            m_name = "Node name not found";
            string parameterName = NameConfig.TryGet(Node.Data.Config);
            if (parameterName != null)
            {
                var stringParameters = Node.Data.Parameters.OfType<IStringParameter>().Where(p => p.Name == parameterName);
                if (stringParameters.Any())
                {
                    m_name = stringParameters.First().Value;
                }
                else
                {
                    var localizedStringParameters = Node.Data.Parameters.OfType<ILocalizedStringParameter>().Where(p => p.Name == parameterName);
                    if (localizedStringParameters.Any())
                    {
                        var p = localizedStringParameters.First();
                        m_name = m_localizer(Id<LocalizedStringType>.FromGuid(p.TypeId.Guid), p.Value);
                    }
                }
            }
        }

        private void SetIcon()
        {
            m_image = null;
            string icon = IconConfig.TryGet(Node.Data.Config);
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
        private Func<Id<LocalizedStringType>, Id<LocalizedText>, string> m_localizer;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_image != null)
                    m_image.Dispose();
            }
        }
    }
}
