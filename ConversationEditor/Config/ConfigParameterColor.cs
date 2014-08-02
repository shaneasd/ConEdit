﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml.Linq;

namespace ConversationEditor
{
    public class ConfigParameterColor : ConfigParameter<Color>
    {
        string m_name;
        public ConfigParameterColor(string name)
        {
            m_name = name;
            InnerValue = Color.Black;
        }

        public override void Load(XElement root)
        {
            var a = root.Element(m_name);
            if (a != null)
            {
                var b = a.Attribute("value");
                if (b != null)
                    InnerValue = Color.FromArgb(int.Parse(b.Value, System.Globalization.NumberStyles.HexNumber));
            }
        }

        public override void Write(XElement root)
        {
            var @string = InnerValue.ToArgb().ToString("X");
            root.Add(new XElement(m_name, new XAttribute("value", @string)));
        }
    }
}
