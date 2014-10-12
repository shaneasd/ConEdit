namespace PluginPack
{
    partial class FancyEnumForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.drawWindow1 = new Utilities.DrawWindow();
            this.greyScrollBar1 = new Utilities.GreyScrollBar();
            this.SuspendLayout();
            // 
            // drawWindow1
            // 
            this.drawWindow1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.drawWindow1.HandleNavigation = false;
            this.drawWindow1.Location = new System.Drawing.Point(0, 0);
            this.drawWindow1.Name = "drawWindow1";
            this.drawWindow1.Size = new System.Drawing.Size(277, 273);
            this.drawWindow1.TabIndex = 1;
            // 
            // greyScrollBar1
            // 
            this.greyScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
            this.greyScrollBar1.Horizontal = false;
            this.greyScrollBar1.Location = new System.Drawing.Point(277, 0);
            this.greyScrollBar1.Maximum = 0F;
            this.greyScrollBar1.Minimum = 0F;
            this.greyScrollBar1.MinimumSize = new System.Drawing.Size(15, 15);
            this.greyScrollBar1.Name = "greyScrollBar1";
            this.greyScrollBar1.PercentageCovered = 1F;
            this.greyScrollBar1.Size = new System.Drawing.Size(15, 273);
            this.greyScrollBar1.TabIndex = 0;
            this.greyScrollBar1.Value = 0F;
            // 
            // FancyEnumForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.drawWindow1);
            this.Controls.Add(this.greyScrollBar1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FancyEnumForm";
            this.Text = "FancyEnumForm";
            this.Leave += new System.EventHandler(this.FancyEnumForm_Leave);
            this.ResumeLayout(false);

        }

        #endregion

        private Utilities.GreyScrollBar greyScrollBar1;
        private Utilities.DrawWindow drawWindow1;
    }
}