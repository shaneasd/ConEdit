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
            this.chkCurrentConversationOnly = new System.Windows.Forms.CheckBox();
            this.txtFind = new System.Windows.Forms.TextBox();
            this.chkWholeWord = new System.Windows.Forms.CheckBox();
            this.chkMatchCase = new System.Windows.Forms.CheckBox();
            this.txtReplace = new System.Windows.Forms.TextBox();
            this.chkPreserveCase = new System.Windows.Forms.CheckBox();
            this.btnReplaceAll = new System.Windows.Forms.Button();
            this.btnReplace = new System.Windows.Forms.Button();
            this.btnFindNext = new System.Windows.Forms.Button();
            this.chkStrings = new System.Windows.Forms.CheckBox();
            this.chkLocalizedStrings = new System.Windows.Forms.CheckBox();
            this.chkDynamicEnumerations = new System.Windows.Forms.CheckBox();
            this.chkRegex = new System.Windows.Forms.CheckBox();
            this.btnFindAll = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chkCurrentConversationOnly
            // 
            this.chkCurrentConversationOnly.AutoSize = true;
            this.chkCurrentConversationOnly.Location = new System.Drawing.Point(12, 109);
            this.chkCurrentConversationOnly.Name = "chkCurrentConversationOnly";
            this.chkCurrentConversationOnly.Size = new System.Drawing.Size(149, 17);
            this.chkCurrentConversationOnly.TabIndex = 5;
            this.chkCurrentConversationOnly.Text = "Current Conversation Only";
            this.chkCurrentConversationOnly.UseVisualStyleBackColor = true;
            this.chkCurrentConversationOnly.CheckedChanged += new System.EventHandler(this.SettingChanged);
            // 
            // txtFind
            // 
            this.txtFind.Location = new System.Drawing.Point(12, 12);
            this.txtFind.Name = "txtFind";
            this.txtFind.Size = new System.Drawing.Size(314, 20);
            this.txtFind.TabIndex = 0;
            this.txtFind.TextChanged += new System.EventHandler(this.SettingChanged);
            // 
            // chkWholeWord
            // 
            this.chkWholeWord.AutoSize = true;
            this.chkWholeWord.Location = new System.Drawing.Point(12, 87);
            this.chkWholeWord.Name = "chkWholeWord";
            this.chkWholeWord.Size = new System.Drawing.Size(83, 17);
            this.chkWholeWord.TabIndex = 4;
            this.chkWholeWord.Text = "Whole word";
            this.chkWholeWord.UseVisualStyleBackColor = true;
            this.chkWholeWord.CheckedChanged += new System.EventHandler(this.SettingChanged);
            // 
            // chkMatchCase
            // 
            this.chkMatchCase.AutoSize = true;
            this.chkMatchCase.Location = new System.Drawing.Point(12, 64);
            this.chkMatchCase.Name = "chkMatchCase";
            this.chkMatchCase.Size = new System.Drawing.Size(83, 17);
            this.chkMatchCase.TabIndex = 3;
            this.chkMatchCase.Text = "Match Case";
            this.chkMatchCase.UseVisualStyleBackColor = true;
            this.chkMatchCase.CheckedChanged += new System.EventHandler(this.SettingChanged);
            // 
            // txtReplace
            // 
            this.txtReplace.Location = new System.Drawing.Point(12, 38);
            this.txtReplace.Name = "txtReplace";
            this.txtReplace.Size = new System.Drawing.Size(314, 20);
            this.txtReplace.TabIndex = 1;
            this.txtReplace.TextChanged += new System.EventHandler(this.SettingChanged);
            // 
            // chkPreserveCase
            // 
            this.chkPreserveCase.AutoSize = true;
            this.chkPreserveCase.Location = new System.Drawing.Point(332, 40);
            this.chkPreserveCase.Name = "chkPreserveCase";
            this.chkPreserveCase.Size = new System.Drawing.Size(95, 17);
            this.chkPreserveCase.TabIndex = 2;
            this.chkPreserveCase.Text = "Preserve Case";
            this.chkPreserveCase.UseVisualStyleBackColor = true;
            this.chkPreserveCase.CheckedChanged += new System.EventHandler(this.SettingChanged);
            // 
            // btnReplaceAll
            // 
            this.btnReplaceAll.Location = new System.Drawing.Point(315, 153);
            this.btnReplaceAll.Name = "btnReplaceAll";
            this.btnReplaceAll.Size = new System.Drawing.Size(95, 50);
            this.btnReplaceAll.TabIndex = 12;
            this.btnReplaceAll.Text = "Replace All";
            this.btnReplaceAll.UseVisualStyleBackColor = true;
            this.btnReplaceAll.Click += new System.EventHandler(this.ReplaceAllClicked);
            // 
            // btnReplace
            // 
            this.btnReplace.Location = new System.Drawing.Point(113, 154);
            this.btnReplace.Name = "btnReplace";
            this.btnReplace.Size = new System.Drawing.Size(95, 49);
            this.btnReplace.TabIndex = 11;
            this.btnReplace.Text = "Replace";
            this.btnReplace.UseVisualStyleBackColor = true;
            this.btnReplace.Click += new System.EventHandler(this.btnReplace_Click);
            // 
            // btnFindNext
            // 
            this.btnFindNext.Location = new System.Drawing.Point(12, 154);
            this.btnFindNext.Name = "btnFindNext";
            this.btnFindNext.Size = new System.Drawing.Size(95, 49);
            this.btnFindNext.TabIndex = 10;
            this.btnFindNext.Text = "Find Next";
            this.btnFindNext.UseVisualStyleBackColor = true;
            this.btnFindNext.Click += new System.EventHandler(this.btnFindNext_Click);
            // 
            // chkStrings
            // 
            this.chkStrings.AutoSize = true;
            this.chkStrings.Checked = true;
            this.chkStrings.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkStrings.Location = new System.Drawing.Point(189, 63);
            this.chkStrings.Name = "chkStrings";
            this.chkStrings.Size = new System.Drawing.Size(166, 17);
            this.chkStrings.TabIndex = 7;
            this.chkStrings.Text = "Search Non-Localized Strings";
            this.chkStrings.UseVisualStyleBackColor = true;
            this.chkStrings.CheckedChanged += new System.EventHandler(this.SettingChanged);
            // 
            // chkLocalizedStrings
            // 
            this.chkLocalizedStrings.AutoSize = true;
            this.chkLocalizedStrings.Checked = true;
            this.chkLocalizedStrings.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLocalizedStrings.Location = new System.Drawing.Point(189, 86);
            this.chkLocalizedStrings.Name = "chkLocalizedStrings";
            this.chkLocalizedStrings.Size = new System.Drawing.Size(143, 17);
            this.chkLocalizedStrings.TabIndex = 8;
            this.chkLocalizedStrings.Text = "Search Localized Strings";
            this.chkLocalizedStrings.UseVisualStyleBackColor = true;
            this.chkLocalizedStrings.CheckedChanged += new System.EventHandler(this.SettingChanged);
            // 
            // chkDynamicEnumerations
            // 
            this.chkDynamicEnumerations.AutoSize = true;
            this.chkDynamicEnumerations.Checked = true;
            this.chkDynamicEnumerations.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDynamicEnumerations.Location = new System.Drawing.Point(189, 109);
            this.chkDynamicEnumerations.Name = "chkDynamicEnumerations";
            this.chkDynamicEnumerations.Size = new System.Drawing.Size(171, 17);
            this.chkDynamicEnumerations.TabIndex = 9;
            this.chkDynamicEnumerations.Text = "Search Dynamic Enumerations";
            this.chkDynamicEnumerations.UseVisualStyleBackColor = true;
            this.chkDynamicEnumerations.CheckedChanged += new System.EventHandler(this.SettingChanged);
            // 
            // chkRegex
            // 
            this.chkRegex.AutoSize = true;
            this.chkRegex.Location = new System.Drawing.Point(12, 132);
            this.chkRegex.Name = "chkRegex";
            this.chkRegex.Size = new System.Drawing.Size(57, 17);
            this.chkRegex.TabIndex = 6;
            this.chkRegex.Text = "Regex";
            this.chkRegex.UseVisualStyleBackColor = true;
            this.chkRegex.CheckedChanged += new System.EventHandler(this.SettingChanged);
            // 
            // btnFindAll
            // 
            this.btnFindAll.Location = new System.Drawing.Point(214, 154);
            this.btnFindAll.Name = "btnFindAll";
            this.btnFindAll.Size = new System.Drawing.Size(95, 49);
            this.btnFindAll.TabIndex = 13;
            this.btnFindAll.Text = "Find All";
            this.btnFindAll.UseVisualStyleBackColor = true;
            this.btnFindAll.Click += new System.EventHandler(this.btnFindAll_Click);
            // 
            // FindAndReplaceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(428, 207);
            this.Controls.Add(this.btnFindAll);
            this.Controls.Add(this.chkRegex);
            this.Controls.Add(this.chkDynamicEnumerations);
            this.Controls.Add(this.chkLocalizedStrings);
            this.Controls.Add(this.chkStrings);
            this.Controls.Add(this.chkPreserveCase);
            this.Controls.Add(this.btnFindNext);
            this.Controls.Add(this.txtReplace);
            this.Controls.Add(this.btnReplace);
            this.Controls.Add(this.btnReplaceAll);
            this.Controls.Add(this.chkMatchCase);
            this.Controls.Add(this.txtFind);
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
        private System.Windows.Forms.Button btnReplace;
        private System.Windows.Forms.Button btnFindNext;
        private System.Windows.Forms.CheckBox chkStrings;
        private System.Windows.Forms.CheckBox chkLocalizedStrings;
        private System.Windows.Forms.CheckBox chkDynamicEnumerations;
        private System.Windows.Forms.CheckBox chkRegex;
        private System.Windows.Forms.Button btnFindAll;
    }
}