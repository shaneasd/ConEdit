﻿namespace ConversationEditor
{
    partial class DefaultSetEditor
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
            this.drawWindow1 = new Utilities.UI.DrawWindow();
            this.SuspendLayout();
            // 
            // drawWindow1
            // 
            this.drawWindow1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.drawWindow1.HandleNavigation = false;
            this.drawWindow1.Location = new System.Drawing.Point(0, 0);
            this.drawWindow1.Margin = new System.Windows.Forms.Padding(0);
            this.drawWindow1.Name = "drawWindow1";
            this.drawWindow1.Size = new System.Drawing.Size(150, 78);
            this.drawWindow1.TabIndex = 1;
            // 
            // DefaultSetEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.drawWindow1);
            this.Name = "DefaultSetEditor";
            this.Size = new System.Drawing.Size(150, 78);
            this.ResumeLayout(false);

        }

        #endregion

        private Utilities.UI.DrawWindow drawWindow1;
    }
}
