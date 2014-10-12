using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    public class ConnectorPosition
    {
        public static ConnectorPosition Top = new ConnectorPosition("Top", Guid.Parse("24c96d32-1704-4c85-b2bf-b8da8731ea47"));
        public static ConnectorPosition Bottom = new ConnectorPosition("Bottom", Guid.Parse("b5461736-18f1-417c-8a54-2c5a1726483b"));
        public static ConnectorPosition Left = new ConnectorPosition("Left", Guid.Parse("adb2301c-a858-44e8-b76c-93e538231960"));
        public static ConnectorPosition Right = new ConnectorPosition("Right", Guid.Parse("d8b8efae-3949-47b3-af7b-8db1e402489e"));

        public T For<T>(Func<T> top, Func<T> bottom, Func<T> left, Func<T> right)
        {
            if (this == ConnectorPosition.Top)
                return top();
            else if (this == ConnectorPosition.Bottom)
                return bottom();
            else if (this == ConnectorPosition.Left)
                return left();
            else if (this == ConnectorPosition.Right)
                return right();
            else
                throw new Exception("Unkown ConnectorPosition in ConnectorPosition.For");
        }

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

        public static ID<ParameterType> ENUM_ID = ID<ParameterType>.Parse("2b075746-9b6e-4d6e-ad39-a083049374f2");
        public static ID<Parameter> PARAMETER_ID = ID<Parameter>.Parse("43903044-1ef9-4c9f-a782-6219fb8e7826");

        public static EnumParameter MakeParameter()
        {
            Enumeration enumeration = new Enumeration(new[] { Top.Tuple, Bottom.Tuple, Left.Tuple, Right.Tuple }, ENUM_ID, Bottom.m_guid);
            return new EnumParameter("Position", PARAMETER_ID, enumeration);
        }

        public EnumerationData.Element Element
        {
            get { return new EnumerationData.Element(m_name, m_guid); }
        }

        public static EnumerationData PositionConnectorDefinition
        {
            get { return new EnumerationData("Position", ENUM_ID, new List<EnumerationData.Element>() { Top.Element, Bottom.Element, Left.Element, Right.Element }); }
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
        public static readonly IConnectionRules Instance = new NoConnections();
        public bool CanConnect(ID<TConnectorDefinition> a, ID<TConnectorDefinition> b)
        {
            return false;
        }
    }

    public interface IConnectionRules
    {
        bool CanConnect(ID<TConnectorDefinition> a, ID<TConnectorDefinition> b);
    }

    public class Output
    {
        public readonly ConnectorDefinitionData m_definition;
        public Output(ID<TConnector> id, ConnectorDefinitionData definition, IEditable parent, List<Parameter> parameters, IConnectionRules rules)
        {
            m_definition = definition;
            Parent = parent;
            Parameters = parameters;
            Rules = rules;
            ID = id;
        }

        public readonly ID<TConnector> ID;
        public readonly IEditable Parent;
        public readonly List<Parameter> Parameters;
        public readonly IConnectionRules Rules;

        public List<Output> m_connections = new List<Output>();

        public IEnumerable<Output> Connections
        {
            get { return m_connections; }
        }

        public event Action Connected;
        public event Action Disconnected;

        public bool CanConnectTo(Output other)
        {
            //Can't connect a nodes output to its own input
            if (object.ReferenceEquals(other.Parent.NodeID, Parent.NodeID))
                return false;

            //Can't connect redundantly to an input this output is already connected to
            if (m_connections.Contains(other))
                return false;

            //Can only connect connectors whose types can be paired according to the rules
            if (!Rules.CanConnect(this.m_definition.Id, other.m_definition.Id))
                return false;

            return true;
        }

        private bool CounterConnect(Output other)
        {
            if (!CanConnectTo(other))
                return false;

            m_connections.Add(other);
            Connected.Execute();
            return true;
        }

        public bool ConnectTo(Output other)
        {
            if (other.CounterConnect(this))
            {
                return CounterConnect(other);
            }
            else
            {
                return false;
            }
        }

        private void CounterDisconnect(Output other)
        {
            m_connections.Remove(other);
            Disconnected.Execute();
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
                        ConnectTo(connection);
                }
            };
        }

        public override bool Equals(object obj)
        {
            Output other = (Output)obj;
            if (other == null)
                return false;
            return ID.Equals(other.ID) && Parent.Equals(other.Parent);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(ID, Parent).GetHashCode();
        }

        public string GetName()
        {
            var name = Parameters.Where(p => p.Id == ConnectorDefinitionData.OUTPUT_NAME).Select(p => p as IStringParameter).Select(p => p.Value).SingleOrDefault() ?? "";
            return name;
        }
    }
}
