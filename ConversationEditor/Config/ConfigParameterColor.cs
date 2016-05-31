using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml.Linq;
using System.Globalization;

namespace ConversationEditor
{
    internal class ConfigParameterColor : ConfigParameter<Color>
    {
        string m_name;
        public ConfigParameterColor(string name)
        {
            m_name = name;
            InnerValue = Color.Black;
        }

        public override void Load(XElement root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));
            var a = root.Element(m_name);
            if (a != null)
            {
                var b = a.Attribute("value");
                if (b != null)
                    InnerValue = Color.FromArgb(int.Parse(b.Value, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            }
        }

        public override void Write(XElement root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));
            var @string = InnerValue.ToArgb().ToString("X", CultureInfo.InvariantCulture);
            root.Add(new XElement(m_name, new XAttribute("value", @string)));
        }
    }
}
