using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ConversationEditor
{
    public class ConfigParameterUint : ConfigParameter<uint>
    {
        string m_name;
        public ConfigParameterUint(string name, uint @default)
        {
            m_name = name;
            InnerValue = @default;
        }

        public override void Load(XElement root)
        {
            uint val;
            var b = root.Attribute("value");
            if (b != null)
                if (uint.TryParse(b.Value, out val))
                    InnerValue = val;
        }

        public override void Write(XElement root)
        {
            var @string = InnerValue.ToString();
            root.Add(new XElement(m_name, new XAttribute("value", @string)));
        }
    }
}
