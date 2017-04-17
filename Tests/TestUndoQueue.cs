using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using NUnit.Framework;
using System.IO;

namespace Tests
{
    public static class TestUndoQueue
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Currently it won't. Can't garantee in the future but also can't find out if it was already disposed without making assumptions about its implementation")]
        [NUnit.Framework.Test]
        public static void TestEverything()
        {
            UpToDateFile.Backend backend = new UpToDateFile.Backend();
            using (MemoryStream m = new MemoryStream())
            {
                using (SaveableFileUndoable file = new SaveableFileUndoable(m, new FileInfo("ignore.txt"), a => { }, backend))
                {
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
                }
            }
        }
    }
}
