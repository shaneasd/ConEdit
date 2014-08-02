//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.IO;
//using MyNamespace;

//namespace RuntimeConversation
//{
//    public class ExecuteConvo
//    {
//        void Execute(Stream stream)
//        {
//            Deserializer d = new Deserializer();
//            var convo = d.Read(stream);
//            MyNamespace.Nodes.Start start = convo.Nodes.OfType<MyNamespace.Nodes.Start>().First();
//            var node = start.id733c7b994f0d4c7bbb03170aefad47f5.Connections.First().Parent;
//            while (node != null)
//            {
//                Process(node);
//            }
//        }

//        private int EvaluateVariable(MyNamespace.Types.Variables v)
//        {
//            switch (v)
//            {
//                case MyNamespace.Types.Variables.MissionCasualties:
//                    return 13; //There were 13 mission casualties
//            }

//            throw new Exception("Unknown variable");
//        }

//        private Node GetConnectedNode(RuntimeConversation.Connector c)
//        {
//            if (c.Connections.Any())
//                return c.Connections.First().Parent;
//            else
//                return null;
//        }

//        private Node Process(Node node)
//        {
//            if (node is MyNamespace.Nodes.Compare)
//            {
//                var comp = node as MyNamespace.Nodes.Compare;
//                return Compare(comp);
//            }
//            else if (node is MyNamespace.Nodes.Diaglog)
//            {
//                var d = node as MyNamespace.Nodes.Diaglog;
//                Dialog(d);
//                return GetConnectedNode(d.id7834227bfaa5422a9730e386d452ed44);
//            }
//            else
//                throw new Exception("Unknown node type");
//        }

//        private void Dialog(MyNamespace.Nodes.Diaglog d)
//        {
//            SpeakLine(d.Line);
//        }

//        private void SpeakLine(string p)
//        {
//            throw new NotImplementedException();
//        }

//        private Node Compare(MyNamespace.Nodes.Compare comp)
//        {
//            if (EvaluateVariable(comp.A) == comp.B)
//                return GetConnectedNode(comp.id2f8369c7a75942c49753b2c032582d06);
//            else if (EvaluateVariable(comp.A) < comp.B)
//                return GetConnectedNode(comp.ide28641e097ac4ad58854f4711fdf826e);
//            else
//                return GetConnectedNode(comp.idbcda53ff88ac45c88c4b1018253589fe);
//        }
//    }
//}
