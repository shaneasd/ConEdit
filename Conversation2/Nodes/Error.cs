using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class Error : Node
    {
        public Error(uint id) : base(id, EditableType.Error) { }

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
            get { return "Error"; }
        }
    }
}
