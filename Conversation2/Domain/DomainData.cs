using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public interface IDomainData
    {
        IReadOnlyList<NodeTypeData> NodeTypes { get; }
        IReadOnlyList<DynamicEnumerationData> DynamicEnumerations { get; }
        IReadOnlyList<LocalDynamicEnumerationData> LocalDynamicEnumerations { get; }
        IReadOnlyList<EnumerationData> Enumerations { get; }
        IReadOnlyList<DecimalData> Decimals { get; }
        IReadOnlyList<LocalizedStringData> LocalizedStrings { get; }
        IReadOnlyList<IntegerData> Integers { get; }
        IReadOnlyList<NodeData> Nodes { get; }
        IReadOnlyList<ConnectorDefinitionData> Connectors { get; }
        IReadOnlyList<ConnectionDefinitionData> Connections { get; }
    }

    public class DomainData : IDomainData
    {
        IReadOnlyList<NodeTypeData> IDomainData.NodeTypes => m_nodeTypes;
        IReadOnlyList<DynamicEnumerationData> IDomainData.DynamicEnumerations => m_dynamicEnumerations;
        IReadOnlyList<LocalDynamicEnumerationData> IDomainData.LocalDynamicEnumerations => m_localDynamicEnumerations;
        IReadOnlyList<EnumerationData> IDomainData.Enumerations => m_enumerations;
        IReadOnlyList<DecimalData> IDomainData.Decimals => m_decimals;
        IReadOnlyList<LocalizedStringData> IDomainData.LocalizedStrings => m_localizedStrings;
        IReadOnlyList<IntegerData> IDomainData.Integers => m_integers;
        IReadOnlyList<NodeData> IDomainData.Nodes => m_nodes;
        IReadOnlyList<ConnectorDefinitionData> IDomainData.Connectors => m_connectors;
        IReadOnlyList<ConnectionDefinitionData> IDomainData.Connections => m_connections;

        private readonly List<NodeTypeData> m_nodeTypes = new List<NodeTypeData>();
        private readonly List<DynamicEnumerationData> m_dynamicEnumerations = new List<DynamicEnumerationData>();
        private readonly List<LocalDynamicEnumerationData> m_localDynamicEnumerations = new List<LocalDynamicEnumerationData>();
        private readonly List<EnumerationData> m_enumerations = new List<EnumerationData>();
        private readonly List<DecimalData> m_decimals = new List<DecimalData>();
        private readonly List<LocalizedStringData> m_localizedStrings = new List<LocalizedStringData>();
        private readonly List<IntegerData> m_integers = new List<IntegerData>();
        private readonly List<NodeData> m_nodes = new List<NodeData>();
        private readonly List<ConnectorDefinitionData> m_connectors = new List<ConnectorDefinitionData>();
        private readonly List<ConnectionDefinitionData> m_connections = new List<ConnectionDefinitionData>();

        public ICollection<NodeTypeData> NodeTypes => m_nodeTypes;
        public ICollection<DynamicEnumerationData> DynamicEnumerations => m_dynamicEnumerations;
        public ICollection<LocalDynamicEnumerationData> LocalDynamicEnumerations => m_localDynamicEnumerations;
        public ICollection<EnumerationData> Enumerations => m_enumerations;
        public ICollection<DecimalData> Decimals => m_decimals;
        public ICollection<LocalizedStringData> LocalizedStrings => m_localizedStrings;
        public ICollection<IntegerData> Integers => m_integers;
        public ICollection<NodeData> Nodes => m_nodes;
        public ICollection<ConnectorDefinitionData> Connectors => m_connectors;
        public ICollection<ConnectionDefinitionData> Connections => m_connections;
    }
}
