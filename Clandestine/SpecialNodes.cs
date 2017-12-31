﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace Clandestine
{
    internal static class SpecialNodes
    {
        public static Id<NodeTypeTemp> Start => Id<NodeTypeTemp>.Parse("656a48b2-324e-4484-a1b1-c3b91ad10c3e");
        public static Id<NodeTypeTemp> StartRadio => Id<NodeTypeTemp>.Parse("9d485bf4-3e7a-4562-b4c2-33b7a881f702");
        public static Id<NodeTypeTemp> ToDo => Id<NodeTypeTemp>.Parse("3a0cf660-90d4-4e06-9d61-6769c3b93211");
        public static Id<NodeTypeTemp> Terminator => Id<NodeTypeTemp>.Parse("b2626790-c010-43d8-b1fb-d2093fd9328c");
        public static Id<NodeTypeTemp> Option => Id<NodeTypeTemp>.Parse("86524441-8da7-4e19-9ff3-c8df67e09f8f");
        public static Id<NodeTypeTemp> Random => Id<NodeTypeTemp>.Parse("03f66e93-bc0c-4713-ae07-dd249d85078d");
        public static Id<NodeTypeTemp> Branch => Id<NodeTypeTemp>.Parse("4a77a85d-306b-44e1-97d2-c23843eaf0b0");
        public static Id<NodeTypeTemp> JumpTo => Id<NodeTypeTemp>.Parse("dd02c832-4896-4f25-9a49-3884d6146fa3");
        public static Id<NodeTypeTemp> JumpTarget => Id<NodeTypeTemp>.Parse("17cf0309-8377-4e3e-9d8d-46d0a7cef943");
    }
}
