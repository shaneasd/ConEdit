using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class Choice : Node
    {
        public Choice(uint id) : base(id, EditableType.Choice) { }

        public override IEnumerable<Parameter> Constants
        {
            get { return Enumerable.Empty<Parameter>(); }
        }

        public override IEnumerable<Parameter> Parameters
        {
            get { return Enumerable.Empty<Parameter>(); }
        }

        public override bool Input { get { return true; } }
        public override IEnumerable<Output> Outputs { get { return Enumerable.Empty<Output>(); } }

        public override string Name
        {
            get { return "Choice"; }
        }
    }
}
