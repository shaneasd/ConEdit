using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conversation
{
    /// <summary>
    /// Criteria by which a connection between two node connectors might be rejected
    /// </summary>
    [Flags]
    public enum ConnectionConsiderations
    {
        None = 0,

        /// <summary>
        /// Two outputs which have the same parent node
        /// </summary>
        SameNode = 1,

        /// <summary>
        /// Two outputs which are already connected
        /// </summary>
        RedundantConnection = 2,

        /// <summary>
        /// Two outputs which cannot be connected according to the custom connection rules
        /// </summary>
        RuleViolation = 4,
    }
}
