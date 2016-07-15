using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Utilities;
using System.Drawing;
using System.Globalization;

namespace ConversationEditor
{
    internal class ColorsConfig : IConfigParameter
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

        private static bool ReadColor(XElement root, string name, ref Color value)
        {
            var a = root.Element(name);
            if (a != null)
            {
                var b = a.Attribute("value");
                if (b != null)
                {
                    value = Color.FromArgb(int.Parse(b.Value, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                    return true;
                }
            }
            return false;
        }

        private static void WriteColor(XContainer root, string name, Color value)
        {
            var @string = value.ToArgb().ToString("X", CultureInfo.InvariantCulture);
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

