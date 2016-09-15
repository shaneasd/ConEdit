using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class StringParameter : Parameter<string>, IStringParameter
    {
        public static ParameterType ParameterType { get; } = ParameterType.Parse("7ca91556-5526-4c5c-b565-00aff5ae85ce");

        public StringParameter(string name, Id<Parameter> id) : base(name, id, ParameterType, null, StaticDeserialize(null)) { }
        public StringParameter(string name, Id<Parameter> id, string defaultValue) : base(name, id, ParameterType, defaultValue, StaticDeserialize(defaultValue)) { }

        protected override Tuple<string, bool> DeserializeValueInner(string value)
        {
            return StaticDeserialize(value);
        }

        private static Tuple<string, bool> StaticDeserialize(string value)
        {
            return Tuple.Create(value, value == null);
        }

        protected override string InnerValueAsString()
        {
            return Value;
        }

        public override string DisplayValue(Func<Id<LocalizedText>, string> localize)
        {
            return Value;
        }

        protected override bool ValueValid(string value)
        {
            return value != null;
        }
    }
}
