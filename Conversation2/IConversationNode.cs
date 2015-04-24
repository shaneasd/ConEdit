using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    public interface IGraphNode
    {
        /// <summary>
        /// ID of the actual node instance in the graph
        /// </summary>
        ID<NodeTemp> Id { get; }

        /// <summary>
        /// ID of the nodes type
        /// </summary>
        ID<NodeTypeTemp> Type { get; }

        string NodeName { get; }

        IEnumerable<Output> Connectors { get; }
    }

    public interface IConversationNode : IGraphNode
    {
        IEnumerable<Parameter> Parameters { get; }
    }

    public interface IConfigurable
    {
        ConfigureResult Configure(Func<IEditable, ConfigureResult> configureData);
    }
}
