using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Conversation;
using ConversationEditor.Controllers;
using ConversationEditor;
using Utilities;
using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI>;
using Conversation.Serialization;
using System.Xml.Linq;

namespace ConversationEditor
{
    public partial class Form1 : Form
    {
        ErrorCheckerController m_errorCheckerController;
        Config m_config;
        INodeFactory m_conversationNodeFactory;
        INodeFactory m_domainNodeFactory;
        INodeFactory m_projectNodeFactory;
        ProjectMenuController m_projectMenuController;

        ConversationEditorControl m_domainEditor2 = new ConversationEditorControl() { Dock = DockStyle.Fill, ShowGrid = true };
        ConversationEditorControl m_projectGraphEditor = new ConversationEditorControl() { Dock = DockStyle.Fill, ShowGrid = true };

        IGraphEditorControl<ConversationNode> m_currentEditor = null;
        IGraphEditorControl<ConversationNode> CurrentEditor
        {
            get
            {
                return m_currentEditor;
            }
            set
            {
                if (m_currentEditor != value || value == null)
                {
                    m_currentEditor = value;
                    splitContainer2.Panel1.SuspendLayout();
                    splitContainer2.Panel1.Controls.Clear();
                    if (value != null)
                        splitContainer2.Panel1.Controls.Add(value.AsControl());
                    splitContainer2.Panel1.ResumeLayout();

                    if (value == conversationEditorControl1)
                    {
                        findAndReplaceToolStripMenuItem.Visible = true;
                        findAndReplaceToolStripMenuItem.Text = "Find in Conversations";
                    }
                    else if (value == m_domainEditor2)
                    {
                        findAndReplaceToolStripMenuItem.Visible = true;
                        findAndReplaceToolStripMenuItem.Text = "Find in Domain";
                    }
                    else if (value == m_projectGraphEditor)
                    {
                        findAndReplaceToolStripMenuItem.Visible = false;
                    }
                    else
                    {
                        findAndReplaceToolStripMenuItem.Visible = false;
                    }

                    projectExplorer.Select(CurrentFile);
                }
            }
        }

        public IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> CurrentFile
        {
            get
            {
                if (CurrentEditor == m_domainEditor2)
                    return m_domainEditor2.CurrentFile;
                else if (CurrentEditor == conversationEditorControl1)
                    return conversationEditorControl1.CurrentFile;
                else if (CurrentEditor == m_projectGraphEditor)
                    return m_projectGraphEditor.CurrentFile;
                else
                    return DummyConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>.Instance;
            }
        }

        public Form1()
        {
            InitializeComponent();

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.GenericSystemHover.ico"))
                this.Icon = new Icon(stream);

            KeyPreview = true;

            try
            {
                var willEdit = new WillEdit
                {
                    IsDecimal = guid => m_projectMenuController.CurrentProject.ConversationDataSource.IsDecimal(guid) || m_projectMenuController.CurrentProject.DomainDataSource.IsDecimal(guid),
                    IsDynamicEnum = guid => m_projectMenuController.CurrentProject.ConversationDataSource.IsDynamicEnum(guid) || m_projectMenuController.CurrentProject.DomainDataSource.IsDynamicEnum(guid),
                    IsEnum = guid => m_projectMenuController.CurrentProject.ConversationDataSource.IsEnum(guid) || m_projectMenuController.CurrentProject.DomainDataSource.IsEnum(guid),
                    IsInteger = guid => m_projectMenuController.CurrentProject.ConversationDataSource.IsInteger(guid) || m_projectMenuController.CurrentProject.DomainDataSource.IsInteger(guid),
                };
                m_config = new Config("config.xml", willEdit);
            }
            catch (Config.LoadFailedException)
            {
                throw;
            }

            InitialiseNodeFactory();
            InitialiseFileMenu();
            InitialiseEditMenu();
            InitialiseProjectMenu();
            InitialiseConversationEditor();
            InitializeDomainEditor();
            InitialiseOptionsMenu();
            InitialiseExportMenu();
            m_errorCheckerController = new ErrorCheckerController(m_config.ErrorCheckers, m_config.Plugins);

            errorList1.HightlightNode += errorList1_HightlightNode;

            menuStrip1.Renderer = ColorScheme.ContextMenu;
            splitContainer1.BackColor = ColorScheme.FormBackground;
            splitContainer1.ForeColor = ColorScheme.Foreground;
            splitContainer2.BackColor = ColorScheme.FormBackground;
            splitContainer2.ForeColor = ColorScheme.Foreground;
            BackColor = ColorScheme.FormBackground;
            ForeColor = ColorScheme.Foreground;
            errorList1.ForeColor = ColorScheme.Foreground;
            errorList1.BackColor = ColorScheme.Background;

            projectExplorer.m_contextMenuItemsFactory = new WrapperContextMenuItemsFactory(() => m_config.Plugins.UnfilteredAssemblies);
        }

