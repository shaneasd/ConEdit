using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Conversation;

namespace ConversationEditor
{
    public class SplitGraphics : EditableGraphics
    {
        protected SplitGraphics()
            : base(null, null)
        {
        }

        //protected override NodeGraphics<EditableRenderer>.TransitionOutNode MakeTransitionOut()
        //{
        //    int outTransitions = m_transitionsOut.Count;

        //    Func<int> x = null;
        //    var t = new TransitionOutNode(this, new Output(""), () => new Rectangle(x(), Area.Bottom, 10, 10));
        //    x = () => Data.Area.Left + 30 * m_transitionsOut.IndexOf(t) + 10;
        //    t.Connected += () => { m_transitionsOut.Add(MakeTransitionOut()); m_updateDisplay = true; };
        //    t.Disconnected += () => { m_transitionsOut.Remove(t); m_updateDisplay = true; };

        //    return t;
        //}

        //public SplitGraphics(IEditable editable, Point location)
        //    : this(editable, location, new Size(30, 30))
        //{
        //}

        //public SplitGraphics(IEditable editable, Point location, Size size)
        //    : base(editable, location, size)
        //{
        //    m_transitionsIn.Add(MakeTransitionIn());
        //    m_transitionsOut.Add(MakeTransitionOut());
        //}

        //protected override void UpdateDisplay(Graphics g)
        //{
        //    m_size.Width = 30 * m_transitionsOut.Count;
        //}
    }
}
