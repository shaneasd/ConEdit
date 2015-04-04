using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Utilities;

namespace Conversation
{
    public interface IEditable
    {
        /// <summary>
        /// ID of the actual node instance in the graph
        /// </summary>
        ID<NodeTemp> NodeID { get; }

        //Node type information
        ID<NodeTypeTemp> NodeTypeID { get; }
        string Name { get; }
        List<NodeData.ConfigData> Config { get; }

        IEnumerable<Parameter> Parameters { get; }
        IEnumerable<Output> Connectors { get; }

        /// <summary>
        /// Triggered when any of the node's connectors is connected/disconnected to/from another connector
        /// </summary>
        event Action Linked;

        /// <summary>
        /// Change the node's ID. Be careful about doing this as things refering to the node by ID will have dangling pointers
        /// </summary>
        /// <param name="id">the new ID</param>
        void ChangeId(ID<NodeTemp> id);

        /// <summary>
        /// Attempt to decorrupt all corrupted parameters within the node
        /// </summary>
        void TryDecorrupt();

        SimpleUndoPair RemoveUnknownParameter(UnknownParameter p);
    }
}
