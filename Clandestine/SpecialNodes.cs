using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace Clandestine
{
    public static class SpecialNodes
    {
        public static readonly ID<NodeTypeTemp> START_GUID = ID<NodeTypeTemp>.Parse("656a48b2-324e-4484-a1b1-c3b91ad10c3e");
        public static readonly ID<NodeTypeTemp> START_RADIO_GUID = ID<NodeTypeTemp>.Parse("9d485bf4-3e7a-4562-b4c2-33b7a881f702");
        public static readonly ID<NodeTypeTemp> TODO_GUID = ID<NodeTypeTemp>.Parse("3a0cf660-90d4-4e06-9d61-6769c3b93211");
        public static readonly ID<NodeTypeTemp> TERMINATOR_GUID = ID<NodeTypeTemp>.Parse("b2626790-c010-43d8-b1fb-d2093fd9328c");
        public static readonly ID<NodeTypeTemp> JUMP_TO_GUID = ID<NodeTypeTemp>.Parse("dd02c832-4896-4f25-9a49-3884d6146fa3");
        public static readonly ID<NodeTypeTemp> JUMP_TARGET_GUID = ID<NodeTypeTemp>.Parse("17cf0309-8377-4e3e-9d8d-46d0a7cef943");
        public static readonly ID<NodeTypeTemp> OPTION_GUID = ID<NodeTypeTemp>.Parse("86524441-8da7-4e19-9ff3-c8df67e09f8f");
        public static readonly ID<NodeTypeTemp> RANDOM_GUID = ID<NodeTypeTemp>.Parse("03f66e93-bc0c-4713-ae07-dd249d85078d");
        public static readonly ID<NodeTypeTemp> CONVERSATION_INFO_GUID = ID<NodeTypeTemp>.Parse("d5974ffe-777b-419c-b9bc-bde980cb99a6");
        public static readonly ID<NodeTypeTemp> BRANCH = ID<NodeTypeTemp>.Parse("4a77a85d-306b-44e1-97d2-c23843eaf0b0");
    }
}
