using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Globalization;
using System.Diagnostics.Contracts;

namespace Conversation
{
    public class IntegerParameter : Parameter<int>, IIntegerParameter
    {
        public class Definition
        {
            public Definition(int? min, int? max)
            {
                if (max < min)
                    throw new ArgumentOutOfRangeException(nameof(max), "max must equal or exceed min");
                Max = max;
                Min = min;
            }
            public int? Max { get; private set; }
            public int? Min { get; private set; }
        }

        private Func<Definition> m_definition;

        public IntegerParameter(string name, Id<Parameter> id, ParameterType typeId, Func<Definition> definition, string defaultValue)
            : base(name, id, typeId, defaultValue, StaticDeserialize(definition(), defaultValue))
        {
            m_definition = definition;
        }

        public IntegerParameter(string name, Id<Parameter> id, ParameterType typeId, Definition definition, string defaultValue)
            : this(name, id, typeId, () => definition, defaultValue)
        {
        }


        protected override Tuple<int, bool> DeserializeValueInner(string value)
        {
            return StaticDeserialize(m_definition(), value);
        }

        private static Tuple<int, bool> StaticDeserialize(Definition definition, string value)
        {
            int val;
            if (!int.TryParse(value, out val))
                return Tuple.Create(0, true);
            else
                return Tuple.Create(val, !StaticValueValid(definition, val));
        }

        protected override bool ValueValid(int value)
        {
            return StaticValueValid(m_definition(), value);
        }

        private static bool StaticValueValid(Definition definition, int value)
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

        public int Max
        {
            get { return m_definition().Max ?? int.MaxValue; }
        }

        public int Min
        {
            get { return m_definition().Min ?? int.MinValue; }
        }

        public override string DisplayValue(Func<Id<LocalizedText>, string> localize)
        {
            return Value.ToString(CultureInfo.CurrentCulture);
        }
    }
}
