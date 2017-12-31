using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    //TODO: This shouldn't really exist at this level
    public static class SpecialNodes
    {
        //It's used for WillRender in UIs and NodeRendererChoice default
        public static Id<NodeTypeTemp> Start => Id<NodeTypeTemp>.Parse("656a48b2-324e-4484-a1b1-c3b91ad10c3e");
        public static Id<NodeTypeTemp> Terminator => Id<NodeTypeTemp>.Parse("b2626790-c010-43d8-b1fb-d2093fd9328c");
    }
}
