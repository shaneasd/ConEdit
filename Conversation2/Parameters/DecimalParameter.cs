using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Globalization;

namespace Conversation
{
    public class DecimalParameter : Parameter<decimal>, IDecimalParameter
    {
        private Definition m_definition;
        public class Definition
        {
            public decimal? Min = null;
            public decimal? Max = null;
        }
        public DecimalParameter(string name, Id<Parameter> id, ParameterType typeId, Definition definition, string defaultValue)
            : base(name, id, typeId,  defaultValue)
        {
            m_definition = definition ?? new Definition();
            TryDecorrupt(); //The first setting will always be corrupt because definition is null
        }

        protected override bool DeserialiseValue(string value)
        {
            if (!decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out m_value))
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

        public decimal Max
        {
            get { return m_definition.Max ?? decimal.MaxValue; }
        }

        public decimal Min
        {
            get { return m_definition.Min ?? decimal.MinValue; }
        }

        public override string DisplayValue(Func<Id<LocalizedText>, string> localize)
        {
            return m_value.ToString(CultureInfo.CurrentCulture);
        }
    }
}
