using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Globalization;
using System.Diagnostics.Contracts;

namespace ConversationEditor
{
    //Doesn't look like anything's using this class at the moment
    internal class ConfigParameterUint : ConfigParameter<uint>
    {
        string m_name;
        public ConfigParameterUint(string name, uint @default)
        {
            m_name = name;
            InnerValue = @default;
        }

        public override void Load(XElement root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));
            var b = root.Attribute("value");
            if (b != null)
                if (uint.TryParse(b.Value, out uint val))
                    InnerValue = val;
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
