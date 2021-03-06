﻿using System;
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
using ConversationEditor;
using Utilities;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
using Conversation.Serialization;
using System.Xml.Linq;
using System.Globalization;

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

        ConversationEditorControl m_domainEditor;
        ConversationEditorControl m_projectGraphEditor;

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
        ColorScheme Scheme { get; } = new ColorScheme();

        public Form1()
        {
            InitializeComponent();

            errorList1.ColorScheme = Scheme;
            projectExplorer.Scheme = Scheme;

            m_context = new SharedContext();

            m_context.CurrentProject.Changed.Register(c => UpdateLocalizationMenu());

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.GenericSystemHover.ico"))
                this.Icon = new Icon(stream);

            KeyPreview = true;

            m_domainEditor = new ConversationEditorControl();
            try
            {
                m_domainEditor.Dock = DockStyle.Fill;
                m_domainEditor.ShowGrid = true;
                m_projectGraphEditor = new ConversationEditorControl();
                try
                {
                    m_projectGraphEditor.Dock = DockStyle.Fill;
                    m_projectGraphEditor.ShowGrid = true;
                    try
                    {
                        var willEdit = new WillEdit
                        (
                            //We need the guid=> indirection because we want to evaluate m_context.CurrentProject.Value.ConversationDataSource at execution time
                            isDecimal: guid => m_context.CurrentProject.Value.ConversationDataSource.IsDecimal(guid) || m_context.CurrentProject.Value.DomainDataSource.IsDecimal(guid),
                            isDynamicEnum: guid => m_context.CurrentProject.Value.ConversationDataSource.IsDynamicEnum(guid) || m_context.CurrentProject.Value.DomainDataSource.IsDynamicEnum(guid),
                            isLocalDynamicEnum: guid => m_context.CurrentProject.Value.ConversationDataSource.IsLocalDynamicEnum(guid) || m_context.CurrentProject.Value.DomainDataSource.IsLocalDynamicEnum(guid),
                            isEnum: guid => m_context.CurrentProject.Value.ConversationDataSource.IsEnum(guid) || m_context.CurrentProject.Value.DomainDataSource.IsEnum(guid),
                            isInteger: guid => m_context.CurrentProject.Value.ConversationDataSource.IsInteger(guid) || m_context.CurrentProject.Value.DomainDataSource.IsInteger(guid),
                            isLocalizedString: guid => m_context.CurrentProject.Value.ConversationDataSource.IsLocalizedString(guid)
                        );
                        m_config = new Config("config.xml", willEdit);
                    }
                    catch (ConfigLoadFailedException)
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

                    menuStrip1.Renderer = Scheme.ContextMenu;
                    splitContainer1.BackColor = Scheme.FormBackground;
                    splitContainer1.ForeColor = Scheme.Foreground;
                    splitContainer2.BackColor = Scheme.FormBackground;
                    splitContainer2.ForeColor = Scheme.Foreground;
                    BackColor = Scheme.FormBackground;
                    ForeColor = Scheme.Foreground;
                    errorList1.ForeColor = Scheme.Foreground;
                    //errorList1.BackColor = Scheme.Background;

                    projectExplorer.m_contextMenuItemsFactory = new WrapperContextMenuItemsFactory((mainAssembly) => m_config.Plugins.UnfilteredAssemblies(mainAssembly));
                }
                catch
                {
                    m_projectGraphEditor.Dispose();
                    throw;
                }
            }
            catch
            {
                m_domainEditor.Dispose();
                throw;
            }

        }

        List<ToolStripMenuItem> localizationMenuItems = new List<ToolStripMenuItem>();
        private void UpdateLocalizationMenu()
        {
            Action update = () =>
            {
                foreach (var item in localizationMenuItems)
                {
                    localizationToolStripMenuItem.DropDownItems.Remove(item);
                }
                localizationMenuItems.Clear();

                int insertIndex = 0;
                foreach (var s in m_context.CurrentProject.Value.Localizer.LocalizationSets)
                {
                    var set = s;
                    ToolStripMenuItem item = new ToolStripMenuItem(set.Name);
                    item.Click += (object sender, EventArgs e) => { m_context.CurrentLocalization.Value = set; };
                    localizationToolStripMenuItem.DropDownItems.Insert(insertIndex, item);
                    localizationMenuItems.Add(item);
                    insertIndex++;
                }
            };
            update();
            m_context.CurrentProject.Value.Localizer.LocalizationSetsChanged += update;
        }

        private void InitialiseNodeFactory()
        {
            m_conversationNodeFactory = new NodeFactory(m_config.ConversationNodeRenderers, GetAllOfType<NodeUI.IFactory>(), a => m_config.ConversationNodeRenderers.ValueChanged += a, (type, textId) => m_context.CurrentProject.Value.Localizer.Localize(type, textId), () => m_context.CurrentProject.Value.ConversationDataSource);
            m_domainNodeFactory = new NodeFactory(m_config.DomainNodeRenderers, (type, textId) => m_context.CurrentProject.Value.Localizer.Localize(type, textId), () => m_context.CurrentProject.Value.DomainDataSource);
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
        private IEnumerable<T> GetAllOfType<T>(MainAssembly mainAssemblies = MainAssembly.Include) where T : class
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
            editor.SetContext(datasource, m_context.CurrentProject.Value.Localizer, m_context.CurrentProject.Value, m_context);
            editor.m_contextMenu.Opening += () => editor.RefreshContextMenu(basicItems.Only().Concat(GetAllOfType<IMenuActionFactory<ConversationNode>>(MainAssembly.Ignore)));

            Action updateGraphViewFromConfig = () =>
            {
                editor.SnapToGrid = m_config.GraphView.SnapToGrid;
                editor.ShowGrid = m_config.GraphView.ShowGrid;
                editor.ShowIds = m_config.GraphView.ShowIds;
                Scheme.Connectors = m_config.ColorScheme.ConnectorColor;
                editor.MinorGridSpacing = m_config.GraphView.MinorGridSpacing;
                editor.MajorGridSpacing = m_config.GraphView.MajorGridSpacing;
            };
            updateGraphViewFromConfig();
            m_config.GraphView.ValueChanged += updateGraphViewFromConfig;
            m_config.ColorScheme.ValueChanged += updateGraphViewFromConfig;
        }

        private void InitialiseEditMenu()
        {
            cutToolStripMenuItem.Click += (a, b) => { if (CurrentEditor != null) CurrentEditor.CutSelection(); };
            copyToolStripMenuItem.Click += (a, b) => { if (CurrentEditor != null) CurrentEditor.CopySelection(); };
            duplicateToolStripMenuItem.Click += (a, b) => { if (CurrentEditor != null) CurrentEditor.DuplicateSelection(); };
            selectAllToolStripMenuItem.Click += (a, b) => { if (CurrentEditor != null) CurrentEditor.SelectAll(); };
            pasteToolStripMenuItem.Click += (a, b) => { if (CurrentEditor != null) CurrentEditor.Paste(null); };
            ungroupToolStripMenuItem.Click += (a, b) => { if (CurrentEditor != null) CurrentEditor.UngroupSelection(); };
            groupToolStripMenuItem.Click += (a, b) => { if (CurrentEditor != null) CurrentEditor.GroupSelection(); };

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
                            m_conversationEditor.SetContext(m_context.CurrentProject.Value.ConversationDataSource, m_context.CurrentProject.Value.Localizer, m_context.CurrentProject.Value, m_context);
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
                try
                {
                    item.Click += (b, c) => { e.UndoUpToAndIncluding(); UpdateUndoMenu(); };
                    if (first)
                    {
                        item.ShortcutKeys = Keys.Control | Keys.Z;
                        first = false;
                    }
                }
                catch
                {
                    item.Dispose();
                    throw;
                }

                undoToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        void ProjectChanged(IProject project)
        {
            Project.TConfig projectConfig = ReadProjectConfig(project);

            m_conversationEditor.SetContext(project.ConversationDataSource, project.Localizer, project, m_context);
            m_conversationEditor.UpdateKeyMappings();

            m_domainEditor.SetContext(project.DomainDataSource, project.Localizer, project, m_context);
            m_domainEditor.UpdateKeyMappings();

            projectExplorer.SetProject(project, projectConfig);
            if (project is Project)
            {
                var con = projectConfig.LastEdited != null ? project.Conversations.FirstOrDefault(c => c.Id == projectConfig.LastEdited) : null;
                var dom = projectConfig.LastEdited != null ? project.DomainFiles.FirstOrDefault(c => c.Id == projectConfig.LastEdited) : null;
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
                Func<Project.TData.LocalizerSetData, bool> matchesLastLocalization = c => c.Id == projectConfig.LastLocalization;
                if (projectConfig.LastLocalization != null)
                {
                    var match = project.Localizer.LocalizationSets.FirstOrDefault(matchesLastLocalization);
                    if (match != null)
                        m_context.CurrentLocalization.Value = match;
                }
            }

            Action updateText = () =>
            {
                Text = project.File.Exists ? (project.File.File.Name + " - " + m_context.CurrentLocalization.Value?.Name ?? "No Localization") : "No Project Open";
            };
            updateText();
            m_context.CurrentLocalization.Changed.Register(c => updateText());

            project.File.Moved += (change) => updateText();
        }

        public void InitialiseProjectMenu()
        {
            //fromXmlToolStripMenuItem.Click += (a, b) =>
            //{
            //    if (m_projectMenuController.Exit())
            //        m_dataSourceController.LoadFromXml();
            //};

            m_context.CurrentProject.Changed.Register(this, (a, b) => a.ProjectChanged(b.To));
            m_projectMenuController = new ProjectMenuController(m_context, m_config.ProjectHistory, m_conversationNodeFactory, m_domainNodeFactory, a => Invoke(a), m_config.Plugins, GetAudioCustomizer, new UpToDateFile.BackEnd());

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

        private IParameterEditor GetParameterEditor(Guid id, ParameterEditorSetupData data)
        {
            var editorFactory = GetAllOfType<IParameterEditorFactory>().Where(e => e.Guid == id).Single();
            IParameterEditor editor = editorFactory.Make(Scheme);
            editor.Setup(data);
            return editor;
        }

        private IParameterEditor GetParameterEditor(ParameterType id, ParameterEditorSetupData data)
        {
            return GetParameterEditor(m_config.ParameterEditors[id], data);
        }

        private ConfigureResult Edit(IConversationNodeData data, AudioGenerationParameters audioContext)
        {
            if (data is UnknownEditable)
            {
                MessageBox.Show("Cannot edit this node as it is not fully defined");
                return ConfigureResult.NotApplicable;
            }
            else
            {
                AutoCompleteSuggestionsDelegate autoCompleteSuggestions = (p, s) => m_context.CurrentProject.Value.AutoCompleteSuggestions(p, s, CurrentFile);

                ConfigureResult2 result = m_config.NodeEditors[data.NodeTypeId].GetEditorFactory().Edit(Scheme, data, audioContext, GetParameterEditor, m_context.CurrentProject.Value.Localizer, m_context.CurrentProject.Value.AudioProvider, autoCompleteSuggestions);
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
                return new Project.TConfig(lastDocument != null ? Id<FileInProject>.Parse(lastDocument.Value) : null,
                                           lastLocalization != null ? Id<Project.TData.LocalizerSetData>.Parse(lastLocalization.Value) : Id<Project.TData.LocalizerSetData>.FromGuid(Guid.Empty));
            }
            return new Project.TConfig(null, null);
        }

        static void WriteProjectConfig(string configPath, Project.TConfig projectConfig)
        {
            var config = new XElement("Config");
            if (projectConfig.LastEdited != null)
                config.Add(new XElement("LastDocument", projectConfig.LastEdited.Serialized()));
            if (projectConfig.LastLocalization != null)
                config.Add(new XElement("LastLocalization", projectConfig.LastLocalization.Serialized()));
            XDocument doc = new XDocument(config);

            try
            {
                using (var s = Util.LoadFileStream(configPath, FileMode.OpenOrCreate, FileAccess.Write, 0))
                {
                    s.SetLength(0);
                    doc.Save(s);
                }
            }
            catch (MyFileLoadException ee)
            {
                MessageBox.Show("Failed to save project config" + ee.Message + ee.InnerException.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var projectFile = m_context.CurrentProject.Value.File;
            string configPath = null;
            Id<FileInProject> lastDocument = null;
            Id<Project.TData.LocalizerSetData> lastLocalization = null;
            if (projectFile.Exists)
            {
                configPath = m_context.CurrentProject.Value.File.File.FullName + ".config";
                lastDocument = projectExplorer.SelectedConversation != null ? projectExplorer.SelectedConversation.Id :
                               projectExplorer.CurrentDomainFile?.Id;
                lastLocalization = m_context.CurrentLocalization.Value?.Id;
            }
            Project.TConfig projectConfig = new Project.TConfig(lastDocument, lastLocalization);
            if (!m_projectMenuController.Exit())
                e.Cancel = true;
            else
            {
                if (CurrentEditor != null)
                    CurrentEditor.CurrentFile = DummyConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>.Instance;
                if (configPath != null)
                {
                    WriteProjectConfig(configPath, projectConfig);
                }
            }

            //TODO: Globals ewww
            if (!e.Cancel)
                UndoQueue.Logger.Dispose();
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
            m_errorCheckerController.Configure(Scheme);
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
            Func<ParameterType, ParameterEditorSetupData, IParameterEditor> config = (id, d) =>
            {
                var result = new DefaultEnumEditor();
                try
                {
                    result.Scheme = Scheme;
                    result.Setup(d);
                }
                catch
                {
                    result.Dispose();
                    throw;
                }
                return result;
            };

            var editor = new DefaultNodeEditorFactory();
            editor.Edit(Scheme, data, null, config, null, null, null).Transformed(a => OnOk(DummyAudioLibrary.Instance, a), a => a).Do(a => a.Redo(), a => { });

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
            Func<ParameterType, ParameterEditorSetupData, IParameterEditor> config = (id, d) =>
            {
                var result = new DefaultEnumEditor();
                try
                {
                    result.Scheme = Scheme;
                    result.Setup(d);
                }
                catch
                {
                    result.Dispose();
                    throw;
                }
                return result;
            };
            var editor = new DefaultNodeEditorFactory();
            editor.Edit(Scheme, data, null, config, null, null, null).Transformed(a => OnOk(DummyAudioLibrary.Instance, a), a => a).Do(a => a.Redo(), a => { });
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
                form.Initalize(Scheme, m_config.Plugins);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "http://stackoverflow.com/questions/8795005/code-analysis-warning-about-disposing-a-form")]
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

            var d = new FindAndReplaceDialog(search, m_context.CurrentProject.Value.Localizer, () => CurrentFile, errorList1.SetErrors);

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
            var allAudioReferences = (conversations.SelectMany(c => c.Nodes.SelectMany(n => n.Data.Parameters.OfType<IAudioParameter>().Select(p => new { Node = n, Path = rooted(p.Value.Value).FullName, Document = c })))).ToHashSet();

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
            var customizations = GetAllOfType<IAudioProviderCustomization>().ToList();
            var config = GetAudioCustomizer();
            audioNamingMethodToolStripMenuItem.DropDownItems.Clear();
            foreach (var customization in customizations)
            {
                var c = customization;
                ToolStripMenuItem item = new ToolStripMenuItem(c.Name);
                item.CheckState = c.GetType() == config.GetType() ? CheckState.Checked : CheckState.Unchecked;
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
            foreach (var pa in m_config.Plugins.UnfilteredAssemblies(MainAssembly.Include).Select(a => a.Assembly))
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
                    if (m_context.CurrentProject.Value.Localizer.IsValid)
                        e.Export(m_context.CurrentProject.Value, m_config.ExportPath, (type, textId) => Tuple.Create(m_context.CurrentProject.Value.Localizer.Localize(type, textId), m_context.CurrentProject.Value.Localizer.LocalizationTime(type, textId)), m_context.ErrorCheckerUtils());
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
                Cleanup();
            }
            base.Dispose(disposing);
        }

        private void Cleanup()
        {
            if (components != null)
                components.Dispose();
            if (Scheme != null)
                Scheme.Dispose();
            if (m_config != null)
                m_config.Dispose();
            if (m_context.CurrentProject != null)
                m_context.CurrentProject.Value.Dispose();
        }

        private void wordCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var localizer = m_context.CurrentProject.Value.Localizer;
            var totalWordCount = localizer.ExistingLocalizations.Select((textId) => StringUtil.WordCount(localizer.Localize(textId.Item1, textId.Item2))).Sum();
            StringBuilder builder = new StringBuilder("Total word count: " + totalWordCount + "\n");
            if (CurrentEditor == m_conversationEditor)
            {
                var conversationWordCount = CurrentEditor.CurrentFile.Nodes.SelectMany(n => n.Data.Parameters.OfType<ILocalizedStringParameter>()).Select(p => StringUtil.WordCount(localizer.Localize(Id<LocalizedStringType>.FromGuid(p.TypeId.Guid), p.Value))).Sum();
                builder.AppendLine("Current conversation word count: " + conversationWordCount);
            }
            MessageBox.Show(builder.ToString());
        }

        private void nodeCountToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int nodeCount = 0;
            foreach (var doc in m_context.CurrentProject.Value.DomainFiles)
                nodeCount += doc.Nodes.Count();
            foreach (var doc in m_context.CurrentProject.Value.Conversations)
                nodeCount += doc.Nodes.Count();

            string message = "Total Nodes: " + nodeCount.ToString(CultureInfo.CurrentCulture);
            message += "\n";
            message += "Current document nodes: " + CurrentFile.Nodes.Count();

            MessageBox.Show(message);
        }

        private void parameterCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int count = 0;
            foreach (var doc in m_context.CurrentProject.Value.DomainFiles)
                foreach (var node in doc.Nodes)
                    count += node.Data.Parameters.Count();
            foreach (var doc in m_context.CurrentProject.Value.Conversations)
                foreach (var node in doc.Nodes)
                    count += node.Data.Parameters.Count();
            string message = "Total Parameters: " + count.ToString(CultureInfo.CurrentCulture);
            message += "\n";
            message += "Current document parameters: " + CurrentFile.Nodes.SelectMany(n => n.Data.Parameters).Count();

            MessageBox.Show(message);
        }

        private void cyclomaticComplexityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            {
                var document = CurrentFile;
                int complexity = CalculateCyclomaticComplexity(document);
                MessageBox.Show("Current document cyclomatic complexity: " + complexity);
            }

            using (StreamWriter w = new StreamWriter("complexity.csv", false))
            {
                foreach (var conversation in m_context.CurrentProject.Value.Conversations)
                {
                    int complexity = CalculateCyclomaticComplexity(conversation);
                    w.WriteLine(conversation.File.File.Name + ", " + complexity);
                }
            }
        }

        private static int CalculateCyclomaticComplexity(IConversationEditorControlData<ConversationNode<INodeGui>, TransitionNoduleUIInfo> document)
        {
            int nodes;
            int connections;
            int endpoints;
            nodes = document.Nodes.Count();
            connections = document.Nodes.SelectMany(n => n.Data.Connectors).SelectMany(c => c.Connections).Count() / 2;
            endpoints = document.Nodes.Where(n => n.Data.Connectors.Where(c => c.Connections.Any()).Count() < 2).Count(); //Nodes that aren't in the middle of a chain
            return connections - nodes + endpoints;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Developed by Shane Tapp. thatguyiknow5@hotmail.com");
        }

        private void setUpLocalizationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetUpLocalizationsForm.SetupLocalizations(m_context.CurrentProject.Value);
        }

        private void layoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentEditor.LayoutNodes();
        }
    }
}
