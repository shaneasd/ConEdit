using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class Terminator : Node
    {
        public Terminator(uint id) : base(id, EditableType.Terminator) { }

        public override IEnumerable<Parameter> Constants
        {
            get { return Enumerable.Empty<Parameter>(); }
        }

        public override IEnumerable<Parameter> Parameters
        {
            get { return Enumerable.Empty<Parameter>(); }
        }

        public override string Name
        {
            get { return "Terminator"; }
        }
    }
}
