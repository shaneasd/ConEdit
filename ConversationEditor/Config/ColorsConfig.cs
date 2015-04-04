using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Utilities;
using System.Drawing;

namespace ConversationEditor
{
    public class ColorsConfig : IConfigParameter
    {
        public void Load(XElement root)
        {
            var node = root.Element("ColorScheme");
            if (node != null)
            {
                ReadColor(node, "Connectors", ref m_connectorColor);
            }
        }

        public void Write(XElement root)
        {
            var node = new XElement("ColorScheme");
            root.Add(node);
            WriteColor(node, "Connectors", ConnectorColor);
        }

        public event Action ValueChanged;

        private bool ReadColor(XElement root, string name, ref Color value)
        {
            var a = root.Element(name);
            if (a != null)
            {
                var b = a.Attribute("value");
                if (b != null)
                {
                    value = Color.FromArgb(int.Parse(b.Value, System.Globalization.NumberStyles.HexNumber));
                    return true;
                }
            }
            return false;
        }

        public void WriteColor(XElement root, string name, Color value)
        {
            var @string = value.ToArgb().ToString("X");
            root.Add(new XElement(name, new XAttribute("value", @string)));
        }

        private Color m_connectorColor = Color.Black;
        public Color ConnectorColor
        {
            get { return m_connectorColor; }
            set
            {
                m_connectorColor = value;
                ValueChanged.Execute();
            }
        }
    }
}

