using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConversationEditor
{
    internal interface IProjectElementList<out TReal, TInterface> : IEnumerable<TInterface>
        where TReal : TInterface
    {
        IEnumerable<TInterface> Load(IEnumerable<FileInfo> fileInfos);
        void Reload();
        TReal New(DirectoryInfo path);
        /// <summary>
        /// Remove the 'element' from the collection. Provided either 'force' is true or element.File.CanClose is true
        /// </summary>
        void Remove(TInterface element, bool force);
        void Delete(TInterface element);
        bool FileLocationOk(string path);

        event Action<TInterface> Added;
        event Action<TInterface> Removed;
        event Action<TInterface, TInterface> Reloaded;
        event Action GotChanged;
    }

}
