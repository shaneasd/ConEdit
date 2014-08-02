using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Conversation;
using Utilities;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGUI>;

    public class MissingDomainFile : IDomainFile
    {
        private MissingFile m_file;

        public MissingDomainFile(FileInfo file)
        {
            m_file = new MissingFile(file);
        }

        ISaveableFile ISaveableFileProvider.File
        {
            get { return m_file; }
        }

        ISaveableFileUndoable ISaveableFileUndoableProvider.UndoableFile
        {
            get { throw new NotSupportedException("Shouldn't be trying to queue modifications to a missing file"); }
        }

        void IInProject.Removed()
        {
            //Doesn't care
        }

        public void Remove(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups)
        {
            throw new NotImplementedException();
        }

        public void Change(Utilities.UndoAction revert)
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

        public Utilities.UndoQueue UndoQueue
        {
            get { return new UndoQueue(); }
        }

        public DomainData Data
        {
            get
            {
                //Not a real domain file so supplies no domain info
                return new DomainData();
            }
        }

        public List<Conversation.Serialization.Error> Errors
        {
            get { return new List<Conversation.Serialization.Error>(); }
        }

        public void ClearErrors()
        {
            throw new NotImplementedException();
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
            m_file.Dispose();
        }

        public TransitionNoduleUIInfo UIInfo(Output connection)
        {
            throw new NotImplementedException();
        }

        public ConversationNode GetNode(ID<NodeTemp> id)
        {
            throw new NotImplementedException();
        }

        public Tuple<IEnumerable<ConversationNode>, IEnumerable<NodeGroup>> DuplicateInto(IEnumerable<Conversation.Serialization.GraphAndUI<NodeUIData>> nodeData, IEnumerable<NodeGroup> groups, System.Drawing.PointF location, LocalizationEngine localization)
        {
            throw new NotImplementedException();
        }

        public void Add(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups)
        {
            throw new NotImplementedException();
        }

        public event Action NodesDeleted { add { } remove { } }


        public ConversationNode MakeNode(IEditable e, NodeUIData uiData)
        {
            throw new NotImplementedException();
        }
    }
}
