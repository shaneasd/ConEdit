using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class DomainData
    {
        public List<NodeTypeData> NodeTypes = new List<NodeTypeData>();
        public List<DynamicEnumerationData> DynamicEnumerations = new List<DynamicEnumerationData>();
        public List<EnumerationData> Enumerations = new List<EnumerationData>();
        public List<DecimalData> Decimals = new List<DecimalData>();
        public List<IntegerData> Integers = new List<IntegerData>();
        public List<NodeData> Nodes = new List<NodeData>();
        public List<ConnectorDefinitionData> Connectors = new List<ConnectorDefinitionData>();
    }
}
