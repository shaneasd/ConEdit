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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Guid property should use Guid type but will always be specified as a string in the attribute constructor for convenience")]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ParameterIdAttribute : Attribute
    {
        public Guid Guid { get; }
        public ParameterIdAttribute(string guid)
        {
            Guid = Guid.Parse(guid);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Guid property should use Guid type but will always be specified as a string in the attribute constructor for convenience")]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EnumValueIdAttribute : Attribute
    {
        public Guid Guid { get; }
        public EnumValueIdAttribute(string guid)
        {
            Guid = Guid.Parse(guid);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Guid property should use Guid type but will always be specified as a string in the attribute constructor for convenience")]
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Struct)]
    public sealed class TypeIdAttribute : Attribute
    {
        public Guid Guid { get; }
        public TypeIdAttribute(string guid)
        {
            Guid = Guid.Parse(guid);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Guid property should use Guid type but will always be specified as a string in the attribute constructor for convenience")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ConnectorTypeIdAttribute : Attribute
    {
        public Guid Guid { get; }
        public ConnectorTypeIdAttribute(string guid)
        {
            Guid = Guid.Parse(guid);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Guid property should use Guid type but will always be specified as a string in the attribute constructor for convenience")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NodeTypeIdAttribute : Attribute
    {
        public Guid Guid { get; }
        public NodeTypeIdAttribute(string guid)
        {
            Guid = Guid.Parse(guid);
        }
    }

    public abstract class ConnectorBase
    {
        private Id<TConnector> m_id;
        public Id<TConnector> Id => m_id;

        protected ConnectorBase(Id<TConnector> id)
        {
            m_id = id;
        }
    }

    public abstract class NodeBase
    {
        //public abstract IEnumerable<Connector> Connectors { get; }
        public Id<NodeTemp> Id { get; }

        public PointF Position { get; }

        protected NodeBase(Id<NodeTemp> id, PointF position)
        {
            Id = id;
            Position = position;
        }

        protected static string GetParameter(IEnumerable<CustomDeserializerParameter> parameters, Guid guid)
        {
            return parameters.First(p => p.Guid == guid).Value;
        }

        public abstract void Connect(Id<TConnector> thisConnectorId, NodeBase other, Id<TConnector> otherConnectorId);
    }

    public class Conversation
    {
        public IEnumerable<RuntimeConversation.NodeBase> Nodes { get; }
        public IEnumerable<LoadError> Errors { get; }

        public Conversation(IEnumerable<RuntimeConversation.NodeBase> nodes, IEnumerable<LoadError> errors)
        {
            Nodes = nodes;
            Errors = errors;
        }
    }
}
