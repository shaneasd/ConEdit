﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using System.Collections.ObjectModel;

namespace ConversationEditor
{
    internal sealed class DummyConversationEditorControlData<TNode, TTransitionUI> : IConversationEditorControlData<TNode, TTransitionUI> where TNode : IRenderable<IGui>
    {
        public static IConversationEditorControlData<TNode, TTransitionUI> Instance = new DummyConversationEditorControlData<TNode, TTransitionUI>();
        private DummyConversationEditorControlData() { }

        public bool Remove(IEnumerable<TNode> nodes, IEnumerable<NodeGroup> groups, ILocalizationEngine localization)
        {
            throw new NotImplementedException();
        }

        public IEnumerableReversible<TNode> Nodes
        {
            get { return EnumerableReversible.Empty<TNode>(); }
        }

        public IEnumerableReversible<NodeGroup> Groups
        {
            get { return EnumerableReversible.Empty<NodeGroup>(); }
        }

        public void RemoveLinks(Output o)
        {
            throw new NotImplementedException();
        }

        ISaveableFileUndoable ISaveableFileUndoableProvider.UndoableFile
        {
            get { throw new NotImplementedException(); }
        }

        ISaveableFile ISaveableFileProvider.File { get { return new MissingFile(null); } }


        public ReadOnlyCollection<Conversation.Serialization.LoadError> Errors
        {
            get { throw new NotImplementedException(); }
        }

        public void ClearErrors()
        {
            throw new NotImplementedException();
        }

        public void BringToFront(IReadonlyNodeSet Selected)
        {
        }

        public event Action FileModifiedExternally
        {
            add { (this as ISaveableFileProvider).File.FileModifiedExternally += value; }
            remove { (this as ISaveableFileProvider).File.FileModifiedExternally -= value; }
        }

        public event Action FileDeletedExternally
        {
            add { (this as ISaveableFileProvider).File.FileDeletedExternally += value; }
            remove { (this as ISaveableFileProvider).File.FileDeletedExternally -= value; }
        }

        public void Dispose()
        {
        }

        public TTransitionUI UIInfo(Output connection, bool canFail)
        {
            throw new NotImplementedException();
        }

        public TNode GetNode(Id<NodeTemp> id)
        {
            throw new NotImplementedException();
        }

        public void Add(IEnumerable<TNode> nodes, IEnumerable<NodeGroup> groups, ILocalizationEngine localization)
        {
            throw new NotImplementedException();
        }

        Tuple<IEnumerable<TNode>, IEnumerable<NodeGroup>> IConversationEditorControlData<TNode, TTransitionUI>.DuplicateInto(IEnumerable<Conversation.Serialization.GraphAndUI<NodeUIData>> nodeData, IEnumerable<NodeGroup> groups, object documentID, System.Drawing.PointF location, ILocalizationEngine localization)
        {
            throw new NotImplementedException();
        }

        public event Action NodesDeleted { add { } remove { } }
        public event Action<TNode> NodeAdded { add { } remove { } }
        public event Action<TNode> NodeRemoved { add { } remove { } }

        public TNode MakeNode(IConversationNodeData e, NodeUIData uiData)
        {
            throw new NotImplementedException();
        }

        public int RelativePosition(TNode ofNode, TNode relativeTo)
        {
            throw new NotImplementedException();
        }
    }
}