        private void InitialiseNodeFactory()
        {

            m_conversationNodeFactory = new NodeFactory(m_config.ConversationNodeRenderers, GetAllNodeRenderers(), a => m_config.ConversationNodeRenderers.ValueChanged += a, guid => m_projectMenuController.CurrentProject.Localizer.Localize(guid));
            m_domainNodeFactory = new NodeFactory(m_config.DomainNodeRenderers, guid => m_projectMenuController.CurrentProject.Localizer.Localize(guid));
            m_projectNodeFactory = new NodeFactory(m_config.ProjectNodeRenderers, guid => m_projectMenuController.CurrentProject.Localizer.Localize(guid));
        }

        private void InitialiseOptionsMenu()
        {
            tsmiShowGrid.Checked = m_config.GraphView.ShowGrid;
            tsmiSnapToGrid.Checked = m_config.GraphView.SnapToGrid;
            tsmiShowIDs.Checked = m_config.GraphView.ShowIDs;
        }

        private bool SelectNode(ConversationNode node, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> file)
        {
            if (!file.Nodes.Contains(node))
                return false;
            if (m_projectMenuController.CurrentProject.Conversations.Contains(file))
            {
                projectExplorer.Select(file);
                conversationEditorControl1.CurrentFile = file;
                conversationEditorControl1.SelectNode(node);
                CurrentEditor = conversationEditorControl1;
            }
            else if (m_projectMenuController.CurrentProject.DomainFiles.Contains(file))
            {
                projectExplorer.Select(file);
                m_domainEditor2.CurrentFile = file;
                m_domainEditor2.SelectNode(node);
                CurrentEditor = m_domainEditor2;
            }
            else if (object.Equals(file.File, m_projectMenuController.CurrentProject.File))
            {
                m_projectGraphEditor.CurrentFile = file; //TODO:Shouldn't be necessary
                m_projectGraphEditor.SelectNode(node);
                CurrentEditor = m_projectGraphEditor;
            }
            else
                return false;
            return true;
        }

