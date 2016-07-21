using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    public struct ConnectionDefinitionData
    {
        public ConnectionDefinitionData(UnorderedTuple2<Id<TConnectorDefinition>> connectors)
        {
            Connectors = connectors;
        }
        public UnorderedTuple2<Id<TConnectorDefinition>> Connectors { get; }
    }
}
