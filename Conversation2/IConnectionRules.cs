using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conversation
{
    public interface IConnectionRules
    {
        /// <summary>
        /// Can a connection of type 'a' connect to a connection of type 'b'
        /// This must be a symmetric relationship. i.e. CanConnect(x,y) = CanConnect(y,x)
        /// </summary>
        bool CanConnect(Id<TConnectorDefinition> a, Id<TConnectorDefinition> b);
    }
}
