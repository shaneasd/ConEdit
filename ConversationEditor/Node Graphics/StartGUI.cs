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
    public class StartGUIFactory : NodeUI.IFactory
    {
        private static StartGUIFactory m_instance = new StartGUIFactory();
        public static StartGUIFactory Instance { get { return m_instance; } }

        public bool WillRender(ID<NodeTypeTemp> nodeType)
        {
            return nodeType == SpecialNodes.Start;
        }

        public string DisplayName
        {
            get { return "Start Node Renderer"; }
        }

        public INodeGUI GetRenderer(ConversationNode<INodeGUI> n, PointF p, Func<ID<LocalizedText>, string> localizer, Func<IDataSource> datasource)
        {
            return new StartGUI(n, p, localizer);
        }

        static Guid m_guid = Guid.Parse("346ac22d-6393-4958-8d36-fedff89b40c0");
        public Guid Guid
        {
            get { return m_guid; }
        }
    }

    internal class StartGUI : NodeUI
    {
        Image image;
        public StartGUI(ConversationNode<INodeGUI> node, PointF p, Func<ID<LocalizedText>, string> localizer)
            : base(node, p)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.Start Icon.png"))
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
            get { return StartGUIFactory.Instance.DisplayName; }
        }
    }
}
