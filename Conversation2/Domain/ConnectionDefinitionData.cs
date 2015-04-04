using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    public struct ConnectionDefinitionData
    {
        public ConnectionDefinitionData(UnorderedTuple2<ID<TConnectorDefinition>> connectors)
        {
            Connectors = connectors;
        }
        public readonly UnorderedTuple2<ID<TConnectorDefinition>> Connectors;
    }
}
