namespace ConversationEditor
{
    partial class DomainEditor
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
            this.btnAddDecimal = new System.Windows.Forms.Button();
            this.btnAddEnumeration = new System.Windows.Forms.Button();
            this.btnAddDynamicEnum = new System.Windows.Forms.Button();
            this.btnAddInteger = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.innerPanel = new System.Windows.Forms.Panel();
            this.greyScrollBar1 = new ConversationEditor.GreyScrollBar();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnAddDecimal
            // 
            this.btnAddDecimal.AutoSize = true;
            this.btnAddDecimal.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddDecimal.Location = new System.Drawing.Point(255, 3);
            this.btnAddDecimal.Name = "btnAddDecimal";
            this.btnAddDecimal.Size = new System.Drawing.Size(77, 23);
            this.btnAddDecimal.TabIndex = 4;
            this.btnAddDecimal.Text = "Add Decimal";
            this.btnAddDecimal.UseVisualStyleBackColor = true;
            // 
            // btnAddEnumeration
            // 
            this.btnAddEnumeration.AutoSize = true;
            this.btnAddEnumeration.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddEnumeration.Location = new System.Drawing.Point(151, 3);
            this.btnAddEnumeration.Name = "btnAddEnumeration";
            this.btnAddEnumeration.Size = new System.Drawing.Size(98, 23);
            this.btnAddEnumeration.TabIndex = 3;
            this.btnAddEnumeration.Text = "Add Enumeration";
            this.btnAddEnumeration.UseVisualStyleBackColor = true;
            // 
            // btnAddDynamicEnum
            // 
            this.btnAddDynamicEnum.AutoSize = true;
            this.btnAddDynamicEnum.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddDynamicEnum.Location = new System.Drawing.Point(3, 3);
            this.btnAddDynamicEnum.Name = "btnAddDynamicEnum";
            this.btnAddDynamicEnum.Size = new System.Drawing.Size(142, 23);
            this.btnAddDynamicEnum.TabIndex = 2;
            this.btnAddDynamicEnum.Text = "Add Dynamic Enumeration";
            this.btnAddDynamicEnum.UseVisualStyleBackColor = true;
            this.btnAddDynamicEnum.Click += new System.EventHandler(this.btnAddDynamicEnum_Click);
            // 
            // btnAddInteger
            // 
            this.btnAddInteger.AutoSize = true;
            this.btnAddInteger.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddInteger.Location = new System.Drawing.Point(338, 3);
            this.btnAddInteger.Name = "btnAddInteger";
            this.btnAddInteger.Size = new System.Drawing.Size(72, 23);
            this.btnAddInteger.TabIndex = 5;
            this.btnAddInteger.Text = "Add Integer";
            this.btnAddInteger.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.btnAddDynamicEnum);
            this.flowLayoutPanel1.Controls.Add(this.btnAddEnumeration);
            this.flowLayoutPanel1.Controls.Add(this.btnAddDecimal);
            this.flowLayoutPanel1.Controls.Add(this.btnAddInteger);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 382);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(634, 29);
            this.flowLayoutPanel1.TabIndex = 8;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.innerPanel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(1);
            this.panel1.Size = new System.Drawing.Size(619, 382);
            this.panel1.TabIndex = 9;
            // 
            // innerPanel
            // 
            this.innerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.innerPanel.Location = new System.Drawing.Point(1, 1);
            this.innerPanel.Name = "innerPanel";
            this.innerPanel.Padding = new System.Windows.Forms.Padding(2);
            this.innerPanel.Size = new System.Drawing.Size(617, 380);
            this.innerPanel.TabIndex = 10;
            // 
            // greyScrollBar1
            // 
            this.greyScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
            this.greyScrollBar1.Horizontal = false;
            this.greyScrollBar1.Location = new System.Drawing.Point(619, 0);
            this.greyScrollBar1.Maximum = 0F;
            this.greyScrollBar1.Minimum = 0F;
            this.greyScrollBar1.MinimumSize = new System.Drawing.Size(15, 15);
            this.greyScrollBar1.Name = "greyScrollBar1";
            this.greyScrollBar1.PercentageCovered = 0F;
            this.greyScrollBar1.Size = new System.Drawing.Size(15, 382);
            this.greyScrollBar1.TabIndex = 6;
            this.greyScrollBar1.Value = 0F;
            this.greyScrollBar1.Scrolled += new System.Action(this.greyScrollBar1_Scrolled);
            // 
            // DomainEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.greyScrollBar1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "DomainEditor";
            this.Size = new System.Drawing.Size(634, 411);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnAddDecimal;
        private System.Windows.Forms.Button btnAddEnumeration;
        private System.Windows.Forms.Button btnAddDynamicEnum;
        private System.Windows.Forms.Button btnAddInteger;
        private GreyScrollBar greyScrollBar1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel innerPanel;
    }
}
