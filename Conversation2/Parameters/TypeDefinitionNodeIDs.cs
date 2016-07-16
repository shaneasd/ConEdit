using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public static class TypeDefinitionNodeIds
    {
        public static Id<NodeTypeTemp> Set { get; } = Id<NodeTypeTemp>.Parse("177bc471-db66-4cf1-a8fb-46ef968f2bc8");
        public static Id<NodeTypeTemp> Enumeration { get; } = Id<NodeTypeTemp>.Parse("05e2be46-7feb-48ed-a66f-2e81153cfd4b");
        public static Id<NodeTypeTemp> Integer { get; } = Id<NodeTypeTemp>.Parse("e8d6ea71-382c-446b-828f-c1bc3a6065d2");
        public static Id<NodeTypeTemp> Decimal { get; } = Id<NodeTypeTemp>.Parse("eb6c7951-c165-4a1d-bbe9-306a9c397482");
        public static Id<NodeTypeTemp> DynamicEnumeration { get; } = Id<NodeTypeTemp>.Parse("100ea79f-7a12-494a-a816-790d054cabd1");
        public static Id<NodeTypeTemp> LocalDynamicEnumeration { get; } = Id<NodeTypeTemp>.Parse("22751578-3c89-4199-8a29-018c2a67e1aa");

        public static IEnumerable<Id<NodeTypeTemp>> All
        {
            get
            {
                yield return Set;
                yield return Enumeration;
                yield return Integer;
                yield return Decimal;
                yield return DynamicEnumeration;
                yield return LocalDynamicEnumeration;
            }
        }
    }
}
