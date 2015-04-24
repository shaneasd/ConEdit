using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ConversationEditor
{
    public class JustStringConfigParameter : ConfigParameter<string>
    {
        public override void Load(XElement root)
        {
            var b = root.Attribute("value");
            if (b != null)
                InnerValue = b.Value;
        }

        public override void Write(XElement root)
        {
            root.Add(new XAttribute("value", InnerValue));
        }
    }

    public class ConfigParameterString : ConfigParameter<string>
    {
        string m_name;
        public ConfigParameterString(string name)
        {
            m_name = name;
            InnerValue = "";
        }

        public override void Load(XElement root)
        {
            var a = root.Element(m_name);
            if (a != null)
            {
                var b = a.Attribute("value");
                if (b != null)
                    InnerValue = b.Value;
            }
        }

        public override void Write(XElement root)
        {
            root.Add(new XElement(m_name, new XAttribute("value", InnerValue)));
        }
    }
}
