using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class LocalizedStringParameter : Parameter<Id<LocalizedText>>, ILocalizedStringParameter
    {
        public LocalizedStringParameter(string name, Id<Parameter> id, ParameterType typeId, string defaultValue) : base(name, id, typeId, defaultValue) { }

        protected override bool DeserialiseValue(string value)
        {
            return Id<LocalizedText>.TryParse(value, out m_value);
        }

        protected override string InnerValueAsString()
        {
            if (m_value != null)
                return m_value.Serialized();
            else
                return Id<LocalizedText>.New().Serialized();
        }

        public override string DisplayValue(Func<Id<LocalizedText>, string> localize)
        {
            return localize(m_value);
        }

        protected override void DecorruptFromNull()
        {
            Value = Id<LocalizedText>.New();
        }
    }
}
