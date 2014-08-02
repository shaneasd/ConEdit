namespace ConversationEditor
{
    partial class SpielEdit
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
            this.subtitle = new System.Windows.Forms.TextBox();
            this.Summary = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // subtitle
            // 
            this.subtitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.subtitle.Location = new System.Drawing.Point(0, 46);
            this.subtitle.Multiline = true;
            this.subtitle.Name = "subtitle";
            this.subtitle.Size = new System.Drawing.Size(292, 152);
            this.subtitle.TabIndex = 0;
            // 
            // Summary
            // 
            this.Summary.Dock = System.Windows.Forms.DockStyle.Top;
            this.Summary.Location = new System.Drawing.Point(0, 13);
            this.Summary.Name = "Summary";
            this.Summary.Size = new System.Drawing.Size(292, 20);
            this.Summary.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(292, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Summary";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(292, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Subtitle";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SpielEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.subtitle);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Summary);
            this.Controls.Add(this.label1);
            this.Name = "SpielEdit";
            this.Text = "SpielEdit";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SpielEdit_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox subtitle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox Summary;
    }
}