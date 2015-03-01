using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

namespace Clandestine.ErrorCheckers
{
    public class CinematicConnectionErrorChecker<T> : ErrorChecker<T>
        where T : IConversationNode
    {
        public class CinematicControllerConnectionError : ConversationError<T>
        {
            public CinematicControllerConnectionError(T node)
                : base(node.Only())
            {
            }
            public override string Message
            {
                get { return "Node has a controller connection that is connected to a non cinematic connector"; }
            }
        }

        public class CinematicControllerConnectionError2 : ConversationError<T>
        {
            public CinematicControllerConnectionError2(T node)
                : base(node.Only())
            {
            }
            public override string Message
            {
                get { return "Node has a controller connection that is not connected to anything"; }
            }
        }

        ID<TConnectorDefinition> CINEMATIC = ID<TConnectorDefinition>.Parse("430e6b72-0caa-476e-8da6-8bd1f127127a");
        ID<TConnectorDefinition> CONTROLLER = ID<TConnectorDefinition>.Parse("5c80f036-9bf8-43b7-afd5-0c4d619917c1");
        public override IEnumerable<ConversationError<T>> Check(IEnumerable<T> conversationFile, IErrorCheckerUtilities utils)
        {
            foreach (var node in conversationFile)
            {
                var controllersConnectors = node.Connectors.Where(c => c.m_definition.Id == CONTROLLER);
                foreach (var connector in controllersConnectors)
                {
                    if ( !connector.Connections.Any())
                        yield return new CinematicControllerConnectionError2(node);
                    foreach (var connection in connector.Connections)
                    {
                        if (connection.m_definition.Id != CINEMATIC)
                        {
                            yield return new CinematicControllerConnectionError(node);
                        }
                    }
                }
            }
        }

        public override string GetName()
        {
            return "Cinematic Connections";
        }
    }
}