        //TODO: Refactor all these to have a generic type parameter
        private IEnumerable<IMenuActionFactory<ConversationNode>> PluginContextMenuFactories()
        {
            List<IMenuActionFactory<ConversationNode>> result = new List<IMenuActionFactory<ConversationNode>>();
            foreach (var pa in m_config.Plugins.UnfilteredAssemblies)
            {
                var factories = pa.Assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IMenuActionFactory<ConversationNode>)));
                foreach (var factory in factories)
                {
                    IMenuActionFactory<ConversationNode> obj = factory.GetConstructor(Type.EmptyTypes).Invoke(new object[0]) as IMenuActionFactory<ConversationNode>;
                    result.Add(obj);
                }
            }
            return result;
        }

        private IEnumerable<IAudioProviderCustomization> PluginAudioProviderCustomizations()
        {
            List<IAudioProviderCustomization> result = new List<IAudioProviderCustomization>();
            foreach (var pa in m_config.Plugins.UnfilteredAssemblies)
            {
                var factories = pa.Assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IAudioProviderCustomization)));
                foreach (var factory in factories)
                {
                    IAudioProviderCustomization obj = factory.GetConstructor(Type.EmptyTypes).Invoke(new object[0]) as IAudioProviderCustomization;
                    result.Add(obj);
                }
            }
            return result;
        }

        private List<IParameterEditorFactory> GetAllParameterEditors()
        {
            List<IParameterEditorFactory> result = new List<IParameterEditorFactory>();
            foreach (var pa in m_config.Plugins.UnfilteredAssemblies.Select(a => a.Assembly).Concat(Assembly.GetExecutingAssembly().Only()))
            {
                var factories = pa.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IParameterEditorFactory)));
                foreach (var factory in factories)
                {
                    IParameterEditorFactory obj = factory.GetConstructor(Type.EmptyTypes).Invoke(new object[0]) as IParameterEditorFactory;
                    result.Add(obj);
                }
            }
            return result;
        }

        private List<NodeUI.IFactory> GetAllNodeRenderers()
        {
            List<NodeUI.IFactory> result = new List<NodeUI.IFactory>();
            foreach (var pa in m_config.Plugins.UnfilteredAssemblies.Select(a => a.Assembly).Concat(Assembly.GetExecutingAssembly().Only()))
            {
                var factories = pa.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(NodeUI.IFactory)));
                foreach (var factory in factories)
                {
                    NodeUI.IFactory obj = factory.GetConstructor(Type.EmptyTypes).Invoke(new object[0]) as NodeUI.IFactory;
                    result.Add(obj);
                }
            }
            return result;
        }

        private void InitialiseConversationEditor()
        {
            InitializeGraphEditor(ConversationCopyPasteController.Instance, m_conversationNodeFactory, m_projectMenuController.CurrentProject.ConversationDataSource, conversationEditorControl1, new GraphContextMenuItems(FindReferences));
        }

        private void InitializeDomainEditor()
        {
            InitializeGraphEditor(ConversationCopyPasteController.Instance, m_domainNodeFactory, m_projectMenuController.CurrentProject.DomainDataSource, m_domainEditor2, new DomainContextMenuItems(FindReferences));
        }

        private void InitializeProjectEditor()
        {
            InitializeGraphEditor(ConversationCopyPasteController.Instance, m_projectNodeFactory, ProjectDomain.Instance, m_projectGraphEditor, new ProjectContextMenuItems(m_projectMenuController.CurrentProject));
        }

        private void InitializeGraphEditor(ConversationCopyPasteController copyPasteController, INodeFactory nodefactory, IDataSource datasource, GraphEditorControl<ConversationNode> editor, IMenuActionFactory<ConversationNode> basicItems)
        {
            editor.Initialise(Edit, nodefactory, copyPasteController, FindReferences);
            editor.SetContext(datasource, m_projectMenuController.CurrentProject.Localizer, m_projectMenuController.CurrentProject);
            editor.m_contextMenu.Opening += () => editor.RefreshContextMenu(basicItems.Only().Concat(PluginContextMenuFactories()));

            Action updateGraphViewFromConfig = () =>
            {
                editor.SnapToGrid = m_config.GraphView.SnapToGrid;
                editor.ShowGrid = m_config.GraphView.ShowGrid;
                editor.ShowIDs = m_config.GraphView.ShowIDs;
                editor.Colors = m_config.ColorScheme.Colors;
                editor.MinorGridSpacing = m_config.GraphView.MinorGridSpacing;
                editor.MajorGridSpacing = m_config.GraphView.MajorGridSpacing;
            };
            updateGraphViewFromConfig();
            m_config.GraphView.ValueChanged += updateGraphViewFromConfig;
            m_config.ColorScheme.ValueChanged += updateGraphViewFromConfig;
        }

        private void InitialiseEditMenu()
        {
            copyToolStripMenuItem.Click += (a, b) => CurrentEditor.CopySelection();
            duplicateToolStripMenuItem.Click += (a, b) => CurrentEditor.DuplicateSelection();
            selectAllToolStripMenuItem.Click += (a, b) => CurrentEditor.SelectAll();
            pasteToolStripMenuItem.Click += (a, b) => CurrentEditor.Paste(null);
            ungroupToolStripMenuItem.Click += (a, b) => CurrentEditor.UngroupSelection();
            groupToolStripMenuItem.Click += (a, b) => CurrentEditor.GroupSelection();

            undoToolStripMenuItem.Click += (a, b) => CurrentFile.File.UndoQueue.Undo();
            redoToolStripMenuItem.Click += (a, b) => CurrentFile.File.UndoQueue.Redo();

            findAndReplaceToolStripMenuItem.Visible = false;
        }

        private void InitialiseFileMenu()
        {
            projectExplorer.ItemSelected += () =>
            {
                if (m_projectMenuController != null) //As we add items initially one of them will be selected. At the time, m_projectMenuController is not yet initialized
                {
                    if (projectExplorer.SelectedConversation != null)
                    {
                        if (m_projectMenuController.CurrentProject.ReloadConversationDatasourceIfRequired())
                        {
                            conversationEditorControl1.SetContext(m_projectMenuController.CurrentProject.ConversationDataSource, m_projectMenuController.CurrentProject.Localizer, m_projectMenuController.CurrentProject);
                            conversationEditorControl1.UpdateKeyMappings();
                        }
                    }
                }

                conversationEditorControl1.CurrentFile = projectExplorer.SelectedConversation;
                m_domainEditor2.CurrentFile = projectExplorer.CurrentDomainFile;

                //m_projectGraphEditor.CurrentFile = m_projectMenuController.CurrentProject; //TODO:

                if (projectExplorer.SelectedConversation != null && projectExplorer.SelectedConversation.File.Exists)
                    CurrentEditor = conversationEditorControl1;
                else if (projectExplorer.CurrentDomainFile != null && projectExplorer.CurrentDomainFile.File.Exists)
                    CurrentEditor = m_domainEditor2;
                else if (projectExplorer.ProjectSelected)
                    CurrentEditor = m_projectGraphEditor;

                //TODO: What happens if you select an item for which there is no editor?
                //else
                //CurrentEditor = null;

                UpdateUndoMenu();
            };
            projectExplorer.LocalizerSelected += () =>
            {
                if (m_projectMenuController != null)
                    m_projectMenuController.CurrentProject.Localizer.SelectLocalizer(projectExplorer.CurrentLocalizer);
                conversationEditorControl1.Invalidate();
                m_domainEditor2.Invalidate();
            };
        }

        private void UpdateUndoMenu()
        {
            var file = (CurrentFile ?? DummyConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>.Instance).File;

            undoToolStripMenuItem.DropDownItems.Clear();
            undoToolStripMenuItem.Enabled = file.UndoQueue.HasUndo;
            redoToolStripMenuItem.Enabled = file.UndoQueue.HasRedo;

            file.UndoQueue.Changed -= UpdateUndoMenu;
            file.UndoQueue.Changed += UpdateUndoMenu;

            bool first = true;
            foreach (var element in file.UndoQueue.Elements.Take(10))
            {
                var e = element;

                ToolStripMenuItem item = new ToolStripMenuItem(e.Description);
                item.Click += (b, c) => { e.UndoUpToAndIncluding(); UpdateUndoMenu(); };
                undoToolStripMenuItem.DropDownItems.Add(item);

                if (first)
                {
                    item.ShortcutKeys = Keys.Control | Keys.Z;
                    first = false;
                }
            }
        }

        public void InitialiseProjectMenu()
        {
            //fromXmlToolStripMenuItem.Click += (a, b) =>
            //{
            //    if (m_projectMenuController.Exit())
            //        m_dataSourceController.LoadFromXml();
            //};

            Action<IProject> projectChanged = null;
            projectChanged = (project) =>
            {
                Project.TConfig projectConfig = ReadProjectConfig(project);

                conversationEditorControl1.SetContext(project.ConversationDataSource, project.Localizer, project);
                conversationEditorControl1.UpdateKeyMappings();

                m_domainEditor2.SetContext(project.DomainDataSource, project.Localizer, project);
                m_domainEditor2.UpdateKeyMappings();

                //TODO: Shouldn't have to do any of this
                //m_projectGraphEditor.SetContext(ProjectDomain.Instance, project.Localizer);
                //m_projectGraphEditor.UpdateKeyMappings();

                projectExplorer.SetProject(project);
                var p = project as Project;
                if (p != null)
                {
                    var con = projectConfig.LastEdited != null ? project.Conversations.FirstOrDefault(c => c.File.File.FullName == p.Rerout(projectConfig.LastEdited.Only()).Single().FullName) : null;
                    var dom = projectConfig.LastEdited != null ? project.DomainFiles.FirstOrDefault(c => c.File.File.FullName == p.Rerout(projectConfig.LastEdited.Only()).Single().FullName) : null;
                    conversationEditorControl1.CurrentFile = con;
                    m_domainEditor2.CurrentFile = dom;
                    if (con != null)
                    {
                        projectExplorer.Select(con);
                        CurrentEditor = conversationEditorControl1;
                    }
                    else if (dom != null)
                    {
                        projectExplorer.Select(dom);
                        CurrentEditor = m_domainEditor2;
                    }
                    else
                        CurrentEditor = null;
                    Func<ILocalizationFile, bool> matchesLastLocalization = c => c.File.File.FullName == (p.Rerout(projectConfig.LastLocalization.Only()).Single()).FullName;
                    IEnumerable<ILocalizationFile> allLocalizers = project.LocalizationFiles;
                    if (projectConfig.LastLocalization != null)
                        allLocalizers = allLocalizers.OrderByDescending(matchesLastLocalization);

                    var loc = allLocalizers.First();
                    projectExplorer.SelectLocalization(loc);
                    p.Localizer.SelectLocalizer(loc);
                }

                Text = project.File.Exists ? project.File.File.Name : "No Project Open";

                project.File.Moved += (from, to) => Text = project.File.File.Name;
            };

            m_projectMenuController = new ProjectMenuController(m_config.ProjectHistory, m_conversationNodeFactory, m_domainNodeFactory, projectExplorer, projectChanged, a => Invoke(a), m_config.Plugins, GetAudioCustomizer);

            this.projectSaveMenuItem.Click += (a, b) => m_projectMenuController.Save();
            this.projectNewMenuItem.Click += (a, b) => m_projectMenuController.New();
            this.projectOpenMenuItem.Click += (a, b) => m_projectMenuController.Open();
            this.projectSaveAsMenuItem.Click += (a, b) => m_projectMenuController.SaveAs();

            Action updateRecentProjects = () =>
                {
                    recentProjectsToolStripMenuItem.DropDownItems.Clear();
                    if (m_config.ProjectHistory.Value.Any())
                    {
                        recentProjectsToolStripMenuItem.Enabled = true;
                        foreach (string path in m_config.ProjectHistory.Value)
                        {
                            string PATH = path;
                            var item = recentProjectsToolStripMenuItem.DropDownItems.Add(PATH);
                            item.Click += (a, b) =>
                            {
                                bool open = false;
                                try
                                {
                                    open = m_projectMenuController.CanClose(true);
                                }
                                catch (MyFileLoadException e)
                                {
                                    Console.Out.WriteLine(e.Message);
                                    Console.Out.WriteLine(e.StackTrace);
                                    Console.Out.WriteLine(e.InnerException.Message);
                                    Console.Out.WriteLine(e.InnerException.StackTrace);
                                    MessageBox.Show("Failed to access " + m_projectMenuController.CurrentProject.File.File.FullName + " for saving");
                                }
                                if (open)
                                {
                                    m_projectMenuController.OpenProject(PATH);
                                }
                            };
                        }
                    }
                    else
                    {
                        recentProjectsToolStripMenuItem.Enabled = false;
                    }
                };
            updateRecentProjects();
            m_config.ProjectHistory.ValueChanged += () => updateRecentProjects();

            //CurrentFile = m_projectMenuController.CurrentProject.Files.FirstOrDefault();
        }

        private IParameterEditor<Control> GetParameterEditor(Guid id, ParameterEditorSetupData data)
        {
            var editorFactory = GetAllParameterEditors().Where(e => e.Guid == id).Single();
            IParameterEditor<Control> editor = editorFactory.Make();
            editor.Setup(data);
            return editor;
        }

        private IParameterEditor<Control> GetParameterEditor(ID<ParameterType> id, ParameterEditorSetupData data)
        {
            return GetParameterEditor(m_config.ParameterEditors[id], data);
        }

        private ConfigureResult Edit(IEditable data, AudioGenerationParameters audioContext)
        {
            if (data is UnknownEditable)
            {
                MessageBox.Show("Cannot edit this node as it is not fully defined");
                return ConfigureResult.NotApplicable;
            }
            else
                return m_config.NodeEditors[data.NodeTypeID].GetEditorFactory().Edit(data, audioContext, GetParameterEditor, m_projectMenuController.CurrentProject.Localizer, m_projectMenuController.CurrentProject.AudioProvider);
        }

        private void errorCheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO: Error checking for domain files

            var errors = m_errorCheckerController.CheckForErrors(CurrentFile.Nodes, new ErrorCheckerUtils(m_projectMenuController.CurrentProject.ConversationDataSource));
            errorList1.SetErrors(errors.Select(error => new ErrorList.Element(error, CurrentFile)));
        }

        private void testEverythingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conversations = m_projectMenuController.CurrentProject.Conversations;
            var errors = conversations.SelectMany(
                            c => m_errorCheckerController.CheckForErrors(c.Nodes, new ErrorCheckerUtils(m_projectMenuController.CurrentProject.ConversationDataSource)).Select(
                                error => new ErrorList.Element(error, c)));
            errorList1.SetErrors(errors);
        }

        private void errorList1_HightlightNode(ConversationNode node, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> file, BoolRef success)
        {
            success &= SelectNode(node, file);
        }

        Project.TConfig ReadProjectConfig(IProject project)
        {
            if (project is DummyProject)
                return new Project.TConfig(null, null);

            var configPath = project.File.File.FullName + ".config";
            if (File.Exists(configPath))
            {
                XElement lastDocument = null;
                XElement lastLocalization = null;
                try
                {
                    XDocument doc = XDocument.Load(configPath);
                    var config = doc.Element("Config");
                    lastDocument = config.Element("LastDocument");
                    lastLocalization = config.Element("LastLocalization");
                }
                catch
                {
                    //Ignore any problems with the file
                    return new Project.TConfig(null, null);
                }
                return new Project.TConfig(lastDocument != null ? lastDocument.Value : null,
                                           lastLocalization != null ? lastLocalization.Value : null);
            }
            return new Project.TConfig(null, null);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var projectFile = m_projectMenuController.CurrentProject.File;
            string configPath = null;
            string lastDocument = null;
            string lastLocalization = null;
            if (projectFile.Exists)
            {
                configPath = m_projectMenuController.CurrentProject.File.File.FullName + ".config";
                lastDocument = projectExplorer.SelectedConversation != null ? FileSystem.RelativePath(projectExplorer.SelectedConversation.File.File, m_projectMenuController.CurrentProject.Origin) :
                               projectExplorer.CurrentDomainFile != null ? FileSystem.RelativePath(projectExplorer.CurrentDomainFile.File.File, m_projectMenuController.CurrentProject.Origin) :
                               null;
                lastLocalization = projectExplorer.CurrentLocalizer != null ? FileSystem.RelativePath(projectExplorer.CurrentLocalizer.File.File, m_projectMenuController.CurrentProject.Origin) : null;
            }
            if (!m_projectMenuController.Exit())
                e.Cancel = true;
            else
            {
                if (configPath != null)
                {
                    var config = new XElement("Config");
                    if (lastDocument != null)
                        config.Add(new XElement("LastDocument", lastDocument));
                    if (lastLocalization != null)
                        config.Add(new XElement("LastLocalization", lastLocalization));
                    XDocument doc = new XDocument(config);

                    try
                    {
                        using (var s = Util.LoadFileStream(configPath, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            s.SetLength(0);
                            doc.Save(s);
                        }
                    }
                    catch (MyFileLoadException)
                    {
                        MessageBox.Show("Failed to save project config");
                    }
                }
            }
        }

        private void throwTestExceptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    throw new InvalidAsynchronousStateException("This is the inner inner exception message");
                }
                catch (Exception a)
                {
                    throw new InvalidCastException("This is the inner exception message", a);
                }
            }
            catch (Exception b)
            {
                throw new InvalidDataException("This is the outer exception message", b);
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_errorCheckerController.Configure();
        }

        private void tsmiShowGrid_CheckedChanged(object sender, EventArgs e)
        {
            m_config.GraphView.ShowGrid = tsmiShowGrid.Checked;
        }

        private void tsmiSnapToGrid_CheckedChanged(object sender, EventArgs e)
        {
            m_config.GraphView.SnapToGrid = tsmiSnapToGrid.Checked;
        }

        private void tsmiShowIDs_CheckedChanged(object sender, EventArgs e)
        {
            m_config.GraphView.ShowIDs = tsmiShowIDs.Checked;
        }

        private void customiseParameterEditorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var data = new ParameterEditorCustomization(m_projectMenuController.CurrentProject.ConversationDataSource, m_config.ParameterEditors, GetAllParameterEditors());
            //var config = new MapConfig<ID<ParameterType>, Guid>("ParameterEditors", kvp => new KeyValuePair<string,string>(kvp.Key.Serialized(), kvp.Value.ToString())
            //                                                    , kvp => new KeyValuePair<ID<ParameterType>,Guid>(ID<ParameterType>.Parse(kvp.Key), Guid.Parse(kvp.Value))
            //                                                    , g => DefaultEnumEditor.Factory.GUID);
            Func<ID<ParameterType>, ParameterEditorSetupData, IParameterEditor<Control>> config = (id, d) =>
            {
                var result = new DefaultEnumEditor();
                result.Setup(d);
                return result;
            };

            var editor = new DefaultNodeEditorFactory();
            editor.Edit(data, null, config, null, null).Do(a => a.Redo(), a => { });

            foreach (var d in data.Parameters)
            {
                m_config.ParameterEditors[d.TypeId] = (d as IEnumParameter).Value;
            }

            //using (ParameterEditorCustomizer2 ec = new ParameterEditorCustomizer2())
            //{
            //    ec.Init(m_config.ParameterEditors, m_projectMenuController.CurrentProject.ConversationDataSource);
            //    ec.ShowDialog();
            //}
        }

        private void customiseNodeRendererToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var data = new NodeRendererCustomization(m_projectMenuController.CurrentProject.ConversationDataSource, m_config.ConversationNodeRenderers, GetAllNodeRenderers());
            Func<ID<ParameterType>, ParameterEditorSetupData, IParameterEditor<Control>> config = (id, d) =>
            {
                var result = new DefaultEnumEditor();
                result.Setup(d);
                return result;
            };
            var editor = new DefaultNodeEditorFactory();
            editor.Edit(data, null, config, null, null).Do(a => a.Redo(), a => { });
            foreach (var d in data.Parameters)
            {
                m_config.ConversationNodeRenderers[ID<NodeTypeTemp>.ConvertFrom(d.TypeId)] = (d as IEnumParameter).Value;
            }

            //TODO: Customize domain node renderers?
            //using (NodeRendererCustomizer form = new NodeRendererCustomizer(m_projectMenuController.CurrentProject.ConversationDataSource, m_config.ConversationNodeRenderers, m_config.Plugins))
            //{
            //    form.ShowDialog();
            //}
        }

        private void pluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new PluginSelector())
            {
                form.Initalize(m_config.Plugins);
                form.ShowDialog();
            }
        }

        private void customiseNodeEditorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (NodeEditorCustomizer form = new NodeEditorCustomizer(m_projectMenuController.CurrentProject.ConversationDataSource, m_config.NodeEditors, m_config.Plugins))
            {
                form.ShowDialog();
            }
        }

        private void findAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IEnumerable<IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>> search;
            if (CurrentEditor == conversationEditorControl1)
            {
                search = m_projectMenuController.CurrentProject.Conversations;
            }
            else if (CurrentEditor == m_domainEditor2)
            {
                search = m_projectMenuController.CurrentProject.DomainFiles;
            }
            else
            {
                throw new Exception("fvjblanbl");
            }

            var d = new FindAndReplaceDialog(search, m_projectMenuController.CurrentProject.Localizer, () => CurrentFile);

            d.FocusNode += (node, file) =>
            {
                if (m_projectMenuController.CurrentProject.Conversations.Contains(file))
                {
                    projectExplorer.Select(file);
                    conversationEditorControl1.CurrentFile = file;
                    CurrentEditor = conversationEditorControl1;
                    conversationEditorControl1.SelectNode(node);
                }
                else if (m_projectMenuController.CurrentProject.DomainFiles.Contains(file))
                {
                    projectExplorer.Select(file);
                    m_domainEditor2.CurrentFile = file;
                    CurrentEditor = m_domainEditor2;
                    m_domainEditor2.SelectNode(node);
                }
                else
                    throw new Exception("skjvblb;wqef");

            };
            d.UpdateDisplay += () => { conversationEditorControl1.Invalidate(true); m_domainEditor2.Invalidate(true); };
            d.Show();
        }

        private void testProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Func<string, FileInfo> rooted = s =>
                {
                    if (Path.IsPathRooted(s))
                        return new FileInfo(s);
                    else
                        return new FileInfo(Path.Combine(m_projectMenuController.CurrentProject.File.File.Directory.FullName, s));
                };

            IEnumerable<IConversationFile> conversations = m_projectMenuController.CurrentProject.Conversations;
            var allAudioReferences = (conversations.SelectMany(c => c.Nodes.SelectMany(n => n.Parameters.OfType<IAudioParameter>().Select(p => new { Node = n, Path = rooted(p.Value.Value).FullName, Document = c })))).ToHashSet();

            var audioFiles = m_projectMenuController.CurrentProject.AudioFiles.Select(a => a.File.File.FullName).Evaluate();

            var unnecessaryAudioFiles = audioFiles.Except(allAudioReferences, a => a, b => b.Path);
            var danglingPointers = allAudioReferences.Except(audioFiles, b => b.Path, a => a);

            IEnumerable<ErrorList.Element> danglingPointerErrors = danglingPointers.Select(a => new ErrorList.Element(new DanglingAudioError(a.Path, a.Node), a.Document));
            IEnumerable<ErrorList.Element> pointlessAudioErrors = unnecessaryAudioFiles.Select(a => new ErrorList.Element(new PointlessAudioError(a), null));
            errorList1.SetErrors(danglingPointerErrors.Concat(pointlessAudioErrors));
        }

        private void FindReferences(ConversationNode node)
        {
            errorList1.SetErrors(m_projectMenuController.CurrentProject.DomainUsage.Usages(node).Select(u => new ErrorList.Element(u.Node, u.File, u.Description)).ToList());
        }

        private void connectorColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (System.Windows.Forms.ColorDialog d = new ColorDialog())
            {
                d.Color = m_config.ColorScheme.ConnectorColor;
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    m_config.ColorScheme.ConnectorColor = d.Color;
                }
            }
        }

        private IAudioProviderCustomization GetAudioCustomizer()
        {
            var customizations = AudioProvider.DefaultCustomization.Only().Concat(PluginAudioProviderCustomizations()).ToList();
            return customizations.FirstOrDefault(c => c.Name == m_config.AudioCustomization.Value) ?? AudioProvider.DefaultCustomization;
        }

        private void audioNamingMethodToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var customizations = AudioProvider.DefaultCustomization.Only().Concat(PluginAudioProviderCustomizations()).ToList();
            var config = GetAudioCustomizer();
            audioNamingMethodToolStripMenuItem.DropDownItems.Clear();
            foreach (var customization in customizations)
            {
                var c = customization;
                ToolStripMenuItem item = new ToolStripMenuItem(customization.Name);
                item.CheckState = customization.GetType() == config.GetType() ? CheckState.Checked : CheckState.Unchecked;
                item.CheckOnClick = true;
                item.CheckedChanged += (a, b) =>
                    {
                        foreach (var i in audioNamingMethodToolStripMenuItem.DropDownItems)
                            if (i != item)
                                item.CheckState = CheckState.Unchecked;
                        m_config.AudioCustomization.Value = c.Name;
                    };
                audioNamingMethodToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private List<T> GetAllFactories<T>() where T : class
        {
            List<T> result = new List<T>();
            foreach (var pa in m_config.Plugins.UnfilteredAssemblies.Select(a => a.Assembly).Concat(Assembly.GetExecutingAssembly().Only()))
            {
                var factories = pa.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(T)));
                foreach (var factory in factories)
                {
                    T obj = factory.GetConstructor(Type.EmptyTypes).Invoke(new object[0]) as T;
                    result.Add(obj);
                }
            }
            return result;
        }

        private void InitialiseExportMenu()
        {
            var exporters = GetAllFactories<IProjectExporter>();
            foreach (var exporter in exporters)
            {
                var e = exporter;
                var item = exportToolStripMenuItem.DropDownItems.Add(e.Name);
                item.Click += (a, b) => { e.Export(m_projectMenuController.CurrentProject, m_config.ExportPath, l => projectExplorer.CurrentLocalizer.Localize(l), new ErrorCheckerUtils(m_projectMenuController.CurrentProject.ConversationDataSource)); };
            }
        }
    }
}
