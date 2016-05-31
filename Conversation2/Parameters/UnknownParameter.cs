using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class UnknownParameter : Parameter<string>
    {
        public static readonly ParameterType s_typeId = ParameterType.Parse("c7b4be3f-add0-4d58-9fe4-ae19c8e95a35");

        public UnknownParameter(Id<Parameter> id, string value)
            : base("Unknown parameter " + id.Guid.ToString().Substring(0, 8), id, s_typeId,  value)
        {
            m_value = value;
        }

        public override string DisplayValue(Func<Id<LocalizedText>, string> localize)
        {
            return m_value;
        }

        protected override string InnerValueAsString()
        {
            return m_value;
        }

        protected override bool DeserialiseValue(string value)
        {
            m_value = value;
            return true;
        }
    }
}
