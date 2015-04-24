using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.IO;
using System.Drawing.Drawing2D;

namespace ConversationEditor
{
    internal abstract class ProjectElementDefinition
    {
        public static IEnumerable<ProjectElementDefinition> Definitions
        {
            get
            {
                yield return ConversationDefinition.Instance;
                yield return DomainDefinition.Instance;
                yield return LocalizationDefinition.Instance;
                yield return AudioDefinition.Instance;
            }
        }

        public static ProjectElementDefinition<T> Get<T>()
        {
            return Definitions.Select(d => d as ProjectElementDefinition<T>).Where(d => d != null).Single();
        }

        public abstract Bitmap Icon { get; }
        public abstract Bitmap MissingIcon { get; }

        public abstract void Update(bool visible, ref ProjectExplorer.VisibilityFilter m_visibility);
    }

    internal abstract class ProjectElementDefinition<T> : ProjectElementDefinition
    {
        public abstract ProjectExplorer.Item MakeMissingElement(Func<RectangleF> area, T item, IProject project, ProjectExplorer.ContainerItem parent, Func<Matrix> toControlTransform, Func<ProjectExplorer.FileSystemObject, string, bool> rename);
        public abstract ProjectExplorer.Item MakeElement(Func<RectangleF> area, T item, IProject project, ProjectExplorer.ContainerItem parent, Func<Matrix> toControlTransform, Func<ProjectExplorer.FileSystemObject, string, bool> rename);
    }

    internal class ConversationDefinition : ProjectElementDefinition<IConversationFile>
    {
        public static ProjectElementDefinition Instance = new ConversationDefinition();
        private static Bitmap m_icon;
        private static Bitmap m_missingIcon;

        static ConversationDefinition()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.Conversation.png"))
                m_icon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.ConversationMissing.png"))
                m_missingIcon = new Bitmap(stream);
        }

        public override Bitmap Icon
        {
            get { return m_icon; }
        }

        public override Bitmap MissingIcon
        {
            get { return m_missingIcon; }
        }

        public override ProjectExplorer.Item MakeMissingElement(Func<RectangleF> area, IConversationFile item, IProject project, ProjectExplorer.ContainerItem parent, Func<Matrix> toControlTransform, Func<ProjectExplorer.FileSystemObject, string, bool> rename)
        {
            var result = new ProjectExplorer.MissingLeafItem<IConversationFile>(area, item, project, MissingIcon, parent, f => f.Conversations, toControlTransform, rename);
            return result;
        }

        public override ProjectExplorer.Item MakeElement(Func<RectangleF> area, IConversationFile item, IProject project, ProjectExplorer.ContainerItem parent, Func<Matrix> toControlTransform, Func<ProjectExplorer.FileSystemObject, string, bool> rename)
        {
            var result = new ProjectExplorer.ConversationItem(area, item, project, parent, Icon, MissingIcon, toControlTransform, rename);
            return result;
        }

