using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class BooleanParameter : Parameter<bool>, IBooleanParameter
    {
        public static readonly ParameterType ParameterType = ParameterType.Parse("3a98d216-7427-45ef-a3ca-cd47431835a0");

        //TODO: Why is it possible to specify a type for a boolean parameter? Isn't there really only one possible element in the class of boolean types?
        //      Same applies to all parameters that aren't in TypeDefinitionNodeIds.All
        //      It looks like StringParameter already does this properly.
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
