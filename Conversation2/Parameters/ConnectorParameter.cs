using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class ConnectorParameter : Parameter<ID<NodeTemp>>, IConnectorParameter //TODO: Pretty sure I don't want this
    {
        public ConnectorParameter(string name, ID<Parameter> id, ID<ParameterType> typeId)
            : base(name, id, null, typeId)
        {
        }

        protected override bool DeserialiseValue(string value)
        {
            return ID<NodeTemp>.TryParse(value, out m_value);
        }

        protected override string InnerValueAsString()
        {
            return m_value.Serialized();
        }

        public override string DisplayValue(Func<ID<LocalizedText>, string> localize)
        {
            //throw new Exception("This should never really need to be called"); //TODO: This should never really need to be called
            return m_value.Serialized();
        }
    }
}
