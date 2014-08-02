using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class Condition : ExternalFunction
    {
        public Condition(uint id, string name, params Parameter[] parameters) : base(id, EditableType.Condition, name, true, true, parameters) { }

        public override string Name { get { return Constants.First(p => p.Name == "FunctionName").ToString(); } }
    }

    //public class Custom : ExternalFunction
    //{
    //    public Custom(uint id, string name, params Parameter[] parameters) : base(id, EditableType.Custom, name, true, true, parameters) { }

    //    public override string Name { get { return Constants.First(p => p.Name == "FunctionName").ToString(); } }
    //}
}
