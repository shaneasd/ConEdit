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

        public bool Remove(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups, ILocalizationEngine localization)
        {
            throw new NotImplementedException();
        }

        public IEnumerableReversible<ConversationNode> Nodes
        {
            get { return EnumerableReversible.Empty<ConversationNode>(); }
        }

        public IEnumerableReversible<NodeGroup> Groups
        {
            get { return EnumerableReversible.Empty<NodeGroup>(); }
        }

        public void RemoveLinks(Output o)
        {
            throw new NotImplementedException();
        }

        public ReadOnlyCollection<LoadError> Errors
        {
            get { throw new NotImplementedException(); }
        }

        public void ClearErrors()
        {
            throw new NotImplementedException();
        }

        bool IInProject.CanRemove(Func<bool> prompt)
        {
            throw new NotImplementedException();
        }

        void IInProject.Removed()
        {
            //Do nothing
        }

        ISaveableFile ISaveableFileProvider.File
        {
            get { throw new NotImplementedException(); }
        }

        ISaveableFileUndoable ISaveableFileUndoableProvider.UndoableFile
        {
            get { throw new NotImplementedException(); }
        }

        public void BringToFront(IReadonlyNodeSet Selected)
        {
            throw new NotImplementedException();
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


        public TransitionNoduleUIInfo UIInfo(Output connection, bool canFail)
        {
            throw new NotImplementedException();
        }

        public ConversationNode GetNode(Id<NodeTemp> id)
        {
            throw new NotImplementedException();
        }


        public Tuple<IEnumerable<ConversationNode>, IEnumerable<NodeGroup>> DuplicateInto(IEnumerable<GraphAndUI<NodeUIData>> nodeData, IEnumerable<NodeGroup> groups, object documentID, System.Drawing.PointF location, ILocalizationEngine localization)
        {
            throw new NotImplementedException();
        }

        public void Add(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups, ILocalizationEngine localization)
        {
            throw new NotImplementedException();
        }

        public event Action NodesDeleted { add { } remove { } }
        public event Action<ConversationNode> NodeAdded { add { } remove { } }
        public event Action<ConversationNode> NodeRemoved { add { } remove { } }


        public ConversationNode MakeNode(IConversationNodeData e, NodeUIData uiData)
        {
            throw new NotImplementedException();
        }

        public int RelativePosition(ConversationNode ofNode, ConversationNode relativeTo)
        {
            throw new NotImplementedException();
        }
    }
}
