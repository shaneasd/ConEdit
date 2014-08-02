using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Utilities;

namespace ConversationEditor
{
    public class GraphViewConfig : IConfigParameter
    {
        public void Load(XElement root)
        {
            var node = root.Element("GraphView");
            if (node != null)
            {
                var showGrid = node.Element("ShowGrid");
                if (showGrid != null)
                    m_showGrid = bool.Parse(showGrid.Attribute("value").Value);

                var snapToGrid = node.Element("SnapToGrid");
                if (snapToGrid != null)
                    m_snapToGrid = bool.Parse(snapToGrid.Attribute("value").Value);

                var showIds = node.Element("ShowIDs");
                if (showIds != null)
                    m_showIDs = bool.Parse(showIds.Attribute("value").Value);
            }
        }

        public void Write(XElement root)
        {
            var node = new XElement("GraphView");
            root.Add(node);
            {
                var showGrid = new XElement("ShowGrid", new XAttribute("value", m_showGrid));
                node.Add(showGrid);

                var snapToGrid = new XElement("SnapToGrid", new XAttribute("value", m_snapToGrid));
                node.Add(snapToGrid);

                var showIds = new XElement("ShowIDs", new XAttribute("value", m_showIDs));
                node.Add(showIds);
            }
        }

        public event Action ValueChanged;

        private bool m_showGrid = false;
        public bool ShowGrid
        {
            get { return m_showGrid; }
            set
            {
                m_showGrid = value;
                ValueChanged.Execute();
            }
        }

        private bool m_snapToGrid = false;
        public bool SnapToGrid
        {
            get { return m_snapToGrid; }
            set
            {
                m_snapToGrid = value;
                ValueChanged.Execute();
            }
        }

        private bool m_showIDs = false;
        public bool ShowIDs
        {
            get { return m_showIDs; }
            set
            {
                m_showIDs = value;
                ValueChanged.Execute();
            }
        }
    }
}
