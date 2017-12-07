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
            public Definition(decimal? min, decimal? max)
            {
                Max = max;
                Min = min;
            }
            public decimal? Min { get; private set; }
            public decimal? Max { get; private set; }
        }
        public DecimalParameter(string name, Id<Parameter> id, ParameterType typeId, Definition definition, string defaultValue)
            : base(name, id, typeId, defaultValue, StaticDeserialize(definition, defaultValue))
        {
            m_definition = definition;
        }

        protected override Tuple<decimal, bool> DeserializeValueInner(string value)
        {
            return StaticDeserialize(m_definition, value);
        }

        private static Tuple<decimal, bool> StaticDeserialize(Definition definition, string value)
        {
            decimal val;
            if (!decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                return Tuple.Create(0.0m, true);
            else
                return Tuple.Create(val, !StaticIsValid(definition, val));
        }

        protected override bool ValueValid(decimal value)
        {
            return StaticIsValid(m_definition, value);
        }

        private static bool StaticIsValid(Definition definition, decimal value)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (value > definition.Max)
                return false;
            if (value < definition.Min)
                return false;
            return true;
        }

        protected override string InnerValueAsString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public decimal Max
        {
            get { return m_definition.Max ?? decimal.MaxValue; }
        }

        public decimal Min
        {
            get { return m_definition.Min ?? decimal.MinValue; }
        }

        public override string DisplayValue(Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localize)
        {
            return Value.ToString(CultureInfo.CurrentCulture);
        }
    }
}
