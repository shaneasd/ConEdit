using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public static class TypeDefinitionNodeIDs
    {
        public static readonly ID<NodeTypeTemp> Enumeration = ID<NodeTypeTemp>.Parse("05e2be46-7feb-48ed-a66f-2e81153cfd4b");
        public static readonly ID<NodeTypeTemp> Integer = ID<NodeTypeTemp>.Parse("e8d6ea71-382c-446b-828f-c1bc3a6065d2");
        public static readonly ID<NodeTypeTemp> Decimal = ID<NodeTypeTemp>.Parse("eb6c7951-c165-4a1d-bbe9-306a9c397482");
        public static readonly ID<NodeTypeTemp> DynamicEnumeration = ID<NodeTypeTemp>.Parse("100ea79f-7a12-494a-a816-790d054cabd1");

        public static IEnumerable<ID<NodeTypeTemp>> All
        {
            get
            {
                yield return Enumeration;
                yield return Integer;
                yield return Decimal;
                yield return DynamicEnumeration;
            }
        }
    }
}
