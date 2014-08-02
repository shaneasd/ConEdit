namespace ConversationEditor.GUIs.TypeEditors
{
    partial class EnumerationTypeEditor
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
            this.backwardTab = new Utilities.InvisibleControl();
            this.forwardTab = new Utilities.InvisibleControl();
            this.drawWindow1 = new ConversationEditor.DrawWindow();
            this.SuspendLayout();
            // 
            // backwardTab
            // 
            this.backwardTab.Location = new System.Drawing.Point(94, 27);
            this.backwardTab.Name = "backwardTab";
            this.backwardTab.Size = new System.Drawing.Size(10, 10);
            this.backwardTab.TabIndex = 3;
            // 
            // forwardTab
            // 
            this.forwardTab.Location = new System.Drawing.Point(119, 15);
            this.forwardTab.Name = "forwardTab";
            this.forwardTab.Size = new System.Drawing.Size(10, 10);
            this.forwardTab.TabIndex = 1;
            // 
            // drawWindow1
            // 
            this.drawWindow1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(56)))), ((int)(((byte)(56)))));
            this.drawWindow1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.drawWindow1.HandleNavigation = false;
            this.drawWindow1.Location = new System.Drawing.Point(0, 0);
            this.drawWindow1.Name = "drawWindow1";
            this.drawWindow1.Size = new System.Drawing.Size(405, 54);
            this.drawWindow1.TabIndex = 2;
            // 
            // EnumerationTypeEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.backwardTab);
            this.Controls.Add(this.forwardTab);
            this.Controls.Add(this.drawWindow1);
            this.Name = "EnumerationTypeEditor";
            this.Size = new System.Drawing.Size(405, 54);
            this.ResumeLayout(false);

        }

        #endregion

        private DrawWindow drawWindow1;
        private Utilities.InvisibleControl forwardTab;
        private Utilities.InvisibleControl backwardTab;
    }
}
