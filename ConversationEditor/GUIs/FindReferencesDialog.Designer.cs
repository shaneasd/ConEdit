namespace ConversationEditor.GUIs
{
    partial class FindReferencesDialog
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
            this.errorList1 = new ConversationEditor.ErrorList();
            this.btnPrev = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // errorList1
            // 
            this.errorList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.errorList1.Location = new System.Drawing.Point(0, 0);
            this.errorList1.Name = "errorList1";
            this.errorList1.Size = new System.Drawing.Size(539, 317);
            this.errorList1.TabIndex = 0;
            // 
            // btnPrev
            // 
            this.btnPrev.Location = new System.Drawing.Point(4, 3);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(88, 40);
            this.btnPrev.TabIndex = 1;
            this.btnPrev.Text = "Previous";
            this.btnPrev.UseVisualStyleBackColor = true;
            this.btnPrev.Click += new System.EventHandler(this.btnPrev_Click);
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(98, 3);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(88, 40);
            this.btnNext.TabIndex = 2;
            this.btnNext.Text = "Next";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(192, 3);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(88, 40);
            this.button3.TabIndex = 3;
            this.button3.Text = "Close";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button3);
            this.panel1.Controls.Add(this.btnPrev);
            this.panel1.Controls.Add(this.btnNext);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 317);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(539, 47);
            this.panel1.TabIndex = 4;
            // 
            // FindReferencesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 364);
            this.Controls.Add(this.errorList1);
            this.Controls.Add(this.panel1);
            this.Name = "FindReferencesDialog";
            this.Text = "FindReferencesDialog";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ErrorList errorList1;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Panel panel1;

    }
}