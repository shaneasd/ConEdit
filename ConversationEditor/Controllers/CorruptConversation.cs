using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.IO;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGUI<TransitionNoduleUIInfo>, TransitionNoduleUIInfo>;
    using ConversationEditor.Serialization;

    public class CorruptConversationFile : IConversationFile
    {
        private ConversationFile m_decorrupted;
        public readonly List<Error> Errors;
        public CorruptConversationFile(ConversationFile decorrupted, List<Error> errors)
        {
            m_decorrupted = decorrupted;
            Errors = errors;
        }

        public FileInfo File
        {
            get { return m_decorrupted.File; }
        }

        public bool Save()
        {
            return true; //Can't be edited so no need to save
        }

        public bool SaveAs(FileStream newFile)
        {
            //Cannot save without reverting the corruption
            throw new NotImplementedException();
        }

        public void Move(FileInfo newPath, Func<bool> replace)
        {
            throw new NotImplementedException("TODO: Probably need to implement this");
        }

        public bool Changed
        {
            get { return false; }
        }

        public bool CanClose()
        {
            throw new NotImplementedException();
        }

        public void ForceClose()
        {
            throw new NotImplementedException();
        }

        public event Action Modified;

        public event Action<FileInfo, FileInfo> Moved;

        public void Add(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup<ConversationNode>> groups)
        {
            throw new NotImplementedException();
        }

        public void Remove(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup<ConversationNode>> groups)
        {
            throw new NotImplementedException();
        }

        public void Change(Utilities.UndoAction revert)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ConversationNode> Nodes
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<NodeGroup<ConversationNode>> Groups
        {
            get { throw new NotImplementedException(); }
        }

        public void RemoveLinks(ITransitionOutNode<ConversationNode, TransitionNoduleUIInfo> o)
        {
            throw new NotImplementedException();
        }

        public void RemoveLinks(ITransitionInNode<ConversationNode, TransitionNoduleUIInfo> o)
        {
            throw new NotImplementedException();
        }

        public Utilities.UndoQueue UndoQueue
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReal
        {
            get { throw new NotImplementedException(); }
        }
    }
}
