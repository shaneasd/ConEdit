using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class Todo : Node
    {
        public Todo(uint id) : base(id, EditableType.Todo) { }

        public override IEnumerable<Parameter> Constants
        {
            get { return Enumerable.Empty<Parameter>(); }
        }

        private StringParameter m_todoText = new StringParameter("TODO");

        public override IEnumerable<Parameter> Parameters
        {
            get { yield return m_todoText; }
        }

        public override string Name
        {
            get { return "TODO"; }
        }
    }
}
