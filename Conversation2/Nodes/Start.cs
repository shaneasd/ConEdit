using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class Start : Node
    {
        public Start(uint id) : base(id, EditableType.Start) { }

        public override IEnumerable<Parameter> Constants
        {
            get { return Enumerable.Empty<Parameter>(); }
        }

        public override IEnumerable<Parameter> Parameters
        {
            get { return Enumerable.Empty<Parameter>(); }
        }

        public override bool Input { get { return false; } }
        public override IEnumerable<Output> Outputs { get { yield return new Output(string.Empty); } }

        public override string Name
        {
            get { return "Start"; }
        }
    }
}
