namespace Utilities.UI
{
    partial class ScrollPanel
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
            this.greyScrollBar1 = new Utilities.GreyScrollBar();
            this.SuspendLayout();
            // 
            // greyScrollBar1
            // 
            this.greyScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
            this.greyScrollBar1.Horizontal = false;
            this.greyScrollBar1.Location = new System.Drawing.Point(648, 0);
            this.greyScrollBar1.Maximum = 0F;
            this.greyScrollBar1.Minimum = 0F;
            this.greyScrollBar1.MinimumSize = new System.Drawing.Size(15, 15);
            this.greyScrollBar1.Name = "greyScrollBar1";
            this.greyScrollBar1.PercentageCovered = 1F;
            this.greyScrollBar1.Size = new System.Drawing.Size(15, 355);
            this.greyScrollBar1.TabIndex = 0;
            this.greyScrollBar1.Value = 0F;
            // 
            // ScrollPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.greyScrollBar1);
            this.Name = "ScrollPanel";
            this.Size = new System.Drawing.Size(663, 355);
            this.ResumeLayout(false);

        }

        #endregion

        private GreyScrollBar greyScrollBar1;
    }
}
