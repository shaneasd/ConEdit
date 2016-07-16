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
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
using Conversation.Serialization;
using System.Xml.Linq;

namespace ConversationEditor
{
    internal partial class Form1 : Form
    {
        ErrorCheckerController m_errorCheckerController;
        SharedContext m_context;
        Config m_config;
        INodeFactory m_conversationNodeFactory;
        INodeFactory m_domainNodeFactory;
        ProjectMenuController m_projectMenuController;

        ConversationEditorControl m_domainEditor = new ConversationEditorControl() { Dock = DockStyle.Fill, ShowGrid = true };
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

                    if (value == m_conversationEditor)
                    {
                        findAndReplaceToolStripMenuItem.Visible = true;
                        findAndReplaceToolStripMenuItem.Text = "Find in Conversations";
                    }
                    else if (value == m_domainEditor)
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

                    if (m_currentEditor != null)
                        projectExplorer.Select(CurrentFile);
                }
            }
        }

        public IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> CurrentFile
        {
            get
            {
                if (CurrentEditor == m_domainEditor)
                    return m_domainEditor.CurrentFile;
                else if (CurrentEditor == m_conversationEditor)
                    return m_conversationEditor.CurrentFile;
                else if (CurrentEditor == m_projectGraphEditor)
                    return m_projectGraphEditor.CurrentFile;
                else
                    return DummyConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>.Instance;
            }
        }

        /// <summary>
        /// All (in theory) colors used within the GUI. Some of these colors updated at runtime from config
        /// </summary>
        readonly ColorScheme m_scheme = new ColorScheme();

        public Form1()
        {
            InitializeComponent();

            errorList1.ColorScheme = m_scheme;
            projectExplorer.Scheme = m_scheme;

            m_context = new SharedContext();

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.GenericSystemHover.ico"))
                this.Icon = new Icon(stream);

            KeyPreview = true;

            try
            {
                var willEdit = new WillEdit
                (
                    isDecimal:  guid => m_context.CurrentProject.Value.ConversationDataSource.IsDecimal(guid) || m_context.CurrentProject.Value.DomainDataSource.IsDecimal(guid),
                    isDynamicEnum: guid => m_context.CurrentProject.Value.ConversationDataSource.IsDynamicEnum(guid) || m_context.CurrentProject.Value.DomainDataSource.IsDynamicEnum(guid),
                    isLocalDynamicEnum: guid => m_context.CurrentProject.Value.ConversationDataSource.IsLocalDynamicEnum(guid) || m_context.CurrentProject.Value.DomainDataSource.IsLocalDynamicEnum(guid),
                    isEnum: guid => m_context.CurrentProject.Value.ConversationDataSource.IsEnum(guid) || m_context.CurrentProject.Value.DomainDataSource.IsEnum(guid),
                    isInteger: guid => m_context.CurrentProject.Value.ConversationDataSource.IsInteger(guid) || m_context.CurrentProject.Value.DomainDataSource.IsInteger(guid)
                );
                m_config = new Config("config.xml", willEdit);
            }
            catch (Config.LoadFailedException)
            {
                throw;
            }

            projectExplorer.Initialize(m_context, m_config.FileFilters);

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

            menuStrip1.Renderer = m_scheme.ContextMenu;
            splitContainer1.BackColor = m_scheme.FormBackground;
            splitContainer1.ForeColor = m_scheme.Foreground;
            splitContainer2.BackColor = m_scheme.FormBackground;
            splitContainer2.ForeColor = m_scheme.Foreground;
            BackColor = m_scheme.FormBackground;
            ForeColor = m_scheme.Foreground;
            errorList1.ForeColor = m_scheme.Foreground;
            errorList1.BackColor = m_scheme.Background;

            projectExplorer.m_contextMenuItemsFactory = new WrapperContextMenuItemsFactory((mainAssembly) => m_config.Plugins.UnfilteredAssemblies(mainAssembly));
        }

        private void InitialiseNodeFactory()
        {
            m_conversationNodeFactory = new NodeFactory(m_config.ConversationNodeRenderers, GetAllOfType<NodeUI.IFactory>(), a => m_config.ConversationNodeRenderers.ValueChanged += a, guid => m_context.CurrentProject.Value.Localizer.Localize(guid), () => m_context.CurrentProject.Value.ConversationDataSource);
            m_domainNodeFactory = new NodeFactory(m_config.DomainNodeRenderers, guid => m_context.CurrentProject.Value.Localizer.Localize(guid), () => m_context.CurrentProject.Value.DomainDataSource);
        }

        private void InitialiseOptionsMenu()
        {
            tsmiShowGrid.Checked = m_config.GraphView.ShowGrid;
            tsmiSnapToGrid.Checked = m_config.GraphView.SnapToGrid;
            tsmiShowIds.Checked = m_config.GraphView.ShowIds;
        }

        private bool SelectNode(ConversationNode node, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> file)
        {
            if (!file.Nodes.Contains(node))
                return false;
            if (m_context.CurrentProject.Value.Conversations.Contains(file))
            {
                projectExplorer.Select(file);
                m_conversationEditor.CurrentFile = file;
                m_conversationEditor.SelectNode(node);
                CurrentEditor = m_conversationEditor;
            }
            else if (m_context.CurrentProject.Value.DomainFiles.Contains(file))
            {
                projectExplorer.Select(file);
                m_domainEditor.CurrentFile = file;
                m_domainEditor.SelectNode(node);
                CurrentEditor = m_domainEditor;
            }
            else if (object.Equals(file.File, m_context.CurrentProject.Value.File))
            {
                m_projectGraphEditor.SelectNode(node);
                CurrentEditor = m_projectGraphEditor;
            }
            else
                return false;
            return true;
        }

        /// <summary>
        /// Search plugin assemblies for all classes implementing interface T
        /// </summary>
        /// <typeparam name="T">The interface that must be implemented</typeparam>
        /// <returns>All classes that implement T</returns>
        private IEnumerable<T> GetAllOfType<T>(MainAssemblies mainAssemblies = MainAssemblies.Include) where T : class
        {
            List<T> result = new List<T>();
            var assemblies = m_config.Plugins.UnfilteredAssemblies(mainAssemblies).Select(a => a.Assembly);
            foreach (var pa in assemblies)
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

        private void InitialiseConversationEditor()
        {
            InitializeGraphEditor(ConversationCopyPasteController.Instance, m_context.CurrentProject.Value.ConversationDataSource, m_conversationEditor, new GraphContextMenuItems(FindReferences));
        }

        private void InitializeDomainEditor()
        {
            InitializeGraphEditor(ConversationCopyPasteController.Instance, m_context.CurrentProject.Value.DomainDataSource, m_domainEditor, new DomainContextMenuItems(FindReferences));
        }

        private void InitializeProjectEditor()
        {
            InitializeGraphEditor(ConversationCopyPasteController.Instance, ProjectDomain.Instance, m_projectGraphEditor, new ProjectContextMenuItems());
        }

        private void InitializeGraphEditor(ConversationCopyPasteController copyPasteController, IDataSource datasource, GraphEditorControl<ConversationNode> editor, IMenuActionFactory<ConversationNode> basicItems)
        {
            editor.Initialise(Edit, copyPasteController, errorList1.SetErrors);
            editor.SetContext(datasource, m_context.CurrentProject.Value.Localizer, m_context.CurrentProject.Value);
            editor.m_contextMenu.Opening += () => editor.RefreshContextMenu(basicItems.Only().Concat(GetAllOfType<IMenuActionFactory<ConversationNode>>(MainAssemblies.Ignore)));

            Action updateGraphViewFromConfig = () =>
            {
                editor.SnapToGrid = m_config.GraphView.SnapToGrid;
                editor.ShowGrid = m_config.GraphView.ShowGrid;
                editor.ShowIds = m_config.GraphView.ShowIds;
                m_scheme.Connectors = m_config.ColorScheme.ConnectorColor;
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
                        if (m_context.CurrentProject.Value.ReloadConversationDatasourceIfRequired())
                        {
                            m_conversationEditor.SetContext(m_context.CurrentProject.Value.ConversationDataSource, m_context.CurrentProject.Value.Localizer, m_context.CurrentProject.Value);
                            m_conversationEditor.UpdateKeyMappings();
                        }
                    }
                }

                m_conversationEditor.CurrentFile = projectExplorer.SelectedConversation;
                m_domainEditor.CurrentFile = projectExplorer.CurrentDomainFile;
                //m_projectGraphEditor.CurrentFile = m_context.CurrentProject.Value;

                if (projectExplorer.SelectedConversation != null && projectExplorer.SelectedConversation.File.Exists)
                    CurrentEditor = m_conversationEditor;
                else if (projectExplorer.CurrentDomainFile != null && projectExplorer.CurrentDomainFile.File.Exists)
                    CurrentEditor = m_domainEditor;
                else if (projectExplorer.ProjectSelected)
                    CurrentEditor = m_projectGraphEditor;
                else
                    CurrentEditor = null;

                UpdateUndoMenu();
            };
            m_context.CurrentLocalization.Changed.Register(this, (a, value) =>
            {
                //Invalidate graph editors as they can display localized text
                m_conversationEditor.Invalidate();
                m_domainEditor.Invalidate();
            });
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

        void ProjectChanged(IProject project)
        {
            Project.TConfig projectConfig = ReadProjectConfig(project);

            m_conversationEditor.SetContext(project.ConversationDataSource, project.Localizer, project);
            m_conversationEditor.UpdateKeyMappings();

            m_domainEditor.SetContext(project.DomainDataSource, project.Localizer, project);
            m_domainEditor.UpdateKeyMappings();

            projectExplorer.SetProject(project, projectConfig);
            var p = project as Project;
            if (p != null)
            {
                var con = projectConfig.LastEdited != null ? project.Conversations.FirstOrDefault(c => c.File.File.FullName == p.Rerout(projectConfig.LastEdited.Only()).Single().FullName) : null;
                var dom = projectConfig.LastEdited != null ? project.DomainFiles.FirstOrDefault(c => c.File.File.FullName == p.Rerout(projectConfig.LastEdited.Only()).Single().FullName) : null;
                m_conversationEditor.CurrentFile = con;
                m_domainEditor.CurrentFile = dom;
                if (con != null)
                {
                    projectExplorer.Select(con);
                    CurrentEditor = m_conversationEditor;
                }
                else if (dom != null)
                {
                    projectExplorer.Select(dom);
                    CurrentEditor = m_domainEditor;
                }
                else
                    CurrentEditor = null;
                Func<ILocalizationFile, bool> matchesLastLocalization = c => c.File.File.FullName == (p.Rerout(projectConfig.LastLocalization.Only()).Single()).FullName;
                if (projectConfig.LastLocalization != null)
                {
                    var match = project.LocalizationFiles.FirstOrDefault(matchesLastLocalization);
                    if (match != null)
                        m_context.CurrentLocalization.Value = match;
                }
            }

            Text = project.File.Exists ? project.File.File.Name : "No Project Open";

            project.File.Moved += (change) => Text = project.File.File.Name;
        }

        public void InitialiseProjectMenu()
        {
            //fromXmlToolStripMenuItem.Click += (a, b) =>
            //{
            //    if (m_projectMenuController.Exit())
            //        m_dataSourceController.LoadFromXml();
            //};

            m_context.CurrentProject.Changed.Register(this, (a, b) => a.ProjectChanged(b.to));
            m_projectMenuController = new ProjectMenuController(m_context, m_config.ProjectHistory, m_conversationNodeFactory, m_domainNodeFactory, a => Invoke(a), m_config.Plugins, GetAudioCustomizer);

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
                                    MessageBox.Show("Failed to access " + m_context.CurrentProject.Value.File.File.FullName + " for saving");
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
            var editorFactory = GetAllOfType<IParameterEditorFactory>().Where(e => e.Guid == id).Single();
            IParameterEditor<Control> editor = editorFactory.Make(m_scheme);
            editor.Setup(data);
            return editor;
        }

        private IParameterEditor<Control> GetParameterEditor(ParameterType id, ParameterEditorSetupData data)
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
            {
                //TODO: This should probably be done by the project itself and should certainly handle local enums better (i.e. at all)
                Func<IParameter, string, IEnumerable<string>> autoCompleteSuggestions = (p, s) => m_context.CurrentProject.Value.AutoCompleteSuggestions(p, s, (t) => m_context.CurrentProject.Value.ConversationDataSource.GetSource(t, CurrentFile));

                ConfigureResult2 result = m_config.NodeEditors[data.NodeTypeId].GetEditorFactory().Edit(m_scheme, data, audioContext, GetParameterEditor, m_context.CurrentProject.Value.Localizer, m_context.CurrentProject.Value.AudioProvider, autoCompleteSuggestions);
                return result.Transformed(a => OnOk(m_context.CurrentProject.Value.AudioProvider, a), b => new ConfigureResult(b));
            }
        }

        private static ConfigureResult OnOk(IAudioLibrary audioProvider, IEnumerable<UpdateParameterData> updates)
        {
            List<Action> undo = new List<Action>();
            List<Action> redo = new List<Action>();
            foreach (UpdateParameterData updateParameterData in updates)
            {
                if (updateParameterData != null)
                {
                    SimpleUndoPair? actions = updateParameterData.Actions;
                    if (actions != null)
                    {
                        undo.Add(actions.Value.Undo);
                        redo.Add(actions.Value.Redo);
                    }
                    if (updateParameterData.Audio != null)
                    {
                        undo.Add(() => audioProvider.UpdateUsage(updateParameterData.Audio.Value));
                        redo.Add(() => audioProvider.UpdateUsage(updateParameterData.Audio.Value));
                    }
                }
            }
            if (undo.Any())
            {
                return new SimpleUndoPair { Redo = () => redo.ForEach(a => a()), Undo = () => undo.ForEach(a => a()) };
            }
            else
            {
                return ConfigureResult.NotApplicable; //This isn't exactly what NotApplicable was intended for but it's the closest match and I can't see a functional difference
            }
        }

        private void errorCheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO: Error checking for domain files

            var errors = m_errorCheckerController.CheckForErrors(CurrentFile.Nodes, m_context.ErrorCheckerUtils());
            errorList1.SetErrors(errors.Select(error => ErrorList.MakeElement(error, CurrentFile)));
        }

        private void testEverythingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conversations = m_context.CurrentProject.Value.Conversations;
            var errors = conversations.SelectMany(
                            c => m_errorCheckerController.CheckForErrors(c.Nodes, m_context.ErrorCheckerUtils()).Select(
                                error => ErrorList.MakeElement(error, c)));
            errorList1.SetErrors(errors);
        }

        private void errorList1_HightlightNode(ConversationNode node, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> file, BoolRef success)
        {
            success &= SelectNode(node, file);
        }

        static Project.TConfig ReadProjectConfig(IProject project)
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
            var projectFile = m_context.CurrentProject.Value.File;
            string configPath = null;
            string lastDocument = null;
            string lastLocalization = null;
            if (projectFile.Exists)
            {
                configPath = m_context.CurrentProject.Value.File.File.FullName + ".config";
                lastDocument = projectExplorer.SelectedConversation != null ? FileSystem.RelativePath(projectExplorer.SelectedConversation.File.File, m_context.CurrentProject.Value.Origin) :
                               projectExplorer.CurrentDomainFile != null ? FileSystem.RelativePath(projectExplorer.CurrentDomainFile.File.File, m_context.CurrentProject.Value.Origin) :
                               null;
                if (m_context.CurrentLocalization.Value.File != null)
                    lastLocalization = FileSystem.RelativePath(m_context.CurrentLocalization.Value.File.File, m_context.CurrentProject.Value.Origin);
            }
            if (!m_projectMenuController.Exit())
                e.Cancel = true;
            else
            {
                if (m_currentEditor != null)
                    m_currentEditor.CurrentFile = DummyConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>.Instance;
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
                        using (var s = Util.LoadFileStream(configPath, FileMode.OpenOrCreate, FileAccess.Write, 0))
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
            m_errorCheckerController.Configure(m_scheme);
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
            m_config.GraphView.ShowIds = tsmiShowIds.Checked;
        }

        private void customiseParameterEditorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var data = new ParameterEditorCustomization(m_context.CurrentProject.Value.ConversationDataSource, m_config.ParameterEditors, GetAllOfType<IParameterEditorFactory>());
            //var config = new MapConfig<ParameterType, Guid>("ParameterEditors", kvp => new KeyValuePair<string,string>(kvp.Key.Serialized(), kvp.Value.ToString())
            //                                                    , kvp => new KeyValuePair<ParameterType,Guid>(ParameterType.Parse(kvp.Key), Guid.Parse(kvp.Value))
            //                                                    , g => DefaultEnumEditor.Factory.GUID);
            Func<ParameterType, ParameterEditorSetupData, IParameterEditor<Control>> config = (id, d) =>
            {
                var result = new DefaultEnumEditor();
                result.Scheme = m_scheme;
                result.Setup(d);
                return result;
            };

            var editor = new DefaultNodeEditorFactory();
            editor.Edit(m_scheme, data, null, config, null, null, null).Transformed(a => OnOk(DummyAudioLibrary.Instance, a), a => a).Do(a => a.Redo(), a => { });

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
            var data = new NodeRendererCustomization(m_context.CurrentProject.Value.ConversationDataSource, m_config.ConversationNodeRenderers, GetAllOfType<NodeUI.IFactory>());
            Func<ParameterType, ParameterEditorSetupData, IParameterEditor<Control>> config = (id, d) =>
            {
                var result = new DefaultEnumEditor();
                result.Scheme = m_scheme;
                result.Setup(d);
                return result;
            };
            var editor = new DefaultNodeEditorFactory();
            editor.Edit(m_scheme, data, null, config, null, null, null).Transformed(a => OnOk(DummyAudioLibrary.Instance, a), a => a).Do(a => a.Redo(), a=> { });
            using (m_config.ConversationNodeRenderers.SuppressValueChanged())
            {
                foreach (var d in data.Parameters)
                {
                    m_config.ConversationNodeRenderers[Id<NodeTypeTemp>.ConvertFrom(d.TypeId)] = (d as IEnumParameter).Value;
                }
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
                form.Initalize(m_scheme, m_config.Plugins);
                form.ShowDialog();
            }
        }

        private void customiseNodeEditorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (NodeEditorCustomizer form = new NodeEditorCustomizer(m_context.CurrentProject.Value.ConversationDataSource, m_config.NodeEditors, m_config.Plugins))
            {
                form.ShowDialog();
            }
        }

        private void findAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IEnumerable<IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>> search;
            if (CurrentEditor == m_conversationEditor)
            {
                search = m_context.CurrentProject.Value.Conversations;
            }
            else if (CurrentEditor == m_domainEditor)
            {
                search = m_context.CurrentProject.Value.DomainFiles;
            }
            else
            {
                throw new InternalLogicException("fvjblanbl");
            }

            var d = new FindAndReplaceDialog(search, m_context.CurrentProject.Value.Localizer, () => CurrentFile);

            d.FocusNode += (node, file) =>
            {
                if (m_context.CurrentProject.Value.Conversations.Contains(file))
                {
                    projectExplorer.Select(file);
                    m_conversationEditor.CurrentFile = file;
                    CurrentEditor = m_conversationEditor;
                    m_conversationEditor.SelectNode(node);
                }
                else if (m_context.CurrentProject.Value.DomainFiles.Contains(file))
                {
                    projectExplorer.Select(file);
                    m_domainEditor.CurrentFile = file;
                    CurrentEditor = m_domainEditor;
                    m_domainEditor.SelectNode(node);
                }
                else
                    throw new InternalLogicException("skjvblb;wqef");

            };
            d.UpdateDisplay += () => { m_conversationEditor.Invalidate(true); m_domainEditor.Invalidate(true); };
            d.Show();
        }

        private void testProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Func<string, FileInfo> rooted = s =>
                {
                    if (Path.IsPathRooted(s))
                        return new FileInfo(s);
                    else
                        return new FileInfo(Path.Combine(m_context.CurrentProject.Value.File.File.Directory.FullName, s));
                };

            IEnumerable<IConversationFile> conversations = m_context.CurrentProject.Value.Conversations;
            var allAudioReferences = (conversations.SelectMany(c => c.Nodes.SelectMany(n => n.Parameters.OfType<IAudioParameter>().Select(p => new { Node = n, Path = rooted(p.Value.Value).FullName, Document = c })))).ToHashSet();

            var audioFiles = m_context.CurrentProject.Value.AudioFiles.Select(a => a.File.File.FullName).Evaluate();

            var unnecessaryAudioFiles = audioFiles.Except(allAudioReferences, a => a, b => b.Path);
            var danglingPointers = allAudioReferences.Except(audioFiles, b => b.Path, a => a);

            IEnumerable<IErrorListElement> danglingPointerErrors = danglingPointers.Select(a => ErrorList.MakeElement(new DanglingAudioError(a.Path, a.Node), a.Document));
            IEnumerable<IErrorListElement> pointlessAudioErrors = unnecessaryAudioFiles.Select(a => ErrorList.MakeElement(new PointlessAudioError(a), null));
            errorList1.SetErrors(danglingPointerErrors.Concat(pointlessAudioErrors));
        }

        private void FindReferences(ConversationNode node)
        {
            errorList1.SetErrors(m_context.CurrentProject.Value.DomainUsage.Usages(node).Select(u => ErrorList.MakeElement(u.Node, u.File, u.Description)).ToList());
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
            var customizations = GetAllOfType<IAudioProviderCustomization>().ToList();
            return customizations.FirstOrDefault(c => c.Name == m_config.AudioCustomization.Value) ?? AudioProvider.DefaultCustomization;
        }

        private void audioNamingMethodToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var customizations = AudioProvider.DefaultCustomization.Only().Concat(GetAllOfType<IAudioProviderCustomization>()).ToList();
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
            foreach (var pa in m_config.Plugins.UnfilteredAssemblies(MainAssemblies.Include).Select(a => a.Assembly))
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
                item.Click += (a, b) =>
                {
                    if (m_context.CurrentLocalization.Value.IsValid)
                        e.Export(m_context.CurrentProject.Value, m_config.ExportPath, l => m_context.CurrentLocalization.Value.Localize(l), m_context.ErrorCheckerUtils());
                    else
                        MessageBox.Show("Cannot export as there is no currently selected localizer");
                };
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                m_scheme.Dispose();
                m_config.Dispose();
            }
            base.Dispose(disposing);
        }

        private void SanityTest(object sender, EventArgs e)
        {
            m_conversationEditor.SanityTest();
        }
    }
}
