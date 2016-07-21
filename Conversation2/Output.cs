using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    public abstract class ConnectorPosition
    {
        private class CTop : ConnectorPosition
        {
            public static ConnectorPosition Instance { get; } = new CTop();
            private CTop() : base("Top", Guid.Parse("24c96d32-1704-4c85-b2bf-b8da8731ea47")) { }
            public override T For<T>(Func<T> top, Func<T> bottom, Func<T> left, Func<T> right) { return top(); }
        }

        private class CBottom : ConnectorPosition
        {
            public static ConnectorPosition Instance { get; } = new CBottom();
            private CBottom() : base("Bottom", Guid.Parse("b5461736-18f1-417c-8a54-2c5a1726483b")) { }
            public override T For<T>(Func<T> top, Func<T> bottom, Func<T> left, Func<T> right) { return bottom(); }
        }

        private class CLeft : ConnectorPosition
        {
            public static ConnectorPosition Instance { get; } = new CLeft();
            private CLeft() : base("Left", Guid.Parse("adb2301c-a858-44e8-b76c-93e538231960")) { }
            public override T For<T>(Func<T> top, Func<T> bottom, Func<T> left, Func<T> right) { return left(); }
        }

        private class CRight : ConnectorPosition
        {
            public static ConnectorPosition Instance { get; } = new CRight();
            private CRight() : base("Right", Guid.Parse("d8b8efae-3949-47b3-af7b-8db1e402489e")) { }
            public override T For<T>(Func<T> top, Func<T> bottom, Func<T> left, Func<T> right) { return right(); }
        }

        public static ConnectorPosition Top { get; } = CTop.Instance;
        public static ConnectorPosition Bottom { get; } = CBottom.Instance;
        public static ConnectorPosition Left { get; } = CLeft.Instance;
        public static ConnectorPosition Right { get; } = CRight.Instance;

        public abstract T For<T>(Func<T> top, Func<T> bottom, Func<T> left, Func<T> right);

        public static bool operator ==(ConnectorPosition a, ConnectorPosition b)
        {
            return object.Equals(a, b);
        }

        public static bool operator !=(ConnectorPosition a, ConnectorPosition b)
        {
            return !object.Equals(a, b);
        }

        public override bool Equals(object obj)
        {
            ConnectorPosition other = obj as ConnectorPosition;
            if (other == null)
                return false;
            else
                return other.m_guid == m_guid;
        }

        public override int GetHashCode()
        {
            return m_guid.GetHashCode();
        }

        public static ConnectorPosition Read(IEnumParameter parameter)
        {
            return (new[] { Top, Bottom, Left, Right }).First(a => a.m_guid == parameter.Value);
        }

        private ConnectorPosition(string name, Guid guid)
        {
            m_name = name;
            m_guid = guid;
        }
        private Guid m_guid;
        private string m_name;

        private Tuple<Guid, string> Tuple { get { return System.Tuple.Create(m_guid, m_name); } }

        public static ParameterType EnumId { get; } = ParameterType.Parse("2b075746-9b6e-4d6e-ad39-a083049374f2");
        public static Id<Parameter> ParameterId { get; } = Id<Parameter>.Parse("43903044-1ef9-4c9f-a782-6219fb8e7826");

        public static EnumParameter MakeParameter()
        {
            Enumeration enumeration = new Enumeration(new[] { Top.Tuple, Bottom.Tuple, Left.Tuple, Right.Tuple }, EnumId, Bottom.m_guid);
            return new EnumParameter("Position", ParameterId, enumeration, null);
        }

        public EnumerationData.Element Element
        {
            get { return new EnumerationData.Element(m_name, m_guid); }
        }

        public static EnumerationData PositionConnectorDefinition
        {
            get { return new EnumerationData("Position", EnumId, new List<EnumerationData.Element>() { Top.Element, Bottom.Element, Left.Element, Right.Element }); }
        }
    }

    //public class OutputDefinition
    //{
    //    private readonly ConnectorDefinitionData m_data;
    //    public string Name { get { return m_data.Name; } }
    //    public ID<TConnectorDefinition> ID { get { return m_data.Id; } }
    //    public IEnumerable<Parameter> Parameters { get { return m_data.Parameters; } }
    //    public ConnectorPosition Position { get { return m_data.Position; } }
    //    private IConnectionRules m_rules;

    //    public OutputDefinition(string name, ID<OutputTemp> guid, IEnumerable<Parameter> parameters, ConnectorPosition position, IConnectionRules rules)
    //    {
    //        m_name = name;
    //        m_guid = guid;
    //        m_parameters = parameters;
    //        m_position = position;
    //        m_rules = rules;
    //    }

    //    public Output Generate(IEditable parent, ID<OutputTemp> id)
    //    {
    //        return new Output(id, this, parent, m_rules);
    //    }
    //}

    public sealed class TConnectorDefinition { }
    public sealed class TConnector { }

    public class NoConnections : IConnectionRules
    {
        public static IConnectionRules Instance { get; } = new NoConnections();
        public bool CanConnect(Id<TConnectorDefinition> a, Id<TConnectorDefinition> b)
        {
            return false;
        }
    }

    public interface IConnectionRules
    {
        bool CanConnect(Id<TConnectorDefinition> a, Id<TConnectorDefinition> b);
    }

    [FlagsAttribute]
    public enum ConnectionConsiderations
    {
        None = 0,
        SameNode = 1,
        RedundantConnection = 2,
        RuleViolation = 4,
    }

    public class Output
    {
        private readonly Id<TConnector> m_id;
        private readonly IEditable m_parent;
        private readonly List<Parameter> m_parameters;
        private readonly IConnectionRules m_rules;

        public readonly ConnectorDefinitionData m_definition;

        public Id<TConnector> Id { get { return m_id; } }
        public IEditable Parent { get { return m_parent; } }
        public List<Parameter> Parameters { get { return m_parameters; } }
        public IConnectionRules Rules { get { return m_rules; } }

        private List<Output> m_connections = new List<Output>();

        public Output(Id<TConnector> id, ConnectorDefinitionData definition, IEditable parent, List<Parameter> parameters, IConnectionRules rules)
        {
            m_definition = definition;
            m_parent = parent;
            m_parameters = parameters;
            m_rules = rules;
            m_id = id;
        }


        public IEnumerable<Output> Connections
        {
            get { return m_connections; }
        }

        public event Action<Output> Connected;
        public event Action<Output> Disconnected;

        public bool CanConnectTo(Output other, ConnectionConsiderations ignore)
        {
            if ((ignore & ConnectionConsiderations.SameNode) == ConnectionConsiderations.None)
            {
                //Can't connect two connectors belonging to the same node
                if (object.ReferenceEquals(other.Parent.NodeId, Parent.NodeId))
                    return false;
            }

            if ((ignore & ConnectionConsiderations.RedundantConnection) == ConnectionConsiderations.None)
            {
                //Can't connect redundantly to an input this output is already connected to
                if (m_connections.Contains(other))
                    return false;
            }

            if ((ignore & ConnectionConsiderations.RuleViolation) == ConnectionConsiderations.None)
            {
                //Can only connect connectors whose types can be paired according to the rules
                if (!Rules.CanConnect(this.m_definition.Id, other.m_definition.Id))
                    return false;
            }

            return true;
        }

        private bool CounterConnect(Output other, bool force)
        {
            if (!CanConnectTo(other, force ? ConnectionConsiderations.RuleViolation : ConnectionConsiderations.None))
                return false;

            m_connections.Add(other);
            Connected.Execute(other);
            return true;
        }

        /// <summary>
        /// Connect this connector with another connector
        /// </summary>
        /// <param name="other">the connector to connect with this connector</param>
        /// <param name="force">true -> ignore connection rules and connect even if rules forbid connection</param>
        /// <returns></returns>
        public bool ConnectTo(Output other, bool force)
        {
            if (other.CounterConnect(this, force))
            {
                return CounterConnect(other, force);
            }
            else
            {
                return false;
            }
        }

        private void CounterDisconnect(Output other)
        {
            m_connections.Remove(other);
            Disconnected.Execute(other);
        }

        public void Disconnect(Output other)
        {
            other.CounterDisconnect(this);
            CounterDisconnect(other);
        }

        public SimpleUndoPair DisconnectAll()
        {
            var connections = m_connections.ToList();
            return new SimpleUndoPair()
            {
                Redo = () =>
                {
                    while (m_connections.Any())
                    {
                        Disconnect(m_connections.First());
                    }
                },
                Undo = () =>
                {
                    foreach (var connection in connections)
                        ConnectTo(connection, true); //no need to check rules. we're just readding a connection that already existed
                }
            };
        }

        public override bool Equals(object obj)
        {
            Output other = (Output)obj;
            if (other == null)
                return false;
            return Id.Equals(other.Id) && Parent.Equals(other.Parent);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(Id, Parent).GetHashCode();
        }

        public string GetName()
        {
            string name = Parameters.Where(p => p.Id == ConnectorDefinitionData.OutputName).Select(p => p as IStringParameter).Select(p => p.Value).SingleOrDefault();
            if (name == null)
            {
                var stringParameters = Parameters.OfType<IStringParameter>();
                if (stringParameters.CountEquals(1))
                    name = stringParameters.Single().Value;
            }
            return name ?? "";
        }
    }
}
