using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Utilities
{
    public static class UndoUtil
    {
        public static SimpleUndoPair Actions(this UndoAction a)
        {
            return new SimpleUndoPair { Undo = a.Undo, Redo = a.Redo };
        }
    }

    public abstract class UndoAction
    {
        public abstract void Undo();
        public abstract void Redo();
        public abstract string Description { get; }

        public static UndoAction NeverSaved { get; } = new DummyUndoAction();

        private class DummyUndoAction : UndoAction
        {
            public override void Undo() { }
            public override void Redo() { }
            public override string Description { get { return ""; } }
        }
    }



    public struct SimpleUndoPair
    {
        public Action Undo { get; set; }
        public Action Redo { get; set; }
    }

    public class GenericUndoAction : UndoAction
    {
        private Action m_undo;
        private Action m_redo;
        private string m_description;

        public GenericUndoAction(SimpleUndoPair undoredo, string description)
        {
            m_undo = undoredo.Undo;
            m_redo = undoredo.Redo;
            m_description = description;
        }

        public GenericUndoAction(Action undo, Action redo, string description)
        {
            m_undo = undo;
            m_redo = redo;
            m_description = description;
        }

        public override void Undo()
        {
            m_undo.Execute();
        }

        public override void Redo()
        {
            m_redo.Execute();
        }

        public override string Description { get { return m_description; } }
    }

    //public class MoveAction : UndoAction
    //{
    //    private Stack<Action> m_actions = new Stack<Action>();
    //    private object m_moved;
    //    private Action m_final;
    //    private string m_description;

    //    public MoveAction(object moved, Action first, Action final, string description)
    //    {
    //        m_moved = moved;
    //        m_actions.Push(first);
    //        m_final = final;
    //        m_description = description;
    //    }

    //    public override void Undo()
    //    {
    //        foreach (Action action in m_actions)
    //            action.Execute();
    //        m_final.Execute();
    //    }

    //    public override void MergeInto(List<UndoAction> other)
    //    {
    //        MoveAction move = other.LastOrDefault() as MoveAction;
    //        if (move != null && move.m_moved.Equals(m_moved) && move.m_final == m_final)
    //        {
    //            foreach (var action in m_actions)
    //                move.m_actions.Push(action);
    //        }
    //        else
    //        {
    //            other.Add(this);
    //        }
    //    }

    //    public override string Description { get { return m_description; } }
    //}
}
