using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Drawing;
using Utilities;
using System.Reflection;
using System.IO;
using Utilities.UI;

namespace ConversationEditor
{
    public class StartGuiFactory : NodeUI.IFactory
    {
        public static StartGuiFactory Instance { get; } = new StartGuiFactory();

        public bool WillRender(Id<NodeTypeTemp> nodeType)
        {
            return nodeType == SpecialNodes.Start;
        }

        public string DisplayName => "Start Node Renderer";

        public INodeGui GetRenderer(ConversationNode<INodeGui> n, PointF p, Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localizer, Func<IDataSource> datasource)
        {
            return new StartGui(n, p);
        }

        public Guid Guid { get; } = Guid.Parse("346ac22d-6393-4958-8d36-fedff89b40c0");
    }

    internal class StartGui : NodeUI
    {
        Image m_image;
        public StartGui(ConversationNode<INodeGui> node, PointF p)
            : base(node, p)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.Start Icon.png"))
                m_image = new Bitmap(stream);
        }

        protected override void InnerDraw(Graphics g, bool selected)
        {
            g.DrawImage(m_image, Area);
            if (selected)
            {
                using (Pen outline = new Pen(Brushes.Black, 2))
                {
                    g.DrawRectangle(outline, Area);
                }
            }
        }

        protected override SizeF CalculateArea(Graphics g)
        {
            return m_image.Size;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                m_image.Dispose();
        }
    }
}
