using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ConversationEditor
{
    public abstract class ConversationSerializer
    {
        public abstract void Write(ConditionGraphics condition);
        public abstract void Write(SpielGraphics spiel);
        public abstract void Write(StartGraphics start);
        public abstract void Write(TerminatorGraphics terminator);
        public abstract void Write(ErrorGraphics error);
        public abstract void Write(TodoGraphics todo);
    }

    public class ConversationSeralizerGUI : ConversationSerializer
    {        
        private XmlWriter m_writer;
        public ConversationSeralizerGUI(XmlWriter writer)
        {
            m_writer = writer;
        }

        private void WriteArea(NodeGraphics ng)
        {
            m_writer.WriteStartElement("Area");
            m_writer.WriteAttributeString("X", ng.Area.X.ToString());
            m_writer.WriteAttributeString("Y", ng.Area.Y.ToString());
            m_writer.WriteAttributeString("Width", ng.Area.Width.ToString());
            m_writer.WriteAttributeString("Height", ng.Area.Height.ToString());
            m_writer.WriteEndElement();
        }
        private static System.Drawing.Rectangle ReadArea(XElement node)
        {
            node = node.Element("Area");
            int x = int.Parse(node.Attribute("X").Value);
            int y= int.Parse(node.Attribute("Y").Value);
            int width = int.Parse(node.Attribute("Width").Value);
            int height = int.Parse(node.Attribute("Height").Value);
            return new System.Drawing.Rectangle(x, y, width, height);
        }


        private static ConditionGraphics ReadCondition(XElement node, DataSource datasource)
        {
            //TODO: Shouldn't be creating this condition. We should be reading it out of the datasource
            Condition condition = datasource.GetCondition(node.Attribute("Name").Value);
            foreach ( ConditionParameter p in condition.Parameters)
                p.DeserialiseValue(datasource, node.Elements("ConditionParameter").Single(n=>n.Attribute("Name").Value == p.Name).Attribute("Value").Value);
            return new ConditionGraphics(datasource, condition, ReadArea(node).Location, ReadArea(node).Size);
        }
        public override void Write(ConditionGraphics condition)
        {
            m_writer.WriteStartElement("Condition");
            m_writer.WriteAttributeString("Name", condition.Name);
            WriteArea(condition);
            foreach (var parameter in condition.Parameters)
            {
                parameter.WriteTo(m_writer);
            }
            m_writer.WriteEndElement();
        }

        public static IEnumerable<NodeGraphics> ReadNodes(XElement root, DataSource datasource)
        {
            var start = root.Elements("Start").Select(ReadStart);
            var conditions = root.Elements("Condition").Select(c=>ReadCondition(c, datasource));
            var terminators = root.Elements("Terminator").Select(ReadTerminator);
            var spiels = root.Elements("Spiel").Select(ReadSpiel);
            var todos = root.Elements("Todo").Select(ReadTodo);
            var errors = root.Elements("Error").Select(ReadError);

            return Utils.Concat<NodeGraphics>(start, conditions, terminators, spiels, todos, errors);
        }

        private static SpielGraphics ReadSpiel(XElement node)
        {
            var area = ReadArea(node);
            string text = node.Attribute("Text").Value;
            Spiel spiel = new Spiel(text);
            return new SpielGraphics(spiel, area.Location, area.Size);
        }
        public override void Write(SpielGraphics spiel)
        {
            m_writer.WriteStartElement("Spiel");
            m_writer.WriteAttributeString("Text", spiel.Text);
            WriteArea(spiel);
            //TODO - I think there's more stuff that should go here.
            m_writer.WriteEndElement();
        }

        private static StartGraphics ReadStart(XElement node)
        {
            return new StartGraphics(ReadArea(node).Location);
        }
        public override void Write(StartGraphics start)
        {
            m_writer.WriteStartElement("Start");
            WriteArea(start);
            //TODO - I think there's more stuff that should go here.
            m_writer.WriteEndElement();
        }

        private static TerminatorGraphics ReadTerminator(XElement node)
        {
            return new TerminatorGraphics(ReadArea(node).Location);
        }
        public override void Write(TerminatorGraphics terminator)
        {
            m_writer.WriteStartElement("Terminator");
            WriteArea(terminator);
            //TODO - I think there's more stuff that should go here.
            m_writer.WriteEndElement();
        }

        private static ErrorGraphics ReadError(XElement node)
        {
            return new ErrorGraphics(ReadArea(node).Location);
        }
        public override void Write(ErrorGraphics error)
        {
            m_writer.WriteStartElement("Error");
            WriteArea(error);
            //TODO - I think there's more stuff that should go here.
            m_writer.WriteEndElement();
        }

        private static TodoGraphics ReadTodo(XElement node)
        {
            return new TodoGraphics(ReadArea(node).Location);
        }
        public override void Write(TodoGraphics todo)
        {
            m_writer.WriteStartElement("Todo");
            WriteArea(todo);
            //TODO - I think there's more stuff that should go here.
            m_writer.WriteEndElement();
        }
    }
}
