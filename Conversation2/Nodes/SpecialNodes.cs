using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    //TODO: This shouldn't really exist at this level
    public static class SpecialNodes
    {
        public static readonly ID<NodeTypeTemp> START_GUID = ID<NodeTypeTemp>.Parse("656a48b2-324e-4484-a1b1-c3b91ad10c3e");
        public static readonly ID<NodeTypeTemp> TERMINATOR_GUID = ID<NodeTypeTemp>.Parse("b2626790-c010-43d8-b1fb-d2093fd9328c");
    }
}
