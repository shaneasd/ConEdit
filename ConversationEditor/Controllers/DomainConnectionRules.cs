using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

namespace ConversationEditor
{
    public class DomainConnectionRules : IConnectionRules
    {
        static readonly HashSet<UnorderedTuple2<ID<TConnectorDefinition>>> acceptablePairings = new HashSet<UnorderedTuple2<ID<TConnectorDefinition>>>()
        {
            UnorderedTuple.Make(DomainIDs.ENUM_OUTPUT_DEFINITION.Id, DomainIDs.ENUM_VALUE_OUTPUT_DEFINITION.Id), //Connect enumeration declarations with possible values
            
            UnorderedTuple.Make(DomainIDs.CONFIG_OUTPUT_DEFINITION.Id, DomainIDs.NODE_OUTPUT_CONFIG_DEFINITION.Id), //Connect node declaration with config
            UnorderedTuple.Make(DomainIDs.CONFIG_OUTPUT_DEFINITION.Id, DomainIDs.PARAMETER_CONFIG_CONNECTOR_DEFINITION.Id), //Connect parameter declarations with config

            UnorderedTuple.Make(DomainIDs.PARAMETER_OUTPUT_DEFINITION.Id, DomainIDs.NODE_OUTPUT_PARAMETERS_DEFINITION.Id), //Connect node declaration with parameters for that node type
            UnorderedTuple.Make(DomainIDs.CONNECTOR_OUTPUT_DEFINITION.Id, DomainIDs.NODE_OUTPUT_CONNECTORS_DEFINITION.Id), //Connect node declaration with connectors for that node type

            UnorderedTuple.Make(DomainIDs.CONNECTOR_DEFINITION_OUTPUT_DEFINITION.Id, DomainIDs.PARAMETER_OUTPUT_DEFINITION.Id), //Connect connector declaration with parameters for that connector type
            //UnorderedTuple.Make(DomainIDs.CONNECTOR_DEFINITION_CONNECTION_DEFINITION.Id, DomainIDs.CONNECTION_DEFINITION_CONNECTOR.Id), //Connect connector declaration with connection declaration
        };

        public static readonly DomainConnectionRules Instance = new DomainConnectionRules();

        public bool CanConnect(ID<TConnectorDefinition> a, ID<TConnectorDefinition> b)
        {
            return acceptablePairings.Contains(UnorderedTuple.Make(a, b));
        }
    }
}
