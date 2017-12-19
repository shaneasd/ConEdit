using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    public class Output
    {
        public ConnectorDefinitionData Definition { get; }
        public Id<TConnector> Id { get; }
        public IConversationNodeData Parent { get; }
        public IReadOnlyList<IParameter> Parameters { get; }
        public IConnectionRules Rules { get; }

        private List<Output> m_connections = new List<Output>();

        public Output(Id<TConnector> id, ConnectorDefinitionData definition, IConversationNodeData parent, IReadOnlyList<IParameter> parameters, IConnectionRules rules)
        {
            Definition = definition;
            Parent = parent;
            Parameters = parameters;
            Rules = rules;
            Id = id;
        }

        public IEnumerable<Output> Connections
        {
            get { return m_connections; }
        }

        public event Action<Output> Connected;
        public event Action<Output> Disconnected;

        public bool CanConnectTo(Output other, ConnectionConsiderations ignore)
        {
            if (!ignore.HasFlag(ConnectionConsiderations.SameNode))
            {
                //Can't connect two connectors belonging to the same node
                if (object.ReferenceEquals(other.Parent.NodeId, Parent.NodeId))
                    return false;
            }

            if (!ignore.HasFlag(ConnectionConsiderations.RedundantConnection))
            {
                //Can't connect redundantly to an input this output is already connected to
                if (m_connections.Contains(other))
                    return false;
            }

            if (!ignore.HasFlag(ConnectionConsiderations.RuleViolation))
            {
                //Can only connect connectors whose types can be paired according to the rules
                if (!Rules.CanConnect(this.Definition.Id, other.Definition.Id))
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
        /// Connect this connector to another connector
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

        public SimpleUndoPair DisconnectAllActions()
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

        /// <summary>
        /// Can be used in user interface to identify the output (more user friendly than the unique id)
        /// If the there is a parameter with Id ConnectorDefinitionData.OutputName of type IStringParameter we use the value of that parameter
        /// If there are more than one such parameters we fail (shouldn't be two parameters with the same id)
        /// Else if there is exactly one string parameter with any Id we use the value of that parameter
        /// Else we are nameless and return an empty string.
        /// </summary>
        public string GetName()
        {
            var nameParameter = Parameters.Where(p => p.Id == ConnectorDefinitionData.OutputName).SingleOrDefault();
            if (nameParameter != null)
            {
                var stringParameter = nameParameter as IStringParameter;
                return stringParameter.Value;
            }
            var stringParameters = Parameters.OfType<IStringParameter>();
            if (stringParameters.CountEquals(1))
                return stringParameters.Single().Value;
            return "";
        }
    }
}
