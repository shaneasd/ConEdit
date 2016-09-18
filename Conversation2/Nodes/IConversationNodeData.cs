using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Utilities;
using System.Collections.ObjectModel;

namespace Conversation
{
    public interface IConversationNodeData
    {
        /// <summary>
        /// ID of the actual node instance in the graph
        /// </summary>
        Id<NodeTemp> NodeId { get; }

        /// <summary>
        /// The type of the node (i.e. the id of the node in the domain file which declares this nodes type)
        /// </summary>
        Id<NodeTypeTemp> NodeTypeId { get; }

        /// <summary>
        /// The name of this node as defined by the declaration of the node's type in the domain file
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Any configuration applied to this node as a consequence of configuration linked to the node declaration in the domain file
        /// </summary>
        IReadOnlyList<NodeData.ConfigData> Config { get; }

        /// <summary>
        /// Parameters of the node that define the data it contains
        /// </summary>
        IEnumerable<IParameter> Parameters { get; }

        /// <summary>
        /// Connections of this node to other nodes
        /// </summary>
        IEnumerable<Output> Connectors { get; }

        /// <summary>
        /// Triggered when any of the node's connectors is connected/disconnected to/from another connector
        /// </summary>
        event Action Linked;

        /// <summary>
        /// Change the node's ID. Be careful about doing this as things refering to the node by ID will have dangling pointers
        /// </summary>
        /// <param name="id">the new ID</param>
        void ChangeId(Id<NodeTemp> id);

        /// <summary>
        /// Actions required to remove the input parameter from this node's list of parameters or undo said action.
        /// </summary>
        SimpleUndoPair RemoveUnknownParameter(UnknownParameter p);
    }
}
