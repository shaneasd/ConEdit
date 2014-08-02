using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Conversation.Serialization;
using System.Drawing;
using System.Globalization;

namespace RuntimeConversation
{
    public abstract class TypeDeserializerBase
    {
        public static void Deserialize(out int a, string value)
        {
            a = int.Parse(value);
        }

        public static void Deserialize(out decimal a, string value)
        {
            a = decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        public static void Deserialize(out string a, string value)
        {
            a = value;
        }

        public static void Deserialize(out bool a, string value)
        {
            a = bool.Parse(value);
        }

        public static void Deserialize(out LocalizedString a, string value)
        {
            a = new LocalizedString(value);
        }

        public static void Deserialize(out Audio a, string value)
        {
            a = new Audio(value);
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ParameterIDAttribute : Attribute
    {
        public Guid Guid { get; private set; }
        public ParameterIDAttribute(string guid)
        {
            Guid = Guid.Parse(guid);
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EnumValueIDAttribute : Attribute
    {
        public Guid Guid { get; private set; }
        public EnumValueIDAttribute(string guid)
        {
            Guid = Guid.Parse(guid);
        }
    }

    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Struct)]
    public class TypeIDAttribute : Attribute
    {
        public Guid Guid { get; private set; }
        public TypeIDAttribute(string guid)
        {
            Guid = Guid.Parse(guid);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ConnectorTypeIDAttribute : Attribute
    {
        public Guid Guid { get; private set; }
        public ConnectorTypeIDAttribute(string guid)
        {
            Guid = Guid.Parse(guid);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NodeTypeIDAttribute : Attribute
    {
        public Guid Guid { get; private set; }
        public NodeTypeIDAttribute(string guid)
        {
            Guid = Guid.Parse(guid);
        }
    }

    public abstract class ConnectorBase
    {
        private ID<TConnector> m_id;
        public ID<TConnector> ID { get { return m_id; } }

        protected ConnectorBase(ID<TConnector> id)
        {
            m_id = id;
        }
    }

    public abstract class NodeBase
    {
        private ID<NodeTemp> m_id;
        //public abstract IEnumerable<Connector> Connectors { get; }
        public ID<NodeTemp> ID { get { return m_id; } }

        public readonly PointF Position;

        public NodeBase(ID<NodeTemp> id, PointF position)
        {
            m_id = id;
            Position = position;
        }

        public string GetParameter(IEnumerable<CustomDeserializer.Parameter> parameters, Guid guid)
        {
            return parameters.First(p => p.Guid == guid).Value;
        }

        public abstract void Connect(ID<TConnector> thisConnectorID, NodeBase other, ID<TConnector> otherConnectorID);
    }

    public class Conversation
    {
        public readonly IEnumerable<RuntimeConversation.NodeBase> Nodes;
        public readonly IEnumerable<Error> Errors;
        public Conversation(IEnumerable<RuntimeConversation.NodeBase> nodes, IEnumerable<Error> errors)
        {
            Nodes = nodes;
            Errors = errors;
        }
    }
}
