using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ConversationEditor
{
    public class Condition
    {
        public readonly string Name;
        private readonly IEnumerable<ConditionParameter> m_parameters;
        public IEnumerable<ConditionParameter> Parameters
        {
            get
            {
                return m_parameters;
            }
        }
        public Condition(string name, params ConditionParameter[] parameters)
        {
            Name = name;
            m_parameters = parameters;
        }

        internal Condition Clone()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class ConditionParameter
    {
        public readonly string Name;
        public ConditionParameter(string name)
        {
            Name = name;
        }
        public abstract void AddToEditor(ConditionEditor editor);
        public abstract ConditionParameter Clone();
        public abstract void WriteTo(XmlWriter writer);
        public abstract void DeserialiseValue(DataSource datasource, string value);
    }

    public abstract class ConditionParameter<T> : ConditionParameter
    {
        public T Value;
        public ConditionParameter(string name, T value) : base(name)
        {
            Value = value;
        }
        public override string ToString()
        {
            return Value.ToString();
        }

        public override void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement("ConditionParameter");
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("Value", Value.ToString());
            writer.WriteEndElement();
        }
    }



}
