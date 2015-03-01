using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using NUnit.Framework;
using System.IO;

namespace Tests
{
    class TestUndoQueue
    {
        [NUnit.Framework.Test]
        public static void TestEverything()
        {
            SaveableFileUndoable file = new SaveableFileUndoable(new MemoryStream(), new FileInfo("ignore.txt"), a => { });
            Assert.False(file.Changed);
            file.Change(new GenericUndoAction(() => { }, () => { }, ""));
            Assert.True(file.Changed);
            file.UndoQueue.Undo();
            Assert.False(file.Changed);
            file.UndoQueue.Redo();
            Assert.True(file.Changed);
            file.Save();
            Assert.False(file.Changed);
            file.Change(new GenericUndoAction(() => { }, () => { }, ""));
            Assert.True(file.Changed);
            file.UndoQueue.Undo();
            Assert.False(file.Changed);
            file.UndoQueue.Undo();
            Assert.True(file.Changed);
            file.UndoQueue.Redo();
            Assert.False(file.Changed);
            file.Dispose();
        }
    }
}
