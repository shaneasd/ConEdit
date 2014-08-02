using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class Import : Node
    {
        private FilePathParameter m_parameter = new FilePathParameter("Conversation");

        public Import(uint id) : base(id, EditableType.Import) { }

        public override IEnumerable<Parameter> Constants
        {
            get { return Enumerable.Empty<Parameter>(); }
        }

        public override IEnumerable<Parameter> Parameters
        {
            get { return new Parameter[] { m_parameter }; }
        }

        public override string Name
        {
            get { return "Import"; }
        }
    }
}
