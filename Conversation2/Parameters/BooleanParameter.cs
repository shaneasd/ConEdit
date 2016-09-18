using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class BooleanParameter : Parameter<bool>, IBooleanParameter
    {
        public static ParameterType ParameterType { get; } = ParameterType.Parse("3a98d216-7427-45ef-a3ca-cd47431835a0");

        public BooleanParameter(string name, Id<Parameter> id, string defaultValue)
            : base(name, id, ParameterType, defaultValue, StaticDeserialize(defaultValue))
        {
        }

        protected override Tuple<bool, bool> DeserializeValueInner(string value)
        {
            return StaticDeserialize(value);
        }

        private static Tuple<bool, bool> StaticDeserialize(string value)
        {
            bool val = false;
            var isValid = bool.TryParse(value, out val);
            return Tuple.Create(val, !isValid);
        }

        protected override string InnerValueAsString()
        {
            return Value.ToString();
        }

        public override string DisplayValue(Func<Id<LocalizedText>, string> localize)
        {
            return Value.ToString();
        }

        protected override bool ValueValid(bool value)
        {
            return true;
        }
    }
}
