using Conversation;
using ConversationEditor;
using NUnit.Framework;
using RawSerialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Tests
{
    class MockLocalizationFile : ILocalizationFile
    {
        private Func<Id<LocalizedText>, string> m_localize;

        public MockLocalizationFile(IEnumerable<Id<LocalizedText>> existingLocalizations, Func<Id<LocalizedText>, string> localize)
        {
            ExistingLocalizations = existingLocalizations;
            m_localize = localize;
        }

        public IEnumerable<Id<LocalizedText>> ExistingLocalizations { get; private set; }

        public string Localize(Id<LocalizedText> id)
        {
            return m_localize(id);
        }

        public ISaveableFile File
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsValid
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public event Action FileDeletedExternally { add { } remove { } }
        public event Action FileModifiedExternally { add { } remove { } }

        public bool CanRemove(Func<bool> prompt)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public void Removed()
        {
            throw new NotImplementedException();
        }

        public SimpleUndoPair SetLocalizationAction(Id<LocalizedText> guid, string p)
        {
            throw new NotImplementedException();
        }

        public DateTime LocalizationTime(Id<LocalizedText> id)
        {
            throw new NotImplementedException();
        }
    }

    public static class TestRawLocalization
    {
        [NUnit.Framework.Test]
        public static void TestDataIntegrity()
        {
            Dictionary<Id<LocalizedText>, string> data = new Dictionary<Id<LocalizedText>, string>()
            {
                { Id<LocalizedText>.New(), "" },
                { Id<LocalizedText>.New(), "test" },
                { Id<LocalizedText>.New(), "test2" },
                { Id<LocalizedText>.New(), "test" },
                { Id<LocalizedText>.New(), new string('*', 65535) },
                { Id<LocalizedText>.New(), "Another test string" },
                { Id<LocalizedText>.New(), "You get the idea" },
                { Id<LocalizedText>.New(), "More test data" },
                { Id<LocalizedText>.New(), "" },
                { Id<LocalizedText>.New(), "B" },
                { Id<LocalizedText>.New(), "C" },
            };

            using (var file = new MockLocalizationFile(data.Keys, k => data[k]))
            {
                using (MemoryStream m = new MemoryStream(100000))
                {
                    GenerateRawLocalization.Write(file, m);

                    m.Position = 0;
                    var read = RawLocalizationReader.Read(m);
                    foreach (var key in data.Keys)
                    {
                        Assert.That(read.Localize(key), Is.EqualTo(data[key]));
                    }
                }
            }
        }
    }
}
