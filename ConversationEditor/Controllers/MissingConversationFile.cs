﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Conversation;
using Conversation.Serialization;
using Utilities;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGUI>;

    public class MissingConversationFile : IConversationFile
    {
        private MissingFile m_file;
        public MissingConversationFile(FileInfo file) 
        {
            m_file = new MissingFile(file);
        }

        public UndoQueue UndoQueue
        {
            get { return new UndoQueue(); }
        }

        public IEnumerableReversible<ConversationNode> Nodes
        {
            get { return EnumerableReversible.Empty<ConversationNode>(); }
        }

        public IEnumerableReversible<NodeGroup> Groups
        {
            get { return EnumerableReversible.Empty<NodeGroup>(); }
        }

        public void Remove(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups)
        {
            throw new NotImplementedException();
        }

        public void Change(Utilities.UndoAction revert)
        {
            throw new NotImplementedException();
        }

        public void RemoveLinks(Output o)
        {
            throw new NotImplementedException();
        }

        public List<Error> Errors
        {
            get { return new List<Error>(); } //A missing file inherently has no errors within its data (though it is a project error to have missing files)
        }

        public void ClearErrors()
        {
            throw new NotImplementedException();
        }

        public bool CanRemove(Func<bool> prompt)
        {
            //Doesn't care
            return true;
        }

        void IInProject.Removed()
        {
            //Do nothing
        }

        public ISaveableFile File
        {
            get { return m_file; }
        }

        ISaveableFileUndoable ISaveableFileUndoableProvider.UndoableFile
        {
            get { throw new NotSupportedException("Shouldn't be queuing up modifications to a file that isn't there"); }
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

        public Tuple<IEnumerable<ConversationNode>, IEnumerable<NodeGroup>> DuplicateInto(IEnumerable<GraphAndUI<NodeUIData>> nodeData, IEnumerable<NodeGroup> groups, System.Drawing.PointF location, LocalizationEngine localization)
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
