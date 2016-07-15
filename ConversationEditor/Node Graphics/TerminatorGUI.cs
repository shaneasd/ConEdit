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
    public class TerminatorGuiFactory : NodeUI.IFactory
    {
        public static TerminatorGuiFactory Instance { get; } = new TerminatorGuiFactory();

        public bool WillRender(Id<NodeTypeTemp> nodeType)
        {
            return nodeType == SpecialNodes.Terminator;
        }

        public string DisplayName
        {
            get { return "Terminator Node Renderer"; }
        }

        public INodeGui GetRenderer(ConversationNode<INodeGui> n, PointF p, Func<Id<LocalizedText>, string> localizer, Func<IDataSource> datasource)
        {
            return new TerminatorGui(n, p, localizer);
        }

        Guid m_guid = Guid.Parse("aacda2b5-bd9e-4fd2-ad0c-5f6a4d764702");
        public Guid Guid
        {
            get { return m_guid; }
        }
    }

    internal class TerminatorGui : NodeUI
    {
        Image m_image;
        public TerminatorGui(ConversationNode<INodeGui> node, PointF p, Func<Id<LocalizedText>, string> localizer)
            : base(node, p)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.End Icon.png"))
                m_image = new Bitmap(stream);
        }

        protected override void InnerDraw(Graphics g, bool selected)
        {
            g.DrawImage(m_image, Area);
            if (selected)
            {
                Pen outline = new Pen(Brushes.Black, 2);
                g.DrawRectangle(outline, Area);
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
