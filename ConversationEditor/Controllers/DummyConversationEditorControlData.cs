using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using System.Collections.ObjectModel;
using System.IO;

namespace ConversationEditor
{
    internal sealed class DummyConversationEditorControlData<TNode, TTransitionUI> : IConversationEditorControlData<TNode, TTransitionUI> where TNode : IRenderable<IGui>
    {
        public static IConversationEditorControlData<TNode, TTransitionUI> Instance = new DummyConversationEditorControlData<TNode, TTransitionUI>();
        private DummyConversationEditorControlData() { }

        public void Move(IEnumerable<ValueTuple<TNode, System.Drawing.PointF>> move) => throw new NotSupportedException();

        public bool Remove(IEnumerable<TNode> nodes, IEnumerable<NodeGroup> groups, ILocalizationEngine localization)
        {
            throw new NotSupportedException();
        }

        public IEnumerableReversible<TNode> Nodes => EnumerableReversible.Empty<TNode>();

        public IEnumerableReversible<NodeGroup> Groups => EnumerableReversible.Empty<NodeGroup>();

        public void RemoveLinks(Output o)
        {
            throw new NotSupportedException();
        }

        ISaveableFileUndoable ISaveableFileUndoableProvider.UndoableFile => throw new NotSupportedException();

        ISaveableFile ISaveableFileProvider.File => new MissingFile(Id<FileInProject>.FromGuid(Guid.Empty), DocumentPath.FromPath("", new DirectoryInfo(".")));

        public ReadOnlyCollection<Conversation.Serialization.LoadError> Errors => throw new NotSupportedException();

        public void ClearErrors()
        {
            throw new NotSupportedException();
        }

        public void BringToFront(IReadOnlyNodeSet Selected)
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
