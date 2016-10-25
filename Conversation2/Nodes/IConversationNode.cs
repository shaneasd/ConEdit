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
        /// This nodes data (but not its UI information)
        /// </summary>
        IConversationNodeData Data { get; }
    }

    public interface IConfigurable
    {
        /// <summary>
        /// Ask the user for modifications to an object and return a ConfigureResult indicating either operations required or reason for aborting the edit operation.
        /// Providing this wrapper layer allows the object to update its state as a consequence of the modification
        /// </summary>
        /// <param name="configureData">The mechanism to query the user for modifications to the node</param>
        /// <returns>Action required as a result of user behavior</returns>
        ConfigureResult Configure(NodeEditOperation configureData);
    }
}
