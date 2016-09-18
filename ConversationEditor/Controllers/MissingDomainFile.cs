using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Conversation;
using Utilities;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGui>;
    using System.Collections.ObjectModel;

    internal sealed class MissingDomainFile : IDomainFile
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

        bool IInProject.CanRemove(Func<bool> prompt)
        {
            //Doesn't care
            return true;
        }

        void IInProject.Removed()
        {
            //Do nothing
        }

        public bool Remove(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups)
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

        public IDomainData Data
        {
            get
            {
                //Not a real domain file so supplies no domain info
                return new DomainData();
            }
        }

        public ReadOnlyCollection<Conversation.Serialization.LoadError> Errors
        {
            get { return new ReadOnlyCollection<Conversation.Serialization.LoadError>(new Conversation.Serialization.LoadError[0]); }
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

        public TransitionNoduleUIInfo UIInfo(Output connection, bool canFail)
        {
            throw new NotImplementedException();
        }

        public ConversationNode GetNode(Id<NodeTemp> id)
        {
            throw new NotImplementedException();
        }

        public void Add(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups)
        {
            throw new NotImplementedException();
        }

        public event Action NodesDeleted { add { } remove { } }


        public ConversationNode MakeNode(IConversationNodeData e, NodeUIData uiData)
        {
            throw new NotImplementedException();
        }

        public event Action ConversationDomainModified { add { } remove { } }
        public event Action<ConversationNode> NodeAdded { add { } remove { } }
        public event Action<ConversationNode> NodeRemoved { add { } remove { } }


        public Tuple<IEnumerable<ConversationNode>, IEnumerable<NodeGroup>> DuplicateInto(IEnumerable<Conversation.Serialization.GraphAndUI<NodeUIData>> nodeData, IEnumerable<NodeGroup> groups, object documentID, System.Drawing.PointF location, ILocalizationEngine localization)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> AutoCompleteSuggestions(IParameter p, string s, Func<ParameterType, DynamicEnumParameter.Source> enumSource)
        {
            return Enumerable.Empty<string>(); //Missing file can't make suggestions but will probably be queried anyway
        }

        public int RelativePosition(ConversationNode ofNode, ConversationNode relativeTo)
        {
            throw new NotImplementedException();
        }
    }
}
