using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public interface IUndoQueueElement
    {
        string Description { get; }
        void UndoUpToAndIncluding();
    }

    /// <summary>
    /// Read access interface for an undo queue.
    /// Allows undoing/redoing but not queueing new items
    /// </summary>
    public interface IUndoQueue
    {
        void Undo();
        void Redo();
        bool HasUndo { get; }
        bool HasRedo { get; }
        IEnumerable<IUndoQueueElement> Elements { get; }
        event Action Changed;
    }

    public class NoUndoQueue : IUndoQueue
    {
        public void Undo()
        {
            throw new NotImplementedException();
        }

        public void Redo()
        {
            throw new NotImplementedException();
        }

        public bool HasUndo
        {
            get { return false; }
        }

        public bool HasRedo
        {
            get { return false; }
        }

        public IEnumerable<IUndoQueueElement> Elements
        {
            get { return Enumerable.Empty<IUndoQueueElement>(); }
        }

        public event Action Changed { add { } remove { } }
    }

    public class UndoQueue : IUndoQueue
    {
        /// <summary>
        /// This should be on the top of the undo queue for the file to be considered unmodified
        /// </summary>
        UndoAction m_undoSaved = null;
        Stack<UndoAction> m_undoActions = new Stack<UndoAction>();
        Stack<UndoAction> m_redoActions = new Stack<UndoAction>();

        private class Element : IUndoQueueElement
        {
            private readonly UndoQueue m_queue;
            public readonly UndoAction Action;
            public Element(UndoQueue queue, UndoAction action)
            {
                m_queue = queue;
                Action = action;
            }

            public string Description
            {
                get { return Action.Description; }
            }

            public void UndoUpToAndIncluding()
            {
                m_queue.UndoTo(this);
            }
        }

        public IEnumerable<IUndoQueueElement> Elements
        {
            get
            {
                return m_undoActions.Select(a => new Element(this, a));
            }
        }

        public event Action Changed;

        /// <summary>
        /// The associated file has been saved and so any existing items are effectively modifications from the saved state of the file if you undo them
        /// </summary>
        public void Saved()
        {
            bool modified = Modified;
            m_undoSaved = m_undoActions.Count != 0 ? m_undoActions.Peek() : null;
            if (modified)
                ModifiedChanged.Execute();
        }

        /// <summary>
        /// Indicates that no amount of undoing/redoing will get the undo queue into a saved state
        /// (This could occur, for example, if a file were modified outside the application.
        /// </summary>
        public void NeverSaved()
        {
            bool modified = Modified;
            m_undoSaved = UndoAction.NeverSaved;
            if (!modified)
                ModifiedChanged.Execute();
        }

        /// <summary>
        /// Is the current item at the top of the queue the most recent change before the most recent save?
        /// </summary>
        public bool Modified
        {
            get
            {
                if (m_undoActions.Count == 0)
                    return m_undoSaved != null;
                else if (m_undoSaved == m_undoActions.Peek())
                    return false;
                else
                    return true;
            }
        }

        public event Action ModifiedChanged;

        public void Queue(UndoAction action)
        {
            bool modified = Modified;
            m_redoActions.Clear();
            m_undoActions.Push(action);
            Changed.Execute();
            if (!modified)
                ModifiedChanged.Execute();
        }

        public void Undo()
        {
            if (m_undoActions.Any())
            {
                bool modified = Modified;
                var a = m_undoActions.Pop();
                a.Undo();
                m_redoActions.Push(a);
                if (modified != Modified)
                    ModifiedChanged.Execute();
                Changed.Execute();
            }
        }

        private void UndoTo(Element element)
        {
            while (m_undoActions.Any() && element.Action != m_undoActions.Peek())
            {
                Undo();
            }

            Undo();
        }

        public void Redo()
        {
            if (m_redoActions.Any())
            {
                bool modified = Modified;
                var a = m_redoActions.Pop();
                a.Redo();
                m_undoActions.Push(a);
                if (modified != Modified)
                    ModifiedChanged.Execute();
                Changed.Execute();
            }
        }

        public bool HasUndo { get { return m_undoActions.Any(); } }
        public bool HasRedo { get { return m_redoActions.Any(); } }
    }
}
