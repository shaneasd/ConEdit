using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class JumpTo : Node
    {
        public readonly DynamicEnumParameter Parameter;
        public JumpTo(uint id, DynamicEnumParameter.Source source) : base(id, EditableType.JumpTo) 
        {
            Parameter = new DynamicEnumParameter("To", source, "JumpTarget");
        }

        public override IEnumerable<Parameter> Constants
        {
            get { return Enumerable.Empty<Parameter>(); }
        }

        public override IEnumerable<Parameter> Parameters
        {
            get { return new Parameter[] { Parameter }; }
        }

        public string Target { get { return Parameter.Value; } }

        public override string Name
        {
            get { return "Jump To"; }
        }
    }

    public class JumpTarget : Node
    {
        public readonly DynamicEnumParameter Parameter;
        public JumpTarget(uint id, DynamicEnumParameter.Source source)
            : base(id, EditableType.JumpTarget)
        {
            Parameter = new DynamicEnumParameter("Name", source, "JumpTarget");
        }

        public override IEnumerable<Parameter> Constants
        {
            get { return Enumerable.Empty<Parameter>(); }
        }

        public override IEnumerable<Parameter> Parameters
        {
            get { return new Parameter[] { Parameter }; }
        }

        public string Target { get { return Parameter.Value; } }

        public override string Name
        {
            get { return "Jump Target"; }
        }
    }
}
