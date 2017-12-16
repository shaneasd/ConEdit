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
    using ConversationNode = ConversationNode<INodeGui>;

    internal class ProjectMenuController
    {
        private INodeFactory m_conversationNodeFactory;
        private INodeFactory m_domainNodeFactory;
        private ConfigParameterList<string> m_config;

        private static OpenFileDialog MakeOpenFileDialog()
        {
            var result = new OpenFileDialog();
            try
            {
                result.DefaultExt = "prj";
                result.Filter = "Conversation Projects|*.prj|All Files (*.*)|*.*";
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        private static SaveFileDialog MakeSaveFileDialog()
        {
            var result = new SaveFileDialog();
            try
            {
                result.DefaultExt = "prj";
                result.Filter = "Conversation Projects|*.prj|All Files (*.*)|*.*";
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        private OpenFileDialog m_ofd = MakeOpenFileDialog();
        private SaveFileDialog m_sfd = MakeSaveFileDialog();

        /// <summary>
        /// Updates the stored (in m_config) list of recently opened projects to ensure that the current project is first in the list
        /// </summary>
        private void UpdateRecentlyOpenedConfig()
        {
            if (m_context.CurrentProject.Value.File.Exists)
            {
                if (!m_config.Value.Any() || m_config.Value[0] != m_context.CurrentProject.Value.File.File.FullName)
                {
                    using (m_config.SuppressCallback())
                    {
                        m_config.Value.Remove(m_context.CurrentProject.Value.File.File.FullName); //Remove the new project from wherever it was previously in the list (if it was there at all)
                        m_config.Value.Insert(0, m_context.CurrentProject.Value.File.File.FullName); //And add (or readd) it at the start of the list
                    }
                }
            }
        }

        /// <summary>
        /// Updates the stored (in m_config) list of recently opened projects to reflect the fact that a project has been renamed from 'from' to 'to'
        /// </summary>
        private void ProjectMoved(FileInfo from, FileInfo to)
        {
            using (m_config.SuppressCallback())
            {
                int index = m_config.Value.IndexOf(from.FullName);
                if (index != -1)
                    m_config.Value[index] = to.FullName;
            }
        }

        private Action<Action> m_executeInGuiThread;
        private PluginsConfig m_pluginsConfig;
        private Func<IAudioProviderCustomization> m_audioCustomization;
        SharedContext m_context;
        private UpToDateFile.BackEnd m_backend;

        public ProjectMenuController(SharedContext context, ConfigParameterList<string> config, INodeFactory conversationNodeFactory, INodeFactory domainNodeFactory, Action<Action> executeInGuiThread, PluginsConfig pluginsConfig, Func<IAudioProviderCustomization> audioCustomization, UpToDateFile.BackEnd backend)
        {
            m_context = context;
            m_executeInGuiThread = executeInGuiThread;
            m_conversationNodeFactory = conversationNodeFactory;
            m_domainNodeFactory = domainNodeFactory;
            m_config = config;
            m_pluginsConfig = pluginsConfig;
            m_audioCustomization = audioCustomization;
            m_backend = backend;
            m_context.ProjectMoved += WeakCallback<Changed<FileInfo>>.Handler(this, (me, a) => me.ProjectMoved(a.From, a.To));
            m_context.CurrentProject.Changed.Register(this, (a, b) => UpdateRecentlyOpenedConfig());

            var file = m_config.Value.FirstOrDefault(a => true, a => a, "");
            if (File.Exists(file))
                OpenProject(file);
        }

        public bool CanClose(bool includeProject)
        {
            var elements = includeProject ? m_context.CurrentProject.Value.Elements : m_context.CurrentProject.Value.ElementsExceptThis;
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
            FileStream projectFile = null;
            try
            {
                projectFile = Util.LoadFileStream(path, FileMode.Open, FileAccess.Read, 0);
            }
            catch (MyFileLoadException e)
            {
                Console.Out.WriteLine(e.Message);
                Console.Out.WriteLine(e.StackTrace);
                Console.Out.WriteLine(e.InnerException.Message);
                Console.Out.WriteLine(e.InnerException.StackTrace);
                MessageBox.Show("Project: " + path + " could not be accessed");
                return;
            }

            try
            {
                try
                {
                    m = new MemoryStream((int)projectFile.Length);
                    projectFile.CopyTo(m);
                    m.Position = 0;
                }
                finally
                {
                    projectFile.Dispose();
                }

                var deserializer = new XMLProject.Deserializer((new FileInfo(path)).Directory);
                Project.TData projectData;
                try
                {
                    projectData = deserializer.Read(m);
                }
                catch (DeserializerVersionMismatchException e)
                {
                    MessageBox.Show("Failed to open project. " + e.Message);
                    return;
                }
                var project = new Project(m_context, projectData, m_conversationNodeFactory, m_domainNodeFactory, m, new FileInfo(path),
                    new XMLProject.Serializer(),
                    SerializationUtils.ConversationSerializer,
                    SerializationUtils.ConversationSerializerDeserializer,
                    SerializationUtils.DomainSerializer, m_pluginsConfig, m_audioCustomization, m_backend);

                project.FileDeletedExternally += () => m_executeInGuiThread(() => { MessageBox.Show("Project file deleted by another application"); });
                project.FileModifiedExternally += () => m_executeInGuiThread(() =>
                    {
                        var choice = MessageBox.Show("Project file modified by another application. Discard project changes and reload project from disk?", "Reload project?", MessageBoxButtons.YesNo);
                        if (choice == DialogResult.Yes)
                        {
                            if (CanClose(false))
                            {
                                m_context.CurrentProject.Value = DummyProject.Instance;
                                OpenProject(project.File.File.FullName);
                            }
                        }
                    });
                project.ElementModifiedExternally += (element, reload) => m_executeInGuiThread(() =>
                {
                    var choice = MessageBox.Show("Project element " + element.File.File.FullName + " modified by another application. Discard changes to this element and reload from disk?", "Reload element?", MessageBoxButtons.YesNo);
                    if (choice == DialogResult.Yes)
                    {
                        reload();
                    }
                });
                project.ElementDeletedExternally += (element) => m_executeInGuiThread(() =>
                {
                    //We never actually remove this callback so if the file is deleted this callback will trigger
                    //informing the user that the file they just deleted was just deleted. As this is pointless
                    //we check if the project in question is in the project. We could simply detach the callback at
                    //the appropriate time but this way seems more resilient to potential bugs
                    if (m_context.CurrentProject.Value.Conversations.Contains(element))
                    {
                        MessageBox.Show("Project element " + element.File.File.FullName + " deleted by another application");
                    }
                });
                m_context.CurrentProject.Value = project;
            }
            finally
            {
                if (m != null)
                    m.Dispose();
            }
        }

        public void Save()
        {
            foreach (var s in m_context.CurrentProject.Value.Elements)
            {
                try
                {
                    var writable = s.File.Writable;
                    if (writable != null)
                        if (writable.Changed)
                            writable.Save();
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void New()
        {
            if (CanClose(true)) //Attempt to clear all the existing conversations
            {
                if (DialogResult.OK == m_sfd.ShowDialog())
                {
                    Project project = null;
                    try
                    {
                        try
                        {
                            project = Project.CreateEmpty(m_context, new FileInfo(m_sfd.FileName), m_conversationNodeFactory, m_domainNodeFactory,
                                                                                 new XMLProject.Serializer(),
                                                                                 SerializationUtils.ConversationSerializer,
                                                                                 SerializationUtils.ConversationSerializerDeserializer,
                                                                                 SerializationUtils.DomainSerializer, m_pluginsConfig, m_audioCustomization, m_backend);
                            m_context.CurrentProject.Value = project;
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
                    }
                    catch
                    {
                        project.Dispose();
                        throw;
                    }
                }
            }
        }

        internal bool SaveAs()
        {
            if (m_sfd.ShowDialog() == DialogResult.OK)
            {
                m_context.CurrentProject.Value.File.Writable.SaveAs(new FileInfo(m_sfd.FileName));
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
                m_context.CurrentProject.Value.Dispose();
                m_context.CurrentProject.Value = DummyProject.Instance;
                return true;
            }
            return false;
        }
    }
}
