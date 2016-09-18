﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using Conversation;
using Conversation.Serialization;
using Utilities;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGui>;

    public interface IConversationEditorControlData<TNode, TTransitionUI> : ISaveableFileUndoableProvider where TNode : IRenderable<IGui>
    {
        TNode GetNode(Id<NodeTemp> id);
        Tuple<IEnumerable<TNode>, IEnumerable<NodeGroup>> DuplicateInto(IEnumerable<GraphAndUI<NodeUIData>> nodeData, IEnumerable<NodeGroup> groups, object documentId, PointF location, ILocalizationEngine localization);
        void Add(IEnumerable<TNode> nodes, IEnumerable<NodeGroup> groups);
        bool Remove(IEnumerable<TNode> nodes, IEnumerable<NodeGroup> groups);
        IEnumerableReversible<TNode> Nodes { get; }
        IEnumerableReversible<NodeGroup> Groups { get; }
        void RemoveLinks(Output o);
        /// <summary>
        /// Issues that were detected in deserialization that can be automatically resolved but with possible loss of data
        /// e.g. removing links that point to non-existent nodes
        /// </summary>
        ReadOnlyCollection<LoadError> Errors { get; }
        void ClearErrors();

        void BringToFront(IReadonlyNodeSet selected);

        TTransitionUI UIInfo(Output connection, bool canFail);

        event Action NodesDeleted;
        event Action<TNode> NodeAdded;
        event Action<TNode> NodeRemoved;
        int RelativePosition(TNode ofNode, TNode relativeTo);

        TNode MakeNode(IConversationNodeData e, NodeUIData uiData);
    }
}
