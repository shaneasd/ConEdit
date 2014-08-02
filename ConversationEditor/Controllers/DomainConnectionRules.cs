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
        static readonly HashSet<UnordererTuple2<ID<TConnectorDefinition>>> acceptablePairings = new HashSet<UnordererTuple2<ID<TConnectorDefinition>>>()
        {
            UnordererTuple.Make(DomainIDs.ENUM_OUTPUT_DEFINITION.Id, DomainIDs.ENUM_VALUE_OUTPUT_DEFINITION.Id), //Connect enumeration declarations with possible values
            
            UnordererTuple.Make(DomainIDs.CONFIG_OUTPUT_DEFINITION.Id, DomainIDs.NODE_OUTPUT_CONFIG_DEFINITION.Id), //Connect node declaration with config
            UnordererTuple.Make(DomainIDs.PARAMETER_OUTPUT_DEFINITION.Id, DomainIDs.NODE_OUTPUT_PARAMETERS_DEFINITION.Id), //Connect node declaration with parameters for that node type
            UnordererTuple.Make(DomainIDs.CONNECTOR_OUTPUT_DEFINITION.Id, DomainIDs.NODE_OUTPUT_CONNECTORS_DEFINITION.Id), //Connect node declaration with connectors for that node type

            UnordererTuple.Make(DomainIDs.CONNECTOR_DEFINITION_OUTPUT_DEFINITION.Id, DomainIDs.PARAMETER_OUTPUT_DEFINITION.Id), //Connect connector declaration with parameters for that connector type
        };

        public static readonly DomainConnectionRules Instance = new DomainConnectionRules();

        public bool CanConnect(ID<TConnectorDefinition> a, ID<TConnectorDefinition> b)
        {
            return acceptablePairings.Contains(UnordererTuple.Make(a, b));
        }
    }
}
