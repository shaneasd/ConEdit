using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Utilities;

namespace ConversationEditor
{
    internal class GraphViewConfig : IConfigParameter
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
                    m_showIds = bool.Parse(showIds.Attribute("value").Value);

                var minorGridSpacing = node.Element("MinorGridSpacing");
                if (minorGridSpacing != null)
                    m_minorGridSpacing = Util.TryParseInt(minorGridSpacing.Attribute("value").Value) ?? m_minorGridSpacing;

                var majorGridSpacing = node.Element("MajorGridSpacing");
                if (majorGridSpacing != null)
                    m_majorGridSpacing = Util.TryParseInt(majorGridSpacing.Attribute("value").Value) ?? m_majorGridSpacing;
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

                var showIds = new XElement("ShowIDs", new XAttribute("value", m_showIds));
                node.Add(showIds);

                var minorGridSpacing = new XElement("MinorGridSpacing", new XAttribute("value", m_minorGridSpacing));
                node.Add(minorGridSpacing);

                var majorGridSpacing = new XElement("MajorGridSpacing", new XAttribute("value", m_majorGridSpacing));
                node.Add(majorGridSpacing);
            }
        }

        public event Action ValueChanged;

        private bool m_showGrid = false;
        public bool ShowGrid
        {
            get { return m_showGrid; }
            set
            {
                if (value != m_showGrid)
                {
                    m_showGrid = value;
                    ValueChanged.Execute();
                }
            }
        }

        private bool m_snapToGrid = false;
        public bool SnapToGrid
        {
            get { return m_snapToGrid; }
            set
            {
                if (value != m_snapToGrid)
                {
                    m_snapToGrid = value;
                    ValueChanged.Execute();
                }
            }
        }

        private bool m_showIds = false;
        public bool ShowIds
        {
            get { return m_showIds; }
            set
            {
                if (value != m_showIds)
                {
                    m_showIds = value;
                    ValueChanged.Execute();
                }
            }
        }

        private int m_minorGridSpacing = 20;
        public int MinorGridSpacing
        {
            get { return m_minorGridSpacing; }
        }

        private int m_majorGridSpacing = 80;
        public int MajorGridSpacing
        {
            get { return m_majorGridSpacing; }
        }
    }
}
