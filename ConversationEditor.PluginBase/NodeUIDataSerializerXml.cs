using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Conversation;
using System.Xml.Linq;
using System.Drawing;
using System.Globalization;

namespace ConversationEditor
{
    public class NodeUIDataSerializerXml : ISerializerDeserializerXml<NodeUIData, NodeUIData>
    {
        public static NodeUIDataSerializerXml Instance = new NodeUIDataSerializerXml();
        private NodeUIDataSerializerXml()
        {
        }

        public void Write(IGui data, XElement node)
        {
            Write(new NodeUIData() { Area = data.Area }, node);
        }

        public void Write(NodeUIData data, XElement node)
        {
            WriteArea(data.Area, node);
        }

        public static void WriteArea(RectangleF area, XElement node)
        {
            XAttribute x = new XAttribute("X", area.X.ToString(CultureInfo.InvariantCulture));
            XAttribute y = new XAttribute("Y", area.Y.ToString(CultureInfo.InvariantCulture));
            XAttribute width = new XAttribute("Width", area.Width.ToString(CultureInfo.InvariantCulture));
            XAttribute height = new XAttribute("Height", area.Height.ToString(CultureInfo.InvariantCulture));
            node.Add(new XElement("Area", x, y, width, height));
        }

        public NodeUIData Read(XElement node)
        {
            return new NodeUIData { Area = ReadArea(node) };
        }

        public static RectangleF ReadArea(XElement node)
        {
            node = node.Element("Area");
            float x = float.Parse(node.Attribute("X").Value, CultureInfo.InvariantCulture);
            float y = float.Parse(node.Attribute("Y").Value, CultureInfo.InvariantCulture);
            float width = float.Parse(node.Attribute("Width").Value, CultureInfo.InvariantCulture);
            float height = float.Parse(node.Attribute("Height").Value, CultureInfo.InvariantCulture);
            return new System.Drawing.RectangleF(x, y, width, height);
        }
    }
}