        public override void Update(bool visible, ref ProjectExplorer.VisibilityFilter m_visibility)
        {
            m_visibility.Conversations = visible;
        }
    }

    internal class LocalizationDefinition : ProjectElementDefinition<ILocalizationFile>
    {
        public static ProjectElementDefinition Instance = new LocalizationDefinition();
        private static Bitmap m_icon;
        private static Bitmap m_missingIcon;

        static LocalizationDefinition()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.LocalisationFile.png"))
                m_icon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.LocalisationFileMissing.png"))
                m_missingIcon = new Bitmap(stream);
        }

        public override Bitmap Icon
        {
            get { return m_icon; }
        }

        public override Bitmap MissingIcon
        {
            get { return m_missingIcon; }
        }

        public override ProjectExplorer.Item MakeMissingElement(Func<RectangleF> area, ILocalizationFile item, IProject project, ProjectExplorer.ContainerItem parent, Func<Matrix> toControlTransform, Func<ProjectExplorer.FileSystemObject, string, bool> rename)
        {
            var result = new ProjectExplorer.MissingLeafItem<ILocalizationFile>(area, item, project, MissingIcon, parent, f => f.Localizations, toControlTransform, rename);
            return result;
        }

        public override ProjectExplorer.Item MakeElement(Func<RectangleF> area, ILocalizationFile item, IProject project, ProjectExplorer.ContainerItem parent, Func<Matrix> toControlTransform, Func<ProjectExplorer.FileSystemObject, string, bool> rename)
        {
            var result = new ProjectExplorer.RealLeafItem<ILocalizationFile, ILocalizationFile>(area, item, Icon, project, parent, f => f.Localizations, toControlTransform, rename);
            return result;
        }

        public override void Update(bool visible, ref ProjectExplorer.VisibilityFilter m_visibility)
        {
            m_visibility.Localizations = visible;
        }
    }

    internal class DomainDefinition : ProjectElementDefinition<IDomainFile>
    {
        public static ProjectElementDefinition Instance = new DomainDefinition();
        private static Bitmap m_icon;
        private static Bitmap m_missingIcon;

        static DomainDefinition()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.Domain.png"))
                m_icon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.DomainMissing.png"))
                m_missingIcon = new Bitmap(stream);
        }

        public override Bitmap Icon
        {
            get { return m_icon; }
        }

        public override Bitmap MissingIcon
        {
            get { return m_missingIcon; }
        }

        public override ProjectExplorer.Item MakeMissingElement(Func<RectangleF> area, IDomainFile item, IProject project, ProjectExplorer.ContainerItem parent, Func<Matrix> toControlTransform, Func<ProjectExplorer.FileSystemObject, string, bool> rename)
        {
            var result = new ProjectExplorer.MissingLeafItem<IDomainFile>(area, item, project, MissingIcon, parent, f => f.Domains, toControlTransform, rename);
            return result;
        }

        public override ProjectExplorer.Item MakeElement(Func<RectangleF> area, IDomainFile item, IProject project, ProjectExplorer.ContainerItem parent, Func<Matrix> toControlTransform, Func<ProjectExplorer.FileSystemObject, string, bool> rename)
        {
            var result = new ProjectExplorer.DomainItem(area, item, project, parent, Icon, MissingIcon, toControlTransform, rename);
            return result;
        }

        public override void Update(bool visible, ref ProjectExplorer.VisibilityFilter m_visibility)
        {
            m_visibility.Domains = visible;
        }
    }

    internal class AudioDefinition : ProjectElementDefinition<IAudioFile>
    {
        public static ProjectElementDefinition Instance = new AudioDefinition();
        private static Bitmap m_icon;
        private static Bitmap m_missingIcon;

        static AudioDefinition()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.Audio.png"))
                m_icon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.AudioMissing.png"))
                m_missingIcon = new Bitmap(stream);
        }

        public override Bitmap Icon
        {
            get { return m_icon; }
        }

        public override Bitmap MissingIcon
        {
            get { return m_missingIcon; }
        }

        public override ProjectExplorer.Item MakeMissingElement(Func<RectangleF> area, IAudioFile item, IProject project, ProjectExplorer.ContainerItem parent, Func<Matrix> toControlTransform, Func<ProjectExplorer.FileSystemObject, string, bool> rename)
        {
            var result = new ProjectExplorer.MissingLeafItem<IAudioFile>(area, item, project, MissingIcon, parent, f => f.Audio, toControlTransform, rename);
            return result;
        }

        public override ProjectExplorer.Item MakeElement(Func<RectangleF> area, IAudioFile item, IProject project, ProjectExplorer.ContainerItem parent, Func<Matrix> toControlTransform, Func<ProjectExplorer.FileSystemObject, string, bool> rename)
        {
            var result = new ProjectExplorer.RealLeafItem<IAudioFile, IAudioFile>(area, item, Icon, project, parent, f => f.Audio, toControlTransform, rename);
            return result;
        }

        public override void Update(bool visible, ref ProjectExplorer.VisibilityFilter m_visibility)
        {
            m_visibility.Audio = visible;
        }
    }
}