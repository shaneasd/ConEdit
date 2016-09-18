using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using ConversationEditor;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Tests.Conversation;

namespace Tests
{
    public static partial class Program
    {
        static void Main()
        {
            ParameterTypeTest.TestSet();
            UnknownEditableTest.Test();
            //EnumerationTest.ParameterEnum();
            //OutputTest.GetName();
            //TypeSetTest.Test();
            //TestReplace.TestReplaceOnce();
            //TestReplace.TestReplaceMany();
            //TestRawLocalization.TestDataIntegrity();
            //TestQuadTree.AllTests();
            //TestFileSystem.TestPathToFromDirDir();
            //TestWeakEvent();
            //TestPolynomial();
            //TestUndoQueue.TestEverything();
            //ManualResetEvent();
        }
    }
}
