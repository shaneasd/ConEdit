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
    using ConversationNode = ConversationNode<INodeGui>;

    public interface IConversationEditorControlData<TNode, TTransitionUI> : ISaveableFileUndoableProvider where TNode : IRenderable<IGui>
    {
        TNode GetNode(Id<NodeTemp> id);

        /// <summary>
        /// Insert nodes/groups into the document without invalidly duplicating ids from the source
        /// All local dynamic enum parameters are shifted to the new document source
        /// All localized strings updated to point to duplicated string data
        /// TODO: AUDIO: something probably happens to audio?
        /// All new ids are generates for all nodes groups contents are updated accordingly
        /// Nodes and groups are shifted so the center of their bounding rectangle is at the input location
        /// </summary>
        /// <param name="nodeData">All required information about nodes to duplicate</param>
        /// <param name="groups">All required information about groups to insert</param>
        /// <param name="location">Location at which to insert the new objects (center of the bounding rectangle for the nodes/groups)</param>
        /// <param name="localization">Localization source</param>
        /// <returns>All nodes/groups inserted</returns>
        Tuple<IEnumerable<TNode>, IEnumerable<NodeGroup>> DuplicateInto(IEnumerable<GraphAndUI<NodeUIData>> nodeData, IEnumerable<NodeGroup> groups, PointF location, ILocalizationEngine localization);

        /// <summary>
        /// Insert nodes/groups into the document 
        /// Nodes and groups are shifted so the center of their bounding rectangle is at the input location
        /// </summary>
        /// <param name="nodeData">All required information about nodes to duplicate</param>
        /// <param name="groups">All required information about groups to insert</param>
        /// <param name="location">Location at which to insert the new objects (center of the bounding rectangle for the nodes/groups)</param>
        /// <param name="localization">Localization source</param>
        /// <returns>All nodes/groups inserted</returns>
        Tuple<IEnumerable<TNode>, IEnumerable<NodeGroup>> InsertInto(IEnumerable<GraphAndUI<NodeUIData>> nodeData, IEnumerable<NodeGroup> groups, PointF location, ILocalizationEngine localization);
        void Add(IEnumerable<TNode> nodes, IEnumerable<NodeGroup> groups, ILocalizationEngine localization);
        bool Remove(IEnumerable<TNode> nodes, IEnumerable<NodeGroup> groups, ILocalizationEngine localization);
        void Move(IEnumerable<ValueTuple<TNode, PointF>> move);
        IEnumerableReversible<TNode> Nodes { get; }
        IEnumerableReversible<NodeGroup> Groups { get; }
        void RemoveLinks(Output o);
        /// <summary>
        /// Issues that were detected in deserialization that can be automatically resolved but with possible loss of data
        /// e.g. removing links that point to non-existent nodes
        /// </summary>
        ReadOnlyCollection<LoadError> Errors { get; } //TODO: This could possibly be implemented using an Error : IConversationEditorControlData class.
        void ClearErrors();

        void BringToFront(IReadOnlyNodeSet selected);

        TTransitionUI UIInfo(Output connection, bool canFail);

        event Action NodesDeleted;
        event Action<TNode> NodeAdded;
        event Action<TNode> NodeRemoved;
        int RelativePosition(TNode ofNode, TNode relativeTo);

        TNode MakeNode(IConversationNodeData e, NodeUIData uiData);
    }
}
