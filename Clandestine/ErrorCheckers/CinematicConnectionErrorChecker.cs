using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

namespace Clandestine.ErrorCheckers
{
    public class CinematicConnectionErrorChecker<T> : ErrorChecker<T>
        where T : class, IConversationNode
    {
        private class CinematicControllerConnectionError : ConversationError<T>
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

        private class CinematicControllerConnectionError2 : ConversationError<T>
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

        Id<TConnectorDefinition> CINEMATIC = Id<TConnectorDefinition>.Parse("430e6b72-0caa-476e-8da6-8bd1f127127a");
        Id<TConnectorDefinition> CONTROLLER = Id<TConnectorDefinition>.Parse("5c80f036-9bf8-43b7-afd5-0c4d619917c1");
        public override IEnumerable<ConversationError<T>> Check(IEnumerable<T> nodes, IErrorCheckerUtilities<T> utils)
        {
            foreach (var node in nodes)
            {
                var controllersConnectors = node.Data.Connectors.Where(c => c.Definition.Id == CONTROLLER);
                foreach (var connector in controllersConnectors)
                {
                    if ( !connector.Connections.Any())
                        yield return new CinematicControllerConnectionError2(node);
                    foreach (var connection in connector.Connections)
                    {
                        if (connection.Definition.Id != CINEMATIC)
                        {
                            yield return new CinematicControllerConnectionError(node);
                        }
                    }
                }
            }
        }

        public override string Name
        {
            get { return "Cinematic Connections"; }
        }
    }
}
