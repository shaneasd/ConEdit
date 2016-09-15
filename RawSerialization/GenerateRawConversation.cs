//using ConversationEditor;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Conversation;
//using System.IO;
//using System.Windows.Forms;

//namespace RawSerialization
//{
//    class GenerateRawConversation : IConversationContextMenuItem
//    {
//        public string Name { get { return "Generate Binary Conversation"; } }

//        private Stream DetermineFileStream()
//        {
//            using (SaveFileDialog sfd = new SaveFileDialog())
//            {
//                sfd.DefaultExt = "bincon";
//                sfd.AddExtension = true;
//                //TODO: Add file pattern
//                if (sfd.ShowDialog() == DialogResult.OK)
//                {
//                    return sfd.OpenFile();
//                }
//                else
//                {
//                    return null;
//                }
//            }
//        }

//        public void Execute(IConversationFile conversation, IErrorCheckerUtilities<IConversationNode> util)
//        {
//            using (Stream stream = DetermineFileStream())
//            {
//                Write(conversation, stream);
//            }
//        }

//        public static void Write(IConversationFile conversation, Stream stream)
//        {
//            //foreach (var node in conversation.Nodes)
//            //{
//            //    node.
//            //}
//        }
//    }
//}
