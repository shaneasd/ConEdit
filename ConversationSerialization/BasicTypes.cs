using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Conversation.Serialization;
using System.Drawing;
using System.Globalization;
using Utilities;

namespace RuntimeConversation
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ParameterIdAttribute : Attribute
    {
        private Guid m_guid;
        public Guid Guid { get { return m_guid; } }
        public ParameterIdAttribute(string guid)
        {
            m_guid = Guid.Parse(guid);
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EnumValueIdAttribute : Attribute
    {
        private Guid m_guid;
        public Guid Guid { get { return m_guid; } }
        public EnumValueIdAttribute(string guid)
        {
            m_guid = Guid.Parse(guid);
        }
    }

    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Struct)]
    public sealed class TypeIdAttribute : Attribute
    {
        private Guid m_guid;
        public Guid Guid { get { return m_guid; } }
        public TypeIdAttribute(string guid)
        {
            m_guid = Guid.Parse(guid);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ConnectorTypeIdAttribute : Attribute
    {
        private Guid m_guid;
        public Guid Guid { get { return m_guid; } }
        public ConnectorTypeIdAttribute(string guid)
        {
            m_guid = Guid.Parse(guid);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NodeTypeIdAttribute : Attribute
    {
        private Guid m_guid;
        public Guid Guid { get { return m_guid; } }
        public NodeTypeIdAttribute(string guid)
        {
            m_guid = Guid.Parse(guid);
        }
    }

    public abstract class ConnectorBase
    {
        private ID<TConnector> m_id;
        public ID<TConnector> Id { get { return m_id; } }

        protected ConnectorBase(ID<TConnector> id)
        {
            m_id = id;
        }
    }

    public abstract class NodeBase
    {
        private readonly ID<NodeTemp> m_id;
        //public abstract IEnumerable<Connector> Connectors { get; }
        public ID<NodeTemp> Id { get { return m_id; } }

        private readonly PointF m_position;
        public PointF Position { get { return m_position; } }

        protected NodeBase(ID<NodeTemp> id, PointF position)
        {
            m_id = id;
            m_position = position;
        }

        protected static string GetParameter(IEnumerable<CustomDeserializerParameter> parameters, Guid guid)
        {
            return parameters.First(p => p.Guid == guid).Value;
        }

        public abstract void Connect(ID<TConnector> thisConnectorID, NodeBase other, ID<TConnector> otherConnectorId);
    }

    public class Conversation
    {
        private readonly IEnumerable<RuntimeConversation.NodeBase> m_nodes;
        public IEnumerable<RuntimeConversation.NodeBase> Nodes { get { return m_nodes; } }

        private readonly IEnumerable<LoadError> m_errors;
        public IEnumerable<LoadError> Errors { get { return m_errors; } }

        public Conversation(IEnumerable<RuntimeConversation.NodeBase> nodes, IEnumerable<LoadError> errors)
        {
            m_nodes = nodes;
            m_errors = errors;
        }
    }
}
