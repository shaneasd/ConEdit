using System;
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
    using ConversationNode = ConversationNode<INodeGUI>;

    public interface IConversationEditorControlData<TNode, TTransitionUI> : ISaveableFileUndoableProvider where TNode : IRenderable<IGUI>
    {
        TNode GetNode(ID<NodeTemp> id);
        Tuple<IEnumerable<TNode>, IEnumerable<NodeGroup>> DuplicateInto(IEnumerable<GraphAndUI<NodeUIData>> nodeData, IEnumerable<NodeGroup> groups, PointF location, ILocalizationEngine localization);
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

        void BringToFront(IReadonlyNodeSet Selected);

        TTransitionUI UIInfo(Output connection);

        event Action NodesDeleted;

        TNode MakeNode(IEditable e, NodeUIData uiData);
    }
}
