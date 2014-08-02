using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class BooleanParameter : Parameter<bool>, IBooleanParameter
    {
        public BooleanParameter(string name, ID<Parameter> id, ID<ParameterType> typeId)
            : base(name, id, false, typeId)
        {
        }

        protected override bool DeserialiseValue(string value)
        {
            return bool.TryParse(value, out m_value);
        }

        protected override string InnerValueAsString()
        {
            return m_value.ToString();
        }

        public override string DisplayValue(Func<ID<LocalizedText>, string> localize)
        {
            return m_value.ToString();
        }
    }
}
