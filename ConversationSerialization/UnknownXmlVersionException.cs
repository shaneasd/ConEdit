using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Conversation.Serialization
{
    [Serializable]
    public class UnknownXmlVersionException : Exception
    {
        public UnknownXmlVersionException(string message) : base(message)
        {
        }

        public UnknownXmlVersionException() : base()
        {
        }

        public UnknownXmlVersionException(string message, Exception inner) : base(message, inner)
        {
        }

        protected UnknownXmlVersionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
