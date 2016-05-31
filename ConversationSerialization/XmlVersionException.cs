using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation.Serialization
{
    [Serializable()]
    public class XmlVersionException : Exception
    {
        public XmlVersionException() : this("Unknown xml version")
        {
        }

        public XmlVersionException(string message)
            : base(message)
        {
        }
        public XmlVersionException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
        protected XmlVersionException(System.Runtime.Serialization.SerializationInfo info,
           System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }  
}
