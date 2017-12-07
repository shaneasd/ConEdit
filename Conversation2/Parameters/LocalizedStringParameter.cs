using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class LocalizedStringParameter : Parameter<Id<LocalizedText>>, ILocalizedStringParameter
    {
        public LocalizedStringParameter(string name, Id<Parameter> id, ParameterType typeId) : base(name, id, typeId, "", new Tuple<Id<LocalizedText>, bool>(null, true))
        {
        }

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

        public override string DisplayValue(Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localize)
        {
            return localize(Id<LocalizedStringType>.FromGuid(TypeId.Guid), Value);
        }

        protected override bool ValueValid(Id<LocalizedText> value)
        {
            return value != null;
        }
    }
}
