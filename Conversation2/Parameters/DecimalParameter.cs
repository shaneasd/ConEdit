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
        public DecimalParameter(string name, ID<Parameter> id, ID<ParameterType> typeId, Definition definition = null, decimal def = 0)
            : base(name, id, def, typeId)
        {
            m_definition = definition ?? new Definition();
        }

        protected override bool DeserialiseValue(string value)
        {
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out m_value);
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

        public override string ToString()
        {
            return m_value.ToString("F" + Misc.GetDecimalPlaces(m_value));
        }

        public override string DisplayValue(Func<ID<LocalizedText>, string> localize)
        {
            return m_value.ToString();
        }
    }
}
