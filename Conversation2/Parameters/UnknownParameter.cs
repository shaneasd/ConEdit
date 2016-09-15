using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class UnknownParameter : Parameter<string>
    {
        public new static ParameterType TypeId { get; } = ParameterType.Parse("c7b4be3f-add0-4d58-9fe4-ae19c8e95a35");

        public UnknownParameter(Id<Parameter> id, string value)
            : base("Unknown parameter " + id.Guid.ToString().Substring(0, 8), id, UnknownParameter.TypeId, value, StaticDeserialize(value))
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
        }

        public override string DisplayValue(Func<Id<LocalizedText>, string> localize)
        {
            return Value;
        }

        protected override string InnerValueAsString()
        {
            return Value;
        }

        protected override Tuple<string, bool> DeserializeValueInner(string value)
        {
            return StaticDeserialize(value);
        }

        private static Tuple<string, bool> StaticDeserialize(string value)
        {
            return Tuple.Create(value, value == null);
        }

        protected override bool ValueValid(string value)
        {
            //Can't change the value of an unknown parameter. Either make it known or delete it.
            return false;
        }
    }
}
