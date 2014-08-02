using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    public class IntegerParameter : Parameter<int>, IIntegerParameter
    {
        public class Definition
        {
            public int? Max = null;
            public int? Min = null;
        }

        public IntegerParameter(string name, ID<Parameter> id, ID<ParameterType> typeId, Definition definition = null, int def = 0) : base(name, id, def, typeId)
        {
            m_definition = definition ?? new Definition();
        }

        protected override bool DeserialiseValue(string value)
        {
            if (!int.TryParse(value, out m_value))
                return false;
            //TODO: Should make the parameter corrupt or something
            //if (m_value > m_max)
            //    return false;
            //if (m_value < m_min)
            //    return false;
            return true;
        }

        protected override string InnerValueAsString()
        {
            return m_value.ToString();
        }

        private Definition m_definition;
        public int Max
        {
            get { return m_definition.Max ?? int.MaxValue; }
        }

        public int Min
        {
            get { return m_definition.Min ?? int.MinValue; }
        }

        public override string DisplayValue(Func<ID<LocalizedText>, string> localize)
        {
            return m_value.ToString();
        }
    }
}
