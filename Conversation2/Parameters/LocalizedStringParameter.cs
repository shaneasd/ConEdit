using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class LocalizedStringParameter : Parameter<Id<LocalizedText>>, ILocalizedStringParameter
    {
        public static ParameterType ParameterType { get; } = ParameterType.Parse("c72e8222-3e10-4995-b32b-5b3ebd8e0f20");

        public LocalizedStringParameter(string name, Id<Parameter> id) : base(name, id, ParameterType, null, new Tuple<Id<LocalizedText>, bool>(null, true)) { }

        protected override Tuple<Id<LocalizedText>, bool> DeserializeValueInner(string value)
        {
            return StaticDeserialize(value);
        }

        private static Tuple<Id<LocalizedText>, bool> StaticDeserialize(string value)
        {
            Id<LocalizedText> val = null;
            var result = Id<LocalizedText>.TryParse(value, out val);
            return Tuple.Create(val, !result);
        }

        protected override string InnerValueAsString()
        {
            return Value.Serialized();
        }

        public override string DisplayValue(Func<Id<LocalizedText>, string> localize)
        {
            return localize(Value);
        }

        protected override bool ValueValid(Id<LocalizedText> value)
        {
            return value != null;
        }
    }
}
