using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class LocalizedStringParameter : Parameter<ID<LocalizedText>>, ILocalizedStringParameter
    {
        public LocalizedStringParameter(string name, ID<Parameter> id, ParameterType typeId, string defaultValue = null) : base(name, id, typeId, defaultValue) { }

        protected override bool DeserialiseValue(string value)
        {
            return ID<LocalizedText>.TryParse(value, out m_value);
        }

        protected override string InnerValueAsString()
        {
            if (m_value != null)
                return m_value.Serialized();
            else
                return ID<LocalizedText>.New().Serialized();
        }

        public override string DisplayValue(Func<ID<LocalizedText>, string> localize)
        {
            return localize(m_value);
        }

        protected override void DecorruptFromNull()
        {
            Value = ID<LocalizedText>.New();
        }
    }
}
