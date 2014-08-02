namespace ConversationEditor
{
    partial class GraphEditorControl<TNode>
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.zoomBar = new ConversationEditor.GreyScrollBar();
            this.hScrollBar1 = new ConversationEditor.GreyScrollBar();
            this.vScrollBar1 = new ConversationEditor.GreyScrollBar();
            this.drawWindow = new ConversationEditor.DrawWindow();
            this.SuspendLayout();
            // 
            // zoomBar
            // 
            this.zoomBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.zoomBar.Horizontal = true;
            this.zoomBar.Location = new System.Drawing.Point(0, 303);
            this.zoomBar.Maximum = 2F;
            this.zoomBar.Minimum = 0.1F;
            this.zoomBar.MinimumSize = new System.Drawing.Size(15, 15);
            this.zoomBar.Name = "zoomBar";
            this.zoomBar.PercentageCovered = 0F;
            this.zoomBar.Size = new System.Drawing.Size(351, 15);
            this.zoomBar.TabIndex = 3;
            this.zoomBar.Value = 1F;
            this.zoomBar.Scrolled += new System.Action(this.zoomBar_Scrolled);
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hScrollBar1.Horizontal = true;
            this.hScrollBar1.Location = new System.Drawing.Point(0, 287);
            this.hScrollBar1.Maximum = 100F;
            this.hScrollBar1.Minimum = 0F;
            this.hScrollBar1.MinimumSize = new System.Drawing.Size(15, 15);
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.PercentageCovered = 0F;
            this.hScrollBar1.Size = new System.Drawing.Size(335, 15);
            this.hScrollBar1.TabIndex = 2;
            this.hScrollBar1.Value = 0F;
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.vScrollBar1.Horizontal = false;
            this.vScrollBar1.Location = new System.Drawing.Point(335, 0);
            this.vScrollBar1.Maximum = 100F;
            this.vScrollBar1.Minimum = 0F;
            this.vScrollBar1.MinimumSize = new System.Drawing.Size(15, 15);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.PercentageCovered = 0F;
            this.vScrollBar1.Size = new System.Drawing.Size(15, 287);
            this.vScrollBar1.TabIndex = 1;
            this.vScrollBar1.Value = 0F;
            // 
            // drawWindow
            // 
            this.drawWindow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.drawWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(56)))), ((int)(((byte)(56)))));
            this.drawWindow.HandleNavigation = false;
            this.drawWindow.Location = new System.Drawing.Point(0, 0);
            this.drawWindow.Name = "drawWindow";
            this.drawWindow.Size = new System.Drawing.Size(334, 286);
            this.drawWindow.TabIndex = 0;
            this.drawWindow.KeyDown += new System.Windows.Forms.KeyEventHandler(this.drawWindow_KeyDown);
            // 
            // ConversationEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.zoomBar);
            this.Controls.Add(this.hScrollBar1);
            this.Controls.Add(this.vScrollBar1);
            this.Controls.Add(this.drawWindow);
            this.Name = "ConversationEditorControl";
            this.Size = new System.Drawing.Size(351, 318);
            this.ResumeLayout(false);

        }

        #endregion

        private DrawWindow drawWindow;
        private ConversationEditor.GreyScrollBar vScrollBar1;
        private ConversationEditor.GreyScrollBar hScrollBar1;
        private GreyScrollBar zoomBar;
    }
}
