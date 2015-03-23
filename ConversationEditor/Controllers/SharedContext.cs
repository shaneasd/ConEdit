using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.IO;

namespace ConversationEditor
{
    public interface ILocalizationContext
    {
        NotifierProperty<ILocalizationFile> CurrentLocalization { get; }
    }

    /// <summary>
    /// Context of the editor shared by several controls
    /// </summary>
    public class SharedContext : ILocalizationContext
    {
        public NotifierProperty<IProject> CurrentProject = new NotifierProperty<IProject>(DummyProject.Instance);
        public event Action<Changed<FileInfo>> ProjectMoved;
        public NotifierProperty<ILocalizationFile> CurrentLocalization { get { return m_currentLocalization; } }
        private readonly NotifierProperty<ILocalizationFile> m_currentLocalization = new NotifierProperty<ILocalizationFile>(DummyLocalizationFile.Instance);

        private void OnProjectMoved(Changed<FileInfo> change)
        {
            ProjectMoved.Execute(change);
        }

        public SharedContext()
        {
            CurrentProject.Changed.Register(change => { change.from.File.Moved -= OnProjectMoved; change.to.File.Moved += OnProjectMoved; });
        }
    }
}
