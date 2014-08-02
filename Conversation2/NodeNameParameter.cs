using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class NodeNameParameter : Parameter<string>, IStringParameter
    {
        public NodeNameParameter(string value)
            : base("FunctionName", value)
        {
        }
        public override void DeserialiseValue(string value)
        {
            m_value = value;
        }

        public string Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }
    }
}
