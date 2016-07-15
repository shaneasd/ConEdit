using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Conversation
{
    /// <summary>
    /// Something has gone wrong as a consequence of badly written code as opposed to unexpected data or environment
    /// </summary>
    [Serializable]
    public class InternalLogicException : Exception
    {
        public InternalLogicException() : base("Internal logic error. This indicates a bug.") { }
        public InternalLogicException(string message) : base(message) { }
        public InternalLogicException(string message, Exception innerException) : base(message, innerException) { }
        protected InternalLogicException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
