using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    public interface INodeType
    {
        IEnumerable<INodeType> ChildTypes { get; }
        IEnumerable<INodeDataGenerator> Nodes { get; }
        string Name { get; }
        Guid Guid { get; }
    }

    public sealed class NodeTypeTemp { }

    public class NodeCategory : INodeType
    {
        public NodeCategory(string name, Guid guid)
        {
            Name = name;
            Guid = guid;
        }

        private List<NodeCategory> m_childTypes { get; } = new List<NodeCategory>();
        public void AddChildType(NodeCategory childType)
        {
            m_childTypes.Add(childType);
        }

        public IEnumerable<NodeCategory> ChildTypes
        {
            get { return m_childTypes; }
        }

        IEnumerable<INodeType> INodeType.ChildTypes
        {
            get { return m_childTypes; }
        }

        private List<INodeDataGenerator> m_nodes { get; } = new List<INodeDataGenerator>();
        public IEnumerable<INodeDataGenerator> Nodes
        {
            get { return m_nodes; }
        }
        public void AddNode(INodeDataGenerator node)
        {
            m_nodes.Add(node);
        }
        public void ClearNodes()
        {
            m_nodes.Clear();
        }

        public string Name { get; }

        public Guid Guid { get; }
    }

    public interface IDataSource
    {
        IEnumerable<ParameterType> ParameterTypes { get; }
        INodeType Nodes { get; }
        INodeDataGenerator GetNode(Id<NodeTypeTemp> guid);
        bool IsInteger(ParameterType type);
        bool IsDecimal(ParameterType type);
        bool IsEnum(ParameterType type);
        bool IsDynamicEnum(ParameterType type);
        bool IsLocalDynamicEnum(ParameterType type);

        bool IsCategoryDefinition(Id<NodeTypeTemp> id);
        bool IsTypeDefinition(Id<NodeTypeTemp> id);
        bool IsConnectorDefinition(Id<NodeTypeTemp> id);
        bool IsNodeDefinition(Id<NodeTypeTemp> id);
        bool IsAutoCompleteNode(Id<NodeTypeTemp> id);

        string GetTypeName(ParameterType type);

        Guid GetCategory(Id<NodeTypeTemp> type);

        DynamicEnumParameter.Source GetSource(ParameterType type, object newSourceId);
    }

    public static class DataSourceUtils
    {
        public static IEnumerable<INodeDataGenerator> AllNodes(this IDataSource datasource)
        {
            return datasource.Nodes.Collapse(n => n.ChildTypes, n => n.Nodes);
        }
    }

    public sealed class DummyDataSource : IDataSource
    {
        private DummyDataSource() { }
        public static IDataSource Instance { get; } = new DummyDataSource();

        IEnumerable<ParameterType> IDataSource.ParameterTypes
        {
            get { return Enumerable.Empty<ParameterType>(); }
        }

        INodeType IDataSource.Nodes
        {
            get { return new NodeCategory("", Guid.NewGuid()); }
        }

        INodeDataGenerator IDataSource.GetNode(Id<NodeTypeTemp> nodeType)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsInteger(ParameterType type)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsDecimal(ParameterType type)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsEnum(ParameterType type)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsDynamicEnum(ParameterType type)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsLocalDynamicEnum(ParameterType type)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsCategoryDefinition(Id<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsTypeDefinition(Id<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsConnectorDefinition(Id<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsNodeDefinition(Id<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsAutoCompleteNode(Id<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        string IDataSource.GetTypeName(ParameterType type)
        {
            throw new NotImplementedException();
        }

        Guid IDataSource.GetCategory(Id<NodeTypeTemp> type)
        {
            throw new NotImplementedException();
        }

        DynamicEnumParameter.Source IDataSource.GetSource(ParameterType type, object newSourceId)
        {
            throw new NotImplementedException();
        }
    }
}
