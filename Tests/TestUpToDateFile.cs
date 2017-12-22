using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Tests
{
    public static class TestUpToDateFile
    {
        [NUnit.Framework.Test]
        public static void StressTest()
        {
            MemoryStream data = new MemoryStream();
            using (StreamWriter w = new StreamWriter(data, Encoding.UTF8, 1024, true))
            {
                w.WriteLine("asgfladhfldsalkhdsfdsfasf");
            }
            data.Position = 0;

            Action<Stream> saveto = s =>
            {
                data.Position = 0;
                data.CopyTo(s);
                data.Position = 0;
            };

            using (FileStream file = new FileStream("test.txt", FileMode.OpenOrCreate, FileAccess.Write))
            {
                saveto(file);
            }

            using (UpToDateFile.BackEnd backEnd = new UpToDateFile.BackEnd())
            {
                using (UpToDateFile f = new UpToDateFile(data, new FileInfo("test.txt"), saveto, backEnd))
                {
                    f.FileChanged += () =>
                    {
                        Assert.Fail("FileChange should not have triggered");
                    };

                    try
                    {
                        for (int i = 0; i < 200; i++)
                        {

                            f.Save();
                            f.Save();

                            data.Position = 10;
                            data.WriteByte((byte)(i % 256));

                            f.Save();
                            Console.WriteLine(i);
                        }
                    }
                    catch (MyFileLoadException)
                    {
                        Assert.Fail();
                    }
                }
            }
        }
    }
}
