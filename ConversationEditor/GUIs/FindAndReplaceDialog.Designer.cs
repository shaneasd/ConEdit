namespace ConversationEditor
{
    partial class FindAndReplaceDialog
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
            this.radNonLocalizedTextOnly = new System.Windows.Forms.RadioButton();
            this.radBoth = new System.Windows.Forms.RadioButton();
            this.radLocalizedTextOnly = new System.Windows.Forms.RadioButton();
            this.chkCurrentConversationOnly = new System.Windows.Forms.CheckBox();
            this.txtFind = new System.Windows.Forms.TextBox();
            this.chkWholeWord = new System.Windows.Forms.CheckBox();
            this.chkMatchCase = new System.Windows.Forms.CheckBox();
            this.txtReplace = new System.Windows.Forms.TextBox();
            this.chkPreserveCase = new System.Windows.Forms.CheckBox();
            this.btnReplaceAll = new System.Windows.Forms.Button();
            this.btnReplace = new System.Windows.Forms.Button();
            this.btnFindNext = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // radNonLocalizedTextOnly
            // 
            this.radNonLocalizedTextOnly.AutoSize = true;
            this.radNonLocalizedTextOnly.Location = new System.Drawing.Point(189, 86);
            this.radNonLocalizedTextOnly.Name = "radNonLocalizedTextOnly";
            this.radNonLocalizedTextOnly.Size = new System.Drawing.Size(137, 17);
            this.radNonLocalizedTextOnly.TabIndex = 6;
            this.radNonLocalizedTextOnly.TabStop = true;
            this.radNonLocalizedTextOnly.Text = "Non-localized Text Only";
            this.radNonLocalizedTextOnly.UseVisualStyleBackColor = true;
            // 
            // radBoth
            // 
            this.radBoth.AutoSize = true;
            this.radBoth.Location = new System.Drawing.Point(189, 109);
            this.radBoth.Name = "radBoth";
            this.radBoth.Size = new System.Drawing.Size(47, 17);
            this.radBoth.TabIndex = 5;
            this.radBoth.TabStop = true;
            this.radBoth.Text = "Both";
            this.radBoth.UseVisualStyleBackColor = true;
            // 
            // radLocalizedTextOnly
            // 
            this.radLocalizedTextOnly.AutoSize = true;
            this.radLocalizedTextOnly.Location = new System.Drawing.Point(189, 64);
            this.radLocalizedTextOnly.Name = "radLocalizedTextOnly";
            this.radLocalizedTextOnly.Size = new System.Drawing.Size(118, 17);
            this.radLocalizedTextOnly.TabIndex = 4;
            this.radLocalizedTextOnly.TabStop = true;
            this.radLocalizedTextOnly.Text = "Localized Text Only";
            this.radLocalizedTextOnly.UseVisualStyleBackColor = true;
            // 
            // chkCurrentConversationOnly
            // 
            this.chkCurrentConversationOnly.AutoSize = true;
            this.chkCurrentConversationOnly.Location = new System.Drawing.Point(12, 109);
            this.chkCurrentConversationOnly.Name = "chkCurrentConversationOnly";
            this.chkCurrentConversationOnly.Size = new System.Drawing.Size(149, 17);
            this.chkCurrentConversationOnly.TabIndex = 3;
            this.chkCurrentConversationOnly.Text = "Current Conversation Only";
            this.chkCurrentConversationOnly.UseVisualStyleBackColor = true;
            // 
            // txtFind
            // 
            this.txtFind.Location = new System.Drawing.Point(12, 12);
            this.txtFind.Name = "txtFind";
            this.txtFind.Size = new System.Drawing.Size(314, 20);
            this.txtFind.TabIndex = 2;
            // 
            // chkWholeWord
            // 
            this.chkWholeWord.AutoSize = true;
            this.chkWholeWord.Location = new System.Drawing.Point(12, 87);
            this.chkWholeWord.Name = "chkWholeWord";
            this.chkWholeWord.Size = new System.Drawing.Size(83, 17);
            this.chkWholeWord.TabIndex = 1;
            this.chkWholeWord.Text = "Whole word";
            this.chkWholeWord.UseVisualStyleBackColor = true;
            // 
            // chkMatchCase
            // 
            this.chkMatchCase.AutoSize = true;
            this.chkMatchCase.Location = new System.Drawing.Point(12, 64);
            this.chkMatchCase.Name = "chkMatchCase";
            this.chkMatchCase.Size = new System.Drawing.Size(83, 17);
            this.chkMatchCase.TabIndex = 0;
            this.chkMatchCase.Text = "Match Case";
            this.chkMatchCase.UseVisualStyleBackColor = true;
            // 
            // txtReplace
            // 
            this.txtReplace.Location = new System.Drawing.Point(12, 38);
            this.txtReplace.Name = "txtReplace";
            this.txtReplace.Size = new System.Drawing.Size(314, 20);
            this.txtReplace.TabIndex = 1;
            // 
            // chkPreserveCase
            // 
            this.chkPreserveCase.AutoSize = true;
            this.chkPreserveCase.Location = new System.Drawing.Point(332, 40);
            this.chkPreserveCase.Name = "chkPreserveCase";
            this.chkPreserveCase.Size = new System.Drawing.Size(95, 17);
            this.chkPreserveCase.TabIndex = 0;
            this.chkPreserveCase.Text = "Preserve Case";
            this.chkPreserveCase.UseVisualStyleBackColor = true;
            // 
            // btnReplaceAll
            // 
            this.btnReplaceAll.Location = new System.Drawing.Point(250, 132);
            this.btnReplaceAll.Name = "btnReplaceAll";
            this.btnReplaceAll.Size = new System.Drawing.Size(113, 50);
            this.btnReplaceAll.TabIndex = 3;
            this.btnReplaceAll.Text = "Replace All";
            this.btnReplaceAll.UseVisualStyleBackColor = true;
            this.btnReplaceAll.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnReplace
            // 
            this.btnReplace.Location = new System.Drawing.Point(131, 132);
            this.btnReplace.Name = "btnReplace";
            this.btnReplace.Size = new System.Drawing.Size(113, 49);
            this.btnReplace.TabIndex = 5;
            this.btnReplace.Text = "Replace";
            this.btnReplace.UseVisualStyleBackColor = true;
            this.btnReplace.Click += new System.EventHandler(this.btnReplace_Click);
            // 
            // btnFindNext
            // 
            this.btnFindNext.Location = new System.Drawing.Point(12, 132);
            this.btnFindNext.Name = "btnFindNext";
            this.btnFindNext.Size = new System.Drawing.Size(113, 49);
            this.btnFindNext.TabIndex = 6;
            this.btnFindNext.Text = "Find Next";
            this.btnFindNext.UseVisualStyleBackColor = true;
            this.btnFindNext.Click += new System.EventHandler(this.btnFindNext_Click);
            // 
            // FindAndReplaceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 191);
            this.Controls.Add(this.chkPreserveCase);
            this.Controls.Add(this.btnFindNext);
            this.Controls.Add(this.txtReplace);
            this.Controls.Add(this.btnReplace);
            this.Controls.Add(this.radNonLocalizedTextOnly);
            this.Controls.Add(this.btnReplaceAll);
            this.Controls.Add(this.radBoth);
            this.Controls.Add(this.chkMatchCase);
            this.Controls.Add(this.txtFind);
            this.Controls.Add(this.radLocalizedTextOnly);
            this.Controls.Add(this.chkWholeWord);
            this.Controls.Add(this.chkCurrentConversationOnly);
            this.Name = "FindAndReplaceDialog";
            this.Text = "Find and Replace";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkWholeWord;
        private System.Windows.Forms.CheckBox chkMatchCase;
        private System.Windows.Forms.CheckBox chkPreserveCase;
        private System.Windows.Forms.TextBox txtFind;
        private System.Windows.Forms.TextBox txtReplace;
        private System.Windows.Forms.Button btnReplaceAll;
        private System.Windows.Forms.CheckBox chkCurrentConversationOnly;
        private System.Windows.Forms.RadioButton radNonLocalizedTextOnly;
        private System.Windows.Forms.RadioButton radBoth;
        private System.Windows.Forms.RadioButton radLocalizedTextOnly;
        private System.Windows.Forms.Button btnReplace;
        private System.Windows.Forms.Button btnFindNext;

    }
}