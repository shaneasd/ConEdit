using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class Trigger : ExternalFunction
    {
        public Trigger(uint id, string name, params Parameter[] parameters) : base(id,EditableType.Trigger, name, true, true, parameters) { }
        public override string Name { get { return Constants.First(p => p.Name == "FunctionName").ToString(); } }
    }
}
