using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public interface IConfigNodeDefinition
    {
        ID<NodeTypeTemp> Id { get; }
        string Name { get; }
        IEnumerable<Parameter> MakeParameters();
    }
}
