using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class StringParameter : Parameter<string>, IStringParameter
    {
        public static readonly ID<ParameterType> PARAMETER_TYPE = ID<ParameterType>.Parse("7ca91556-5526-4c5c-b565-00aff5ae85ce");

        public StringParameter(string name, ID<Parameter> id, ID<ParameterType> typeId) : base(name, id, "", typeId) { }

        protected override bool DeserialiseValue(string value)
        {
            m_value = value;
            return true;
        }

        protected override string InnerValueAsString()
        {
            return m_value;
        }

        public override string DisplayValue(Func<ID<LocalizedText>, string> localize)
        {
            return m_value;
        }
    }
}
