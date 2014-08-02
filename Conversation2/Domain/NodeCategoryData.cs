using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public struct NodeTypeData
    {
        public NodeTypeData(string name, Guid guid, Guid parent)
        {
            Name = name;
            Guid = guid;
            Parent = parent;
        }
        public readonly string Name;
        public readonly Guid Guid;
        public readonly Guid Parent;
    }

}
