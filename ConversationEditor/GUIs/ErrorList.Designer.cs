﻿namespace ConversationEditor
{
    partial class ErrorList
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
            Utilities.UI.DrawWindow.DefaultColorScheme defaultColorScheme1 = new Utilities.UI.DrawWindow.DefaultColorScheme();
            this.drawWindow1 = new Utilities.UI.DrawWindow();
            this.greyScrollBar1 = new Utilities.UI.GreyScrollBar();
            this.SuspendLayout();
            // 
            // drawWindow1
            // 
            this.drawWindow1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.drawWindow1.BackColor = System.Drawing.Color.Transparent;
            this.drawWindow1.ColorScheme = defaultColorScheme1;
            this.drawWindow1.HandleNavigation = false;
            this.drawWindow1.Location = new System.Drawing.Point(0, 0);
            this.drawWindow1.Name = "drawWindow1";
            this.drawWindow1.Size = new System.Drawing.Size(933, 146);
            this.drawWindow1.TabIndex = 1;
            // 
            // greyScrollBar1
            // 
            this.greyScrollBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.greyScrollBar1.Horizontal = false;
            this.greyScrollBar1.LargeChange = 0.05F;
            this.greyScrollBar1.Location = new System.Drawing.Point(935, 0);
            this.greyScrollBar1.Maximum = 0F;
            this.greyScrollBar1.Minimum = 0F;
            this.greyScrollBar1.MinimumSize = new System.Drawing.Size(15, 15);
            this.greyScrollBar1.Name = "greyScrollBar1";
            this.greyScrollBar1.PercentageCovered = 0F;
            this.greyScrollBar1.Size = new System.Drawing.Size(15, 146);
            this.greyScrollBar1.SmallChange = 0.01F;
            this.greyScrollBar1.TabIndex = 0;
            this.greyScrollBar1.Value = 0F;
            // 
            // ErrorList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.drawWindow1);
            this.Controls.Add(this.greyScrollBar1);
            this.Name = "ErrorList";
            this.Size = new System.Drawing.Size(951, 150);
            this.ResumeLayout(false);

        }

        #endregion

        private Utilities.UI.GreyScrollBar greyScrollBar1;
        private Utilities.UI.DrawWindow drawWindow1;
    }
}
