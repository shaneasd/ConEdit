using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace Conversation
{
    public class FilePathParameter: Parameter<FileInfo>, IFilePathParameter
    {
        public FilePathParameter(string name, string typeName = "FilePath") : base(name, new FileInfo("."), typeName) { }
        public override void DeserialiseValue(XElement node)
        {
            m_value = new FileInfo(node.Attribute("value").Value);
            if (!m_value.Exists)
                throw new Exception();
        }

        public override void WriteValueTo(XElement node)
        {
            node.Add(new XAttribute("value", m_value.ToString()));
        }

        public override Parameter Copy()
        {
            return new FilePathParameter(Name, TypeName) { Value = Value };
        }
    }
}
