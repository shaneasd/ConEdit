using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Conversation;

namespace ConversationEditor
{
    public interface IProjectElementList<out TReal, TInterface> : IEnumerable<TInterface>
        where TReal : TInterface
    {
        /// <summary>
        /// Load files unknown to the project, generating new FileIds for each
        /// </summary>
        IEnumerable<TInterface> Load(IEnumerable<DocumentPath> paths);
        /// <summary>
        /// Load files already known to the project, determining their path from a lookup against their FileId
        /// </summary>
        IEnumerable<TInterface> Load(IEnumerable<Id<FileInProject>> fileInfos);
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
