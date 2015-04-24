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
    public class TerminatorGUIFactory : NodeUI.IFactory
    {
        public static TerminatorGUIFactory Instance = new TerminatorGUIFactory();

        public bool WillRender(ID<NodeTypeTemp> nodeType)
        {
            return nodeType == SpecialNodes.Terminator;
        }

        public string DisplayName
        {
            get { return "Terminator Node Renderer"; }
        }

        public INodeGUI GetRenderer(ConversationNode<INodeGUI> n, PointF p, Func<ID<LocalizedText>, string> localizer, Func<IDataSource> datasource)
        {
            return new TerminatorGUI(n, p, localizer);
        }

        Guid m_guid = Guid.Parse("aacda2b5-bd9e-4fd2-ad0c-5f6a4d764702");
        public Guid Guid
        {
            get { return m_guid; }
        }
    }

    internal class TerminatorGUI : NodeUI
    {
        Image image;
        public TerminatorGUI(ConversationNode<INodeGUI> node, PointF p, Func<ID<LocalizedText>, string> localizer)
            : base(node, p)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.End Icon.png"))
                image = new Bitmap(stream);
        }

        protected override void InnerDraw(Graphics g, bool selected)
        {
            g.DrawImage(image, Area);
            if (selected)
            {
                Pen outline = new Pen(Brushes.Black, 2);
                g.DrawRectangle(outline, Area);
            }
        }

        protected override SizeF CalculateArea(Graphics g)
        {
            return image.Size;
        }

        public override string DisplayName
        {
            get { return TerminatorGUIFactory.Instance.DisplayName; }
        }
    }
}
