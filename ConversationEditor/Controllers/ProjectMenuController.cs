using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Utilities;
using Conversation;
using Conversation.Serialization;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGUI>;

    public class ProjectMenuController
    {
        private INodeFactory m_conversationNodeFactory;
        private INodeFactory m_domainNodeFactory;
        private ConfigParameterList<string> m_config;
        private OpenFileDialog m_ofd = new OpenFileDialog() { DefaultExt = "prj", Filter = "Conversation Projects|*.prj|All Files (*.*)|*.*" };
        private SaveFileDialog m_sfd = new SaveFileDialog() { DefaultExt = "prj", Filter = "Conversation Projects|*.prj|All Files (*.*)|*.*" };
        private IProject m_project;

        public IProject CurrentProject
        {
            get { return m_project; }
            set
            {
                if (m_project != null)
                    m_project.File.Moved -= ProjectMoved;
                m_project = value;
                m_project.File.Moved += ProjectMoved;

                if (m_project.File.Exists)
                {
                    using (m_config.SuppressCallback())
                    {
                        m_config.Value.Remove(m_project.File.File.FullName);
                        m_config.Value.Insert(0, m_project.File.File.FullName);
                    }
                }

                ProjectChanged.Execute(CurrentProject);
            }
        }

        private void ProjectMoved(FileInfo from, FileInfo to)
        {
            using (m_config.SuppressCallback())
            {
                m_config.Value.Remove(from.FullName);
                m_config.Value.Remove(to.FullName);
                m_config.Value.Insert(0, to.FullName);
            }
        }

        public event Action<IProject> ProjectChanged;
        private Action<Action> m_executeInGUIThread;
        private PluginsConfig m_pluginsConfig;

        public ProjectMenuController(ConfigParameterList<string> config, INodeFactory conversationNodeFactory, INodeFactory domainNodeFactory, ProjectExplorer list, Action<IProject> projectChanged, Action<Action> executeInGUIThread, PluginsConfig pluginsConfig)
        {
            m_executeInGUIThread = executeInGUIThread;
            m_conversationNodeFactory = conversationNodeFactory;
            m_domainNodeFactory = domainNodeFactory;
            m_config = config;
            m_pluginsConfig = pluginsConfig;
            CurrentProject = DummyProject.Instance;

            ProjectChanged += projectChanged;

            var file = m_config.Value.FirstOrDefault(a => true, a => a, "");
            if (File.Exists(file))
                OpenProject(file);
        }

        public bool CanClose(bool includeProject)
        {
            var elements = includeProject ? CurrentProject.Elements : CurrentProject.ElementsExceptThis;
            foreach (var s in elements)
            {
                try
                {
                    if (!s.File.CanClose())
                        return false;
                }
                catch (MyFileLoadException e)
                {
                    Console.Out.WriteLine(e.Message);
                    Console.Out.WriteLine(e.StackTrace);
                    Console.Out.WriteLine(e.InnerException.Message);
                    Console.Out.WriteLine(e.InnerException.StackTrace);
                    MessageBox.Show("Failed to access " + s.File.File.FullName + " for saving");
                    return false;
                }
            }
            return true;
        }

        public void Open()
        {
            if (CanClose(true))
            {
                if (m_ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    OpenProject(m_ofd.FileName);
                }
            }
        }

        public void OpenProject(string path)
        {
            MemoryStream m = null;

            try
            {
                using (var projectFile = Util.LoadFileStream(path, FileMode.Open, FileAccess.Read))
                {
                    m = new MemoryStream((int)projectFile.Length);
                    projectFile.CopyTo(m);
                    m.Position = 0;
                }
            }
            catch (MyFileLoadException e)
            {
                Console.Out.WriteLine(e.Message);
                Console.Out.WriteLine(e.StackTrace);
                Console.Out.WriteLine(e.InnerException.Message);
                Console.Out.WriteLine(e.InnerException.StackTrace);
                MessageBox.Show("Project: " + path + " could not be accessed");
            }

            if (m != null)
            {
                var deserializer = new XMLProject.Deserializer();
                var projectData = deserializer.Read(m);
                var project = new Project(projectData, m_conversationNodeFactory, m_domainNodeFactory, m, new FileInfo(path),
                    new XMLProject.Serializer(),
                    SerializationUtils.ConversationSerializer,
                    SerializationUtils.ConversationSerializerDeserializer,
                    SerializationUtils.DomainSerializer, m_pluginsConfig);

                project.FileDeletedExternally += () => m_executeInGUIThread(() => { MessageBox.Show("Project file deleted by another application"); });
                project.FileModifiedExternally += () => m_executeInGUIThread(() =>
                    {
                        var choice = MessageBox.Show("Project file modified by another application. Discard project changes and reload project from disk?", "Reload project?", MessageBoxButtons.YesNo);
                        if (choice == DialogResult.Yes)
                        {
                            if (CanClose(false))
                            {
                                CurrentProject = DummyProject.Instance;
                                //TODO: Update the GUI to reflect the change in project, in case the load fails.
                                OpenProject(project.File.File.FullName);
                            }
                        }
                    });
                project.ElementModifiedExternally += (element, reload) => m_executeInGUIThread(() =>
                {
                    var choice = MessageBox.Show("Project element " + element.File.File.FullName + " modified by another application. Discard changes to this element and reload project from disk?", "Reload element?", MessageBoxButtons.YesNo);
                    if (choice == DialogResult.Yes)
                    {
                        reload();
                    }
                });
                project.ElementDeletedExternally += (element) => m_executeInGUIThread(() => { MessageBox.Show("Project element " + element.File.File.FullName + " deleted by another application"); });
                CurrentProject = project;
            }
        }

        public void Save()
        {
            foreach (var s in CurrentProject.Elements)
            {
                try
                {
                    if (s.File.Changed)
                        s.File.Save();
                }
                catch (MyFileLoadException e)
                {
                    Console.Out.WriteLine(e.Message);
                    Console.Out.WriteLine(e.StackTrace);
                    Console.Out.WriteLine(e.InnerException.Message);
                    Console.Out.WriteLine(e.InnerException.StackTrace);
                    MessageBox.Show("Failed to access " + s.File.File.FullName + " for saving");
                }
            }
        }

        public void New()
        {
            if (CanClose(true)) //Attempt to clear all the existing conversations
            {
                if (DialogResult.OK == m_sfd.ShowDialog())
                {
                    Project project = null;
                    try
                    {
                        project = Project.CreateEmpty(new FileInfo(m_sfd.FileName), m_conversationNodeFactory, m_domainNodeFactory,
                            new XMLProject.Serializer(),
                            SerializationUtils.ConversationSerializer,
                            SerializationUtils.ConversationSerializerDeserializer,
                            SerializationUtils.DomainSerializer, m_pluginsConfig);
                    }
                    catch (MyFileLoadException e)
                    {
                        Console.Out.WriteLine(e.Message);
                        Console.Out.WriteLine(e.StackTrace);
                        Console.Out.WriteLine(e.InnerException);
                        Console.Out.WriteLine(e.InnerException.StackTrace);
                        MessageBox.Show("Failed to access file");
                        return;
                    }

                    CurrentProject = project;
                }
            }
        }

        internal bool SaveAs()
        {
            if (m_sfd.ShowDialog() == DialogResult.OK)
            {
                CurrentProject.File.SaveAs(new FileInfo(m_sfd.FileName));
                ProjectChanged.Execute(CurrentProject);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Exit()
        {
            if (CanClose(true))
            {
                CurrentProject = DummyProject.Instance;
                return true;
            }
            return false;
        }
    }
}
