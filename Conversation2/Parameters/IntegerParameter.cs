using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Globalization;

namespace Conversation
{
    public class IntegerParameter : Parameter<int>, IIntegerParameter
    {
        public class Definition
        {
            public Definition(int? max, int? min)
            {
                Max = max;
                Min = min;
            }
            public int? Max { get; private set; }
            public int? Min { get; private set; }
        }

        public IntegerParameter(string name, Id<Parameter> id, ParameterType typeId, Definition definition, string defaultValue)
            : base(name, id, typeId, defaultValue)
        {
            m_definition = definition ?? new Definition(null, null);
            TryDecorrupt(); //The first setting will always be corrupt because definition is null
        }

        protected override bool DeserialiseValue(string value)
        {
            if (!int.TryParse(value, out m_value))
                return false;
            //m_definition is not set the first time this is called (within the parent constructor)
            if (m_definition == null)
                return false;
            if (m_value > m_definition.Max)
                return false;
            if (m_value < m_definition.Min)
                return false;
            return true;
        }

        protected override string InnerValueAsString()
        {
            return m_value.ToString(CultureInfo.InvariantCulture);
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

        public override string DisplayValue(Func<Id<LocalizedText>, string> localize)
        {
            return m_value.ToString(CultureInfo.CurrentCulture);
        }
    }
}
