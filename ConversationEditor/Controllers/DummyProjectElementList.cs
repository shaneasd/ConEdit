using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Conversation;

namespace ConversationEditor
{
    internal class DummyProjectElementList<TReal, TInterface> : IProjectElementList<TReal, TInterface> where TReal : TInterface
    {
        public static readonly DummyProjectElementList<TReal, TInterface> Instance = new DummyProjectElementList<TReal, TInterface>();

        public IEnumerable<TInterface> Load(IEnumerable<DocumentPath> fileInfos)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TInterface> Load(IEnumerable<Id<FileInProject>> fileInfos)
        {
            throw new NotImplementedException();
        }

        public void Reload()
        {
            throw new NotImplementedException();
        }

        public TReal New(DirectoryInfo path)
        {
            throw new NotImplementedException();
        }

        public void Remove(TInterface element, bool force)
        {
            throw new NotImplementedException();
        }

        public void Delete(TInterface element)
        {
            throw new NotImplementedException();
        }

        public bool FileLocationOk(string path)
        {
            throw new NotImplementedException();
        }

        public event Action<TInterface> Added { add { } remove { } }

        public event Action<TInterface> Removed { add { } remove { } }

        public event Action<TInterface, TInterface> Reloaded { add { } remove { } }

        public event Action GotChanged { add { } remove { } }

        public IEnumerator<TInterface> GetEnumerator()
        {
            return Enumerable.Empty<TInterface>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Enumerable.Empty<TInterface>().GetEnumerator();
        }
    }
}
