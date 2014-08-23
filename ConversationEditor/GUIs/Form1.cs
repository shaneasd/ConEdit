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

namespace ConversationEditor
{
    public partial class Form1 : Form
    {
        ErrorCheckerController m_errorCheckerController;
        Config m_config;
        INodeFactory m_conversationNodeFactory;
        INodeFactory m_domainNodeFactory;
        ProjectMenuController m_projectMenuController;

        ConversationEditorControl m_domainEditor2 = new ConversationEditorControl() { Dock = DockStyle.Fill, ShowGrid = true };

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

            CurrentEditor = null;
        }

        private void InitialiseNodeFactory()
        {
            m_conversationNodeFactory = new NodeFactory(m_config.ConversationNodeRenderers, guid => m_projectMenuController.CurrentProject.Localizer.Localize(guid));
            m_domainNodeFactory = new NodeFactory(m_config.DomainNodeRenderers, guid => m_projectMenuController.CurrentProject.Localizer.Localize(guid));
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
                conversationEditorControl1.CurrentFile = file;
                conversationEditorControl1.SelectNode(node);
                CurrentEditor = conversationEditorControl1;
            }
            else if (m_projectMenuController.CurrentProject.DomainFiles.Contains(file))
            {
                m_domainEditor2.CurrentFile = file;
                m_domainEditor2.SelectNode(node);
                CurrentEditor = m_domainEditor2;
            }
            else
                return false;
            return true;
        }

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

        private void InitialiseConversationEditor()
        {
            InitializeGraphEditor(ConversationCopyPasteController.Instance, m_conversationNodeFactory, m_projectMenuController.CurrentProject.ConversationDataSource, conversationEditorControl1, new GraphContextMenuItems(FindReferences, Edit));
        }

        private void InitializeDomainEditor()
        {
            InitializeGraphEditor(ConversationCopyPasteController.Instance, m_domainNodeFactory, m_projectMenuController.CurrentProject.DomainDataSource, m_domainEditor2, new DomainContextMenuItems(FindReferences, Edit));
        }

        private void InitializeGraphEditor(ConversationCopyPasteController copyPasteController, INodeFactory nodefactory, IDataSource datasource, GraphEditorControl<ConversationNode> editor, GraphContextMenuItems basicItems)
        {
            editor.Initialise(Edit, nodefactory, copyPasteController, FindReferences);
            editor.SetContext(datasource, m_projectMenuController.CurrentProject.Localizer);
            editor.m_contextMenu.Opening += () => editor.RefreshContextMenu(basicItems.Only().Concat(PluginContextMenuFactories()));

            Action updateGraphViewFromConfig = () =>
            {
                editor.SnapToGrid = m_config.GraphView.SnapToGrid;
                editor.ShowGrid = m_config.GraphView.ShowGrid;
                editor.ShowIDs = m_config.GraphView.ShowIDs;
                editor.Colors = m_config.ColorScheme.Colors;
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
                    if (m_projectMenuController.CurrentProject.ReloadConversationDatasourceIfRequired())
                    {
                        conversationEditorControl1.SetContext(m_projectMenuController.CurrentProject.ConversationDataSource, m_projectMenuController.CurrentProject.Localizer);
                        conversationEditorControl1.UpdateKeyMappings();
                    }
                }

                conversationEditorControl1.CurrentFile = projectExplorer.SelectedConversation;
                m_domainEditor2.CurrentFile = projectExplorer.CurrentDomainFile;

                if (projectExplorer.SelectedConversation != null && projectExplorer.SelectedConversation.File.Exists)
                    CurrentEditor = conversationEditorControl1;
                else if (projectExplorer.CurrentDomainFile != null && projectExplorer.CurrentDomainFile.File.Exists)
                    CurrentEditor = m_domainEditor2;
                //TODO: What happens if you select an item for which there is no editor?
                //else
                //CurrentEditor = null;

                UpdateUndoMenu();
            };
            projectExplorer.LocalizerSelected += () =>
            {
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
                conversationEditorControl1.SetContext(project.ConversationDataSource, project.Localizer);
                conversationEditorControl1.UpdateKeyMappings();
                conversationEditorControl1.CurrentFile = null;

                m_domainEditor2.SetContext(project.DomainDataSource, project.Localizer);
                m_domainEditor2.UpdateKeyMappings();
                m_domainEditor2.CurrentFile = null;

                projectExplorer.SetProject(project);
                Text = project.File.Exists ? project.File.File.Name : "No Project Open";

                //TODO: Select the most appropriate file?

                project.File.Moved += (from, to) => Text = project.File.File.Name;
            };

            m_projectMenuController = new ProjectMenuController(m_config.ProjectHistory, m_conversationNodeFactory, m_domainNodeFactory, projectExplorer, projectChanged, a => Invoke(a), m_config.Plugins);

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
                                    open = m_projectMenuController.CurrentProject.File.CanClose();
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

        private ConfigureResult Edit(IEditable data)
        {
            if (data is UnknownEditable)
            {
                MessageBox.Show("Cannot edit this node as it is not fully defined");
                return ConfigureResult.NotApplicable;
            }
            else
                return m_config.NodeEditors[data.NodeTypeID].GetEditorFactory().Edit(data, m_config.ParameterEditors, m_projectMenuController.CurrentProject.Localizer, m_projectMenuController.CurrentProject.AudioProvider);
        }

        private void errorCheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO: Error checking for domain files
            var errors = m_errorCheckerController.CheckForErrors(CurrentFile.Nodes);
            errorList1.SetErrors(errors.Select(error => new ErrorList.Element(error, CurrentFile)));
        }

        private void errorList1_HightlightNode(ConversationNode node, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> file, BoolRef success)
        {
            success &= SelectNode(node, file);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!m_projectMenuController.Exit())
                e.Cancel = true;
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
            using (EditorCustomizer ec = new EditorCustomizer())
            {
                ec.Init(m_config.ParameterEditors, m_projectMenuController.CurrentProject.ConversationDataSource);
                ec.ShowDialog();
            }
        }

        private void customiseNodeRendererToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO: Customize domain node renderers?
            using (NodeRendererCustomizer form = new NodeRendererCustomizer(m_projectMenuController.CurrentProject.ConversationDataSource, m_config.ConversationNodeRenderers, m_config.Plugins))
            {
                form.ShowDialog();
            }
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

        class DanglingAudioError : ConversationError<ConversationNode>
        {
            private string m_file;
            public DanglingAudioError(string file, ConversationNode node)
                : base(node.Only())
            {
                m_file = file;
            }

            public override string Message
            {
                get { return "Audio parameter value '" + m_file + "' does not have a corresponding audio file loaded in the project"; }
            }
        }

        class PointlessAudioError : ConversationError<ConversationNode>
        {
            private string m_file;
            public PointlessAudioError(string file)
                : base(Enumerable.Empty<ConversationNode>())
            {
                m_file = file;
            }

            public override string Message
            {
                get { return "Audio file '" + m_file + "' is not referenced by any conversation in this project"; }
            }
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

        private void domainAsCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DomainData builtIn = new DomainData();
            builtIn.Connectors.Add(SpecialConnectors.Input);
            builtIn.Connectors.Add(SpecialConnectors.Output);
            var data = m_projectMenuController.CurrentProject.DomainFiles.Select(d => d.Data).Concat(builtIn.Only());
            using (var sfd = new SaveFileDialog())
            {
                sfd.DefaultExt = "*.cs";
                sfd.AddExtension = true;
                sfd.CreatePrompt = false;
                sfd.InitialDirectory = m_config.ExportPath.Value;
                sfd.OverwritePrompt = true;
                sfd.ValidateNames = true;
                sfd.Title = "Export to C# source file";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    m_config.ExportPath.Value = Path.GetDirectoryName(sfd.FileName);
                    CsDomain<INodeGUI, NodeUIData, ConversationEditorData>.Serializer s = new CsDomain<INodeGUI, NodeUIData, ConversationEditorData>.Serializer(BaseTypeSet.BasicTypeMap(), "MyNamespace");
                    using (var stream = new FileStream(sfd.FileName, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        stream.SetLength(0);
                        s.Write(data, stream);
                    }
                }
            }
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
    }
}
