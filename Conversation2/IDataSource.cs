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
        IEnumerable<EditableGenerator> Nodes { get; }
        string Name { get; }
        Guid Guid { get; }
    }

    public sealed class NodeTypeTemp { }

    public class NodeType : INodeType
    {
        public NodeType(string name, Guid guid)
        {
            m_name = name;
            m_guid = guid;
        }

        public readonly List<NodeType> m_childTypes = new List<NodeType>();
        public IEnumerable<INodeType> ChildTypes
        {
            get { return m_childTypes; }
        }

        public readonly List<EditableGenerator> m_nodes = new List<EditableGenerator>();
        public IEnumerable<EditableGenerator> Nodes
        {
            get { return m_nodes; }
        }

        public readonly string m_name;
        public string Name
        {
            get { return m_name; }
        }

        public readonly Guid m_guid;
        public Guid Guid
        {
            get { return m_guid; }
        }
    }

    public interface IDataSource
    {
        IEnumerable<ParameterType> ParameterTypes { get; }
        INodeType Nodes { get; }
        EditableGenerator GetNode(ID<NodeTypeTemp> guid);
        bool IsInteger(ParameterType guid);
        bool IsDecimal(ParameterType guid);
        bool IsEnum(ParameterType guid);
        bool IsDynamicEnum(ParameterType guid);

        bool IsCategoryDefinition(ID<NodeTypeTemp> id);
        bool IsTypeDefinition(ID<NodeTypeTemp> id);
        bool IsConnectorDefinition(ID<NodeTypeTemp> id);
        bool IsNodeDefinition(ID<NodeTypeTemp> id);

        string GetTypeName(ParameterType type);

        Guid GetCategory(ID<NodeTypeTemp> type);
    }

    public static class DataSourceUtils
    {
        public static IEnumerable<EditableGenerator> AllNodes(this IDataSource datasource)
        {
            return datasource.Nodes.Collapse(n => n.ChildTypes, n => n.Nodes);
        }
    }

    public class DummyDataSource : IDataSource
    {
        private DummyDataSource() { }
        public static readonly IDataSource Instance = new DummyDataSource();

        IEnumerable<ParameterType> IDataSource.ParameterTypes
        {
            get { return Enumerable.Empty<ParameterType>(); }
        }

        INodeType IDataSource.Nodes
        {
            get { return new NodeType("", Guid.NewGuid()); }
        }

        EditableGenerator IDataSource.GetNode(ID<NodeTypeTemp> nodeType)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsInteger(ParameterType parameterType)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsDecimal(ParameterType parameterType)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsEnum(ParameterType parameterType)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsDynamicEnum(ParameterType parameterType)
        {
            throw new NotImplementedException();
        }


        public bool IsCategoryDefinition(ID<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        public bool IsTypeDefinition(ID<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        public bool IsConnectorDefinition(ID<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        public bool IsNodeDefinition(ID<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }


        bool IDataSource.IsCategoryDefinition(ID<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsTypeDefinition(ID<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsConnectorDefinition(ID<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        bool IDataSource.IsNodeDefinition(ID<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        string IDataSource.GetTypeName(ParameterType type)
        {
            throw new NotImplementedException();
        }

        Guid IDataSource.GetCategory(ID<NodeTypeTemp> type)
        {
            throw new NotImplementedException();
        }
    }
}
