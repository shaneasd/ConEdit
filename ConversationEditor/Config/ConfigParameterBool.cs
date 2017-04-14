using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Globalization;

namespace ConversationEditor
{
    public class ConfigParameterBool : ConfigParameter<bool>
    {
        string m_name;
        public ConfigParameterBool(string name, bool @default)
        {
            m_name = name;
            InnerValue = @default;
        }

        public override void Load(XElement root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));
            bool val;
            var n = root.Element(m_name);
            if (n != null)
            {
                var b = n.Attribute("value");
                if (b != null)
                    if (bool.TryParse(b.Value, out val))
                        InnerValue = val;
            }
        }

        public override void Write(XElement root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));
            var @string = InnerValue.ToString(CultureInfo.InvariantCulture);
            root.Add(new XElement(m_name, new XAttribute("value", @string)));
        }
    }
}
