﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Utilities;
using System.Collections.ObjectModel;

namespace Conversation
{
    public interface IEditable
    {
        /// <summary>
        /// ID of the actual node instance in the graph
        /// </summary>
        Id<NodeTemp> NodeId { get; }

        //Node type information
        Id<NodeTypeTemp> NodeTypeId { get; }
        string Name { get; }
        IReadOnlyList<NodeData.ConfigData> Config { get; }

        IEnumerable<IParameter> Parameters { get; }
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

        SimpleUndoPair RemoveUnknownParameter(UnknownParameter p);
    }
}
