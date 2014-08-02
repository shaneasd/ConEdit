using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ConversationEditor
{
    //public class ConfigParameterStringList : ConfigParameter<List<string>>
    //{
    //    string m_name;
    //    public ConfigParameterStringList(string name)
    //    {
    //        m_name = name;
    //        InnerValue = new List<string>();
    //    }

    //    public override void Load(XElement root)
    //    {
    //        var a = root.Element(m_name);
    //        InnerValue = a.Elements("Element").Select(e => e.Attribute("value").Value).ToList();
    //    }

    //    public override void Write(XElement root)
    //    {
    //        var elements = InnerValue.Select(s => new XElement("Element", new XAttribute("value", s)));
    //        root.Add(new XElement(m_name, elements.ToArray()));
    //    }
    //}
}
