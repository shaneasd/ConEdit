using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class LocalizedStringParameter : Parameter<ID<LocalizedText>>, ILocalizedStringParameter
    {
        public LocalizedStringParameter(string name, ID<Parameter> id, ID<ParameterType> typeId) : base(name, id, new ID<LocalizedText>(), typeId) { }

        protected override bool DeserialiseValue(string value)
        {
            return ID<LocalizedText>.TryParse(value, out m_value);
        }

        protected override string InnerValueAsString()
        {
            return m_value.Serialized();
        }

        public override string DisplayValue(Func<ID<LocalizedText>, string> localize)
        {
            return localize(m_value) ?? "Missing Localization";
        }
    }
}
