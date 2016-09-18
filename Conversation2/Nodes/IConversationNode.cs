using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    public interface IConversationNode
    {
        /// <summary>
        /// Wraps Data.NodeId
        /// </summary>
        Id<NodeTemp> Id { get; }

        /// <summary>
        /// Wraps Data.NodeTypeId
        /// </summary>
        Id<NodeTypeTemp> Type { get; }

        /// <summary>
        /// Wraps Data.Name
        /// </summary>
        string NodeName { get; }

        /// <summary>
        /// Wraps Data.Connectors
        /// </summary>
        IEnumerable<Output> Connectors { get; }

        /// <summary>
        /// Wraps Data.Parameters
        /// </summary>
        IEnumerable<IParameter> Parameters { get; }

        /// <summary>
        /// This nodes data (but not its UI information)
        /// </summary>
        IConversationNodeData Data { get; }
    }

    public interface IConfigurable
    {
        ConfigureResult Configure(Func<IConversationNodeData, ConfigureResult> configureData);
    }
}
