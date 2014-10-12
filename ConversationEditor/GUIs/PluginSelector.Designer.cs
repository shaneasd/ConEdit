namespace ConversationEditor
{
    partial class PluginSelector
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
            this.button1 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.drawWindow1 = new Utilities.DrawWindow();
            this.greyScrollBar1 = new Utilities.GreyScrollBar();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(336, 407);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(77, 28);
            this.button1.TabIndex = 1;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "dll";
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.InitialDirectory = ".";
            this.openFileDialog1.Multiselect = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 398);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(425, 42);
            this.panel1.TabIndex = 4;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(253, 9);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(77, 28);
            this.button2.TabIndex = 6;
            this.button2.Text = "Ok";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // drawWindow1
            // 
            this.drawWindow1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(56)))), ((int)(((byte)(56)))));
            this.drawWindow1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.drawWindow1.HandleNavigation = false;
            this.drawWindow1.Location = new System.Drawing.Point(0, 0);
            this.drawWindow1.Name = "drawWindow1";
            this.drawWindow1.Size = new System.Drawing.Size(410, 398);
            this.drawWindow1.TabIndex = 6;
            this.drawWindow1.Paint += new System.Windows.Forms.PaintEventHandler(this.drawWindow1_Paint);
            this.drawWindow1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.drawWindow1_MouseClick);
            // 
            // greyScrollBar1
            // 
            this.greyScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
            this.greyScrollBar1.Horizontal = false;
            this.greyScrollBar1.Location = new System.Drawing.Point(410, 0);
            this.greyScrollBar1.Maximum = 10F;
            this.greyScrollBar1.Minimum = 0F;
            this.greyScrollBar1.MinimumSize = new System.Drawing.Size(15, 15);
            this.greyScrollBar1.Name = "greyScrollBar1";
            this.greyScrollBar1.PercentageCovered = 0F;
            this.greyScrollBar1.Size = new System.Drawing.Size(15, 398);
            this.greyScrollBar1.SmallChange = 0.1F;
            this.greyScrollBar1.TabIndex = 7;
            this.greyScrollBar1.Value = 0F;
            // 
            // PluginSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(425, 440);
            this.Controls.Add(this.drawWindow1);
            this.Controls.Add(this.greyScrollBar1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.panel1);
            this.Name = "PluginSelector";
            this.Text = "PluginSelector";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button2;
        private Utilities.DrawWindow drawWindow1;
        private Utilities.GreyScrollBar greyScrollBar1;
    }
}