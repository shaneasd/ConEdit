namespace ConversationEditor
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ConversationEditor.ColorScheme colorScheme4 = new ConversationEditor.ColorScheme();
            ConversationEditor.ColorScheme colorScheme5 = new ConversationEditor.ColorScheme();
            ConversationEditor.ColorScheme colorScheme6 = new ConversationEditor.ColorScheme();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.projectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectNewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectOpenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recentProjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectSaveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectSaveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.duplicateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ungroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findAndReplaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.customiseParameterEditorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.customiseNodeEditorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.customiseNodeRendererToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiShowGrid = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSnapToGrid = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiShowIds = new System.Windows.Forms.ToolStripMenuItem();
            this.connectorColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.audioNamingMethodToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.errorCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testEverythingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.throwTestExceptionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.projectExplorer = new ConversationEditor.ProjectExplorer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.m_conversationEditor = new ConversationEditor.ConversationEditorControl();
            this.errorList1 = new ConversationEditor.ErrorList();
            this.sanityTestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectToolStripMenuItem,
            this.editToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.errorCheckToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.debugToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.menuStrip1.Size = new System.Drawing.Size(989, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // projectToolStripMenuItem
            // 
            this.projectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectNewMenuItem,
            this.projectOpenMenuItem,
            this.recentProjectsToolStripMenuItem,
            this.projectSaveMenuItem,
            this.projectSaveAsMenuItem});
            this.projectToolStripMenuItem.Name = "projectToolStripMenuItem";
            this.projectToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.projectToolStripMenuItem.Text = "Project";
            // 
            // projectNewMenuItem
            // 
            this.projectNewMenuItem.Name = "projectNewMenuItem";
            this.projectNewMenuItem.Size = new System.Drawing.Size(150, 22);
            this.projectNewMenuItem.Text = "New";
            // 
            // projectOpenMenuItem
            // 
            this.projectOpenMenuItem.Name = "projectOpenMenuItem";
            this.projectOpenMenuItem.Size = new System.Drawing.Size(150, 22);
            this.projectOpenMenuItem.Text = "Open";
            // 
            // recentProjectsToolStripMenuItem
            // 
            this.recentProjectsToolStripMenuItem.Name = "recentProjectsToolStripMenuItem";
            this.recentProjectsToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.recentProjectsToolStripMenuItem.Text = "Recent Projects";
            // 
            // projectSaveMenuItem
            // 
            this.projectSaveMenuItem.Name = "projectSaveMenuItem";
            this.projectSaveMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.projectSaveMenuItem.Size = new System.Drawing.Size(150, 22);
            this.projectSaveMenuItem.Text = "Save";
            // 
            // projectSaveAsMenuItem
            // 
            this.projectSaveAsMenuItem.Name = "projectSaveAsMenuItem";
            this.projectSaveAsMenuItem.Size = new System.Drawing.Size(150, 22);
            this.projectSaveAsMenuItem.Text = "Save As";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.duplicateToolStripMenuItem,
            this.selectAllToolStripMenuItem,
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.groupToolStripMenuItem,
            this.ungroupToolStripMenuItem,
            this.findAndReplaceToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.editToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            // 
            // duplicateToolStripMenuItem
            // 
            this.duplicateToolStripMenuItem.Name = "duplicateToolStripMenuItem";
            this.duplicateToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.duplicateToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.duplicateToolStripMenuItem.Text = "Duplicate";
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.selectAllToolStripMenuItem.Text = "Select All";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.redoToolStripMenuItem.Text = "Redo";
            // 
            // groupToolStripMenuItem
            // 
            this.groupToolStripMenuItem.Name = "groupToolStripMenuItem";
            this.groupToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.groupToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.groupToolStripMenuItem.Text = "Group";
            // 
            // ungroupToolStripMenuItem
            // 
            this.ungroupToolStripMenuItem.Name = "ungroupToolStripMenuItem";
            this.ungroupToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U)));
            this.ungroupToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.ungroupToolStripMenuItem.Text = "Ungroup";
            // 
            // findAndReplaceToolStripMenuItem
            // 
            this.findAndReplaceToolStripMenuItem.Name = "findAndReplaceToolStripMenuItem";
            this.findAndReplaceToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findAndReplaceToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.findAndReplaceToolStripMenuItem.Text = "Find and Replace";
            this.findAndReplaceToolStripMenuItem.Click += new System.EventHandler(this.findAndReplaceToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pluginsToolStripMenuItem,
            this.customiseParameterEditorsToolStripMenuItem,
            this.customiseNodeEditorsToolStripMenuItem,
            this.customiseNodeRendererToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.tsmiShowGrid,
            this.tsmiSnapToGrid,
            this.tsmiShowIds,
            this.connectorColorToolStripMenuItem,
            this.audioNamingMethodToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.toolsToolStripMenuItem.Text = "Options";
            // 
            // pluginsToolStripMenuItem
            // 
            this.pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
            this.pluginsToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.pluginsToolStripMenuItem.Text = "Plugins";
            this.pluginsToolStripMenuItem.Click += new System.EventHandler(this.pluginsToolStripMenuItem_Click);
            // 
            // customiseParameterEditorsToolStripMenuItem
            // 
            this.customiseParameterEditorsToolStripMenuItem.Name = "customiseParameterEditorsToolStripMenuItem";
            this.customiseParameterEditorsToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.customiseParameterEditorsToolStripMenuItem.Text = "Customise Parameter Editors";
            this.customiseParameterEditorsToolStripMenuItem.Click += new System.EventHandler(this.customiseParameterEditorsToolStripMenuItem_Click);
            // 
            // customiseNodeEditorsToolStripMenuItem
            // 
            this.customiseNodeEditorsToolStripMenuItem.Name = "customiseNodeEditorsToolStripMenuItem";
            this.customiseNodeEditorsToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.customiseNodeEditorsToolStripMenuItem.Text = "Customise Node Editors";
            this.customiseNodeEditorsToolStripMenuItem.Click += new System.EventHandler(this.customiseNodeEditorsToolStripMenuItem_Click);
            // 
            // customiseNodeRendererToolStripMenuItem
            // 
            this.customiseNodeRendererToolStripMenuItem.Name = "customiseNodeRendererToolStripMenuItem";
            this.customiseNodeRendererToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.customiseNodeRendererToolStripMenuItem.Text = "Customise Node Renderers";
            this.customiseNodeRendererToolStripMenuItem.Click += new System.EventHandler(this.customiseNodeRendererToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.optionsToolStripMenuItem.Text = "Customise Error Checkers";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // tsmiShowGrid
            // 
            this.tsmiShowGrid.CheckOnClick = true;
            this.tsmiShowGrid.Name = "tsmiShowGrid";
            this.tsmiShowGrid.Size = new System.Drawing.Size(212, 22);
            this.tsmiShowGrid.Text = "Show Grid";
            this.tsmiShowGrid.CheckedChanged += new System.EventHandler(this.tsmiShowGrid_CheckedChanged);
            // 
            // tsmiSnapToGrid
            // 
            this.tsmiSnapToGrid.CheckOnClick = true;
            this.tsmiSnapToGrid.Name = "tsmiSnapToGrid";
            this.tsmiSnapToGrid.Size = new System.Drawing.Size(212, 22);
            this.tsmiSnapToGrid.Text = "Snap to Grid";
            this.tsmiSnapToGrid.CheckedChanged += new System.EventHandler(this.tsmiSnapToGrid_CheckedChanged);
            // 
            // tsmiShowIds
            // 
            this.tsmiShowIds.CheckOnClick = true;
            this.tsmiShowIds.Name = "tsmiShowIds";
            this.tsmiShowIds.Size = new System.Drawing.Size(212, 22);
            this.tsmiShowIds.Text = "Show IDs";
            this.tsmiShowIds.CheckedChanged += new System.EventHandler(this.tsmiShowIDs_CheckedChanged);
            // 
            // connectorColorToolStripMenuItem
            // 
            this.connectorColorToolStripMenuItem.Name = "connectorColorToolStripMenuItem";
            this.connectorColorToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.connectorColorToolStripMenuItem.Text = "Connector Color";
            this.connectorColorToolStripMenuItem.Click += new System.EventHandler(this.connectorColorToolStripMenuItem_Click);
            // 
            // audioNamingMethodToolStripMenuItem
            // 
            this.audioNamingMethodToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.defaultToolStripMenuItem});
            this.audioNamingMethodToolStripMenuItem.Name = "audioNamingMethodToolStripMenuItem";
            this.audioNamingMethodToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.audioNamingMethodToolStripMenuItem.Text = "Audio Naming Method";
            this.audioNamingMethodToolStripMenuItem.DropDownOpening += new System.EventHandler(this.audioNamingMethodToolStripMenuItem_DropDownOpening);
            // 
            // defaultToolStripMenuItem
            // 
            this.defaultToolStripMenuItem.Checked = true;
            this.defaultToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.defaultToolStripMenuItem.Name = "defaultToolStripMenuItem";
            this.defaultToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.defaultToolStripMenuItem.Text = "Default";
            // 
            // errorCheckToolStripMenuItem
            // 
            this.errorCheckToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testToolStripMenuItem,
            this.testProjectToolStripMenuItem,
            this.testEverythingToolStripMenuItem});
            this.errorCheckToolStripMenuItem.Name = "errorCheckToolStripMenuItem";
            this.errorCheckToolStripMenuItem.Size = new System.Drawing.Size(89, 20);
            this.errorCheckToolStripMenuItem.Text = "Error Checking";
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.testToolStripMenuItem.Text = "Test Document";
            this.testToolStripMenuItem.Click += new System.EventHandler(this.errorCheckToolStripMenuItem_Click);
            // 
            // testProjectToolStripMenuItem
            // 
            this.testProjectToolStripMenuItem.Name = "testProjectToolStripMenuItem";
            this.testProjectToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.testProjectToolStripMenuItem.Text = "Test Project";
            this.testProjectToolStripMenuItem.Click += new System.EventHandler(this.testProjectToolStripMenuItem_Click);
            // 
            // testEverythingToolStripMenuItem
            // 
            this.testEverythingToolStripMenuItem.Name = "testEverythingToolStripMenuItem";
            this.testEverythingToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.testEverythingToolStripMenuItem.Text = "Test Everything";
            this.testEverythingToolStripMenuItem.Click += new System.EventHandler(this.testEverythingToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.exportToolStripMenuItem.Text = "Export";
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.throwTestExceptionToolStripMenuItem,
            this.sanityTestToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // throwTestExceptionToolStripMenuItem
            // 
            this.throwTestExceptionToolStripMenuItem.Name = "throwTestExceptionToolStripMenuItem";
            this.throwTestExceptionToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.throwTestExceptionToolStripMenuItem.Text = "Throw test Exception";
            this.throwTestExceptionToolStripMenuItem.Click += new System.EventHandler(this.throwTestExceptionToolStripMenuItem_Click);
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.projectExplorer);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(989, 561);
            this.splitContainer1.SplitterDistance = 181;
            this.splitContainer1.TabIndex = 2;
            // 
            // projectExplorer
            // 
            this.projectExplorer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.projectExplorer.Location = new System.Drawing.Point(0, 0);
            this.projectExplorer.Margin = new System.Windows.Forms.Padding(0);
            this.projectExplorer.Name = "projectExplorer";
            colorScheme4.Connectors = System.Drawing.Color.Black;
            this.projectExplorer.Scheme = colorScheme4;
            this.projectExplorer.Size = new System.Drawing.Size(181, 561);
            this.projectExplorer.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.m_conversationEditor);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.errorList1);
            this.splitContainer2.Size = new System.Drawing.Size(804, 561);
            this.splitContainer2.SplitterDistance = 419;
            this.splitContainer2.TabIndex = 4;
            // 
            // conversationEditorControl1
            // 
            this.m_conversationEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            colorScheme5.Connectors = System.Drawing.Color.Black;
            this.m_conversationEditor.Colors = colorScheme5;
            this.m_conversationEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_conversationEditor.GraphScale = 1F;
            this.m_conversationEditor.Location = new System.Drawing.Point(0, 0);
            this.m_conversationEditor.MajorGridSpacing = 0;
            this.m_conversationEditor.MinorGridSpacing = 0;
            this.m_conversationEditor.Name = "conversationEditorControl1";
            this.m_conversationEditor.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.m_conversationEditor.ShowGrid = false;
            this.m_conversationEditor.ShowIds = true;
            this.m_conversationEditor.Size = new System.Drawing.Size(804, 419);
            this.m_conversationEditor.SnapToGrid = false;
            this.m_conversationEditor.TabIndex = 4;
            // 
            // errorList1
            // 
            colorScheme6.Connectors = System.Drawing.Color.Black;
            this.errorList1.ColorScheme = colorScheme6;
            this.errorList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.errorList1.Location = new System.Drawing.Point(0, 0);
            this.errorList1.Name = "errorList1";
            this.errorList1.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.errorList1.Size = new System.Drawing.Size(804, 138);
            this.errorList1.TabIndex = 3;
            // 
            // sanityTestToolStripMenuItem
            // 
            this.sanityTestToolStripMenuItem.Name = "sanityTestToolStripMenuItem";
            this.sanityTestToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.sanityTestToolStripMenuItem.Text = "Sanity Test";
            this.sanityTestToolStripMenuItem.Click += new System.EventHandler(this.SanityTest);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(989, 585);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem errorCheckToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem duplicateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem customiseParameterEditorsToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private ErrorList errorList1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem throwTestExceptionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsmiShowGrid;
        private System.Windows.Forms.ToolStripMenuItem tsmiSnapToGrid;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem customiseNodeRendererToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pluginsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem customiseNodeEditorsToolStripMenuItem;
        private ProjectExplorer projectExplorer;
        private System.Windows.Forms.ToolStripMenuItem projectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectNewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectSaveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectOpenMenuItem;
        private System.Windows.Forms.ToolStripMenuItem groupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ungroupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectSaveAsMenuItem;
        private ConversationEditorControl m_conversationEditor;
        private System.Windows.Forms.ToolStripMenuItem findAndReplaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recentProjectsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsmiShowIds;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectorColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem audioNamingMethodToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defaultToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testEverythingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sanityTestToolStripMenuItem;
    }
}

