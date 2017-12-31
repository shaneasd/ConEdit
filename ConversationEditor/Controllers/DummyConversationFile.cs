using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Conversation;
using Conversation.Serialization;
using Utilities;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGui>;
    using System.Collections.ObjectModel;

    internal sealed class DummyConversationFile : IConversationFile
    {
        public static IConversationFile Instance { get; } = new DummyConversationFile();
        private DummyConversationFile() { }

        public bool Remove(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups, ILocalizationEngine localization) => throw new NotSupportedException();

        public IEnumerableReversible<ConversationNode> Nodes => EnumerableReversible.Empty<ConversationNode>();

        public IEnumerableReversible<NodeGroup> Groups => EnumerableReversible.Empty<NodeGroup>();

        public void RemoveLinks(Output o) => throw new NotSupportedException();

        public ReadOnlyCollection<LoadError> Errors => throw new NotSupportedException();

        public void ClearErrors() => throw new NotSupportedException();

        bool IInProject.CanRemove(Func<bool> prompt) => throw new NotSupportedException();

        void IInProject.Removed()
        {
            //Do nothing
        }

        ISaveableFile ISaveableFileProvider.File => throw new NotSupportedException();

        ISaveableFileUndoable ISaveableFileUndoableProvider.UndoableFile => throw new NotSupportedException();

        public void BringToFront(IReadOnlyNodeSet Selected) => throw new NotSupportedException();

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


        public TransitionNoduleUIInfo UIInfo(Output connection, bool canFail) => throw new NotSupportedException();

        public ConversationNode GetNode(Id<NodeTemp> id) => throw new NotSupportedException();

        public Tuple<IEnumerable<ConversationNode>, IEnumerable<NodeGroup>> DuplicateInto(IEnumerable<GraphAndUI<NodeUIData>> nodeData, IEnumerable<NodeGroup> groups, object documentID, System.Drawing.PointF location, ILocalizationEngine localization)
        {
            throw new NotSupportedException();
        }

        public void Add(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups, ILocalizationEngine localization) => throw new NotSupportedException();

        public event Action NodesDeleted { add { } remove { } }
        public event Action<ConversationNode> NodeAdded { add { } remove { } }
        public event Action<ConversationNode> NodeRemoved { add { } remove { } }


        public ConversationNode MakeNode(IConversationNodeData e, NodeUIData uiData) => throw new NotSupportedException();

        public int RelativePosition(ConversationNode ofNode, ConversationNode relativeTo) => throw new NotSupportedException();

        Id<FileInProject> IInProject.Id => throw new NotSupportedException();
    }
}
