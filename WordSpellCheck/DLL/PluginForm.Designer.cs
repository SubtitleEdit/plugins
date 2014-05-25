namespace Nikse.SubtitleEdit.PluginLogic
{
    partial class PluginForm
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
            this.components = new System.ComponentModel.Container();
            this.buttonOK = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.labelDescription = new System.Windows.Forms.Label();
            this.listViewSubtitle = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.startSpellCheckFromCurrentLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelActionInfo = new System.Windows.Forms.Label();
            this.comboBoxDictionaries = new System.Windows.Forms.ComboBox();
            this.groupBoxSuggestions = new System.Windows.Forms.GroupBox();
            this.buttonUseSuggestion = new System.Windows.Forms.Button();
            this.buttonUseSuggestionAlways = new System.Windows.Forms.Button();
            this.listBoxSuggestions = new System.Windows.Forms.ListBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.groupBoxWordNotFound = new System.Windows.Forms.GroupBox();
            this.buttonAddToNamesEtcList = new System.Windows.Forms.Button();
            this.buttonGoogleIt = new System.Windows.Forms.Button();
            this.checkBoxUseNamesEtc = new System.Windows.Forms.CheckBox();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonAddToDictionary = new System.Windows.Forms.Button();
            this.buttonSkipOnce = new System.Windows.Forms.Button();
            this.buttonChangeAll = new System.Windows.Forms.Button();
            this.textBoxWord = new System.Windows.Forms.TextBox();
            this.buttonSkipAll = new System.Windows.Forms.Button();
            this.buttonChange = new System.Windows.Forms.Button();
            this.richTextBoxParagraph = new System.Windows.Forms.RichTextBox();
            this.buttonEditWholeText = new System.Windows.Forms.Button();
            this.labelFullText = new System.Windows.Forms.Label();
            this.buttonEditWord = new System.Windows.Forms.Button();
            this.textBoxWholeText = new System.Windows.Forms.TextBox();
            this.buttonUpdateWholeText = new System.Windows.Forms.Button();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBoxSuggestions.SuspendLayout();
            this.groupBoxWordNotFound.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(645, 525);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 8;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(726, 525);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "C&ancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(13, 13);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(82, 13);
            this.labelDescription.TabIndex = 2;
            this.labelDescription.Text = "labelDescription";
            // 
            // listViewSubtitle
            // 
            this.listViewSubtitle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewSubtitle.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8});
            this.listViewSubtitle.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewSubtitle.FullRowSelect = true;
            this.listViewSubtitle.HideSelection = false;
            this.listViewSubtitle.Location = new System.Drawing.Point(12, 29);
            this.listViewSubtitle.MultiSelect = false;
            this.listViewSubtitle.Name = "listViewSubtitle";
            this.listViewSubtitle.Size = new System.Drawing.Size(789, 195);
            this.listViewSubtitle.TabIndex = 0;
            this.listViewSubtitle.UseCompatibleStateImageBehavior = false;
            this.listViewSubtitle.View = System.Windows.Forms.View.Details;
            this.listViewSubtitle.SelectedIndexChanged += new System.EventHandler(this.listViewSubtitle_SelectedIndexChanged);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "#";
            this.columnHeader4.Width = 50;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Start";
            this.columnHeader5.Width = 100;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "End";
            this.columnHeader6.Width = 100;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Duration";
            this.columnHeader7.Width = 80;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Text";
            this.columnHeader8.Width = 393;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startSpellCheckFromCurrentLineToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(252, 26);
            // 
            // startSpellCheckFromCurrentLineToolStripMenuItem
            // 
            this.startSpellCheckFromCurrentLineToolStripMenuItem.Name = "startSpellCheckFromCurrentLineToolStripMenuItem";
            this.startSpellCheckFromCurrentLineToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.startSpellCheckFromCurrentLineToolStripMenuItem.Text = "Start spell check from current line";
            this.startSpellCheckFromCurrentLineToolStripMenuItem.Click += new System.EventHandler(this.startSpellCheckFromCurrentLineToolStripMenuItem_Click);
            // 
            // labelActionInfo
            // 
            this.labelActionInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelActionInfo.AutoSize = true;
            this.labelActionInfo.Location = new System.Drawing.Point(12, 535);
            this.labelActionInfo.Name = "labelActionInfo";
            this.labelActionInfo.Size = new System.Drawing.Size(77, 13);
            this.labelActionInfo.TabIndex = 105;
            this.labelActionInfo.Text = "labelActionInfo";
            // 
            // comboBoxDictionaries
            // 
            this.comboBoxDictionaries.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxDictionaries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDictionaries.FormattingEnabled = true;
            this.comboBoxDictionaries.Location = new System.Drawing.Point(564, 5);
            this.comboBoxDictionaries.Name = "comboBoxDictionaries";
            this.comboBoxDictionaries.Size = new System.Drawing.Size(237, 21);
            this.comboBoxDictionaries.TabIndex = 11;
            this.comboBoxDictionaries.Visible = false;
            this.comboBoxDictionaries.SelectedIndexChanged += new System.EventHandler(this.comboBoxDictionaries_SelectedIndexChanged);
            // 
            // groupBoxSuggestions
            // 
            this.groupBoxSuggestions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxSuggestions.Controls.Add(this.buttonUseSuggestion);
            this.groupBoxSuggestions.Controls.Add(this.buttonUseSuggestionAlways);
            this.groupBoxSuggestions.Controls.Add(this.listBoxSuggestions);
            this.groupBoxSuggestions.Location = new System.Drawing.Point(314, 323);
            this.groupBoxSuggestions.Name = "groupBoxSuggestions";
            this.groupBoxSuggestions.Size = new System.Drawing.Size(291, 209);
            this.groupBoxSuggestions.TabIndex = 7;
            this.groupBoxSuggestions.TabStop = false;
            this.groupBoxSuggestions.Text = "Suggestions";
            // 
            // buttonUseSuggestion
            // 
            this.buttonUseSuggestion.Location = new System.Drawing.Point(93, 17);
            this.buttonUseSuggestion.Name = "buttonUseSuggestion";
            this.buttonUseSuggestion.Size = new System.Drawing.Size(90, 21);
            this.buttonUseSuggestion.TabIndex = 0;
            this.buttonUseSuggestion.Text = "Use";
            this.buttonUseSuggestion.UseVisualStyleBackColor = true;
            this.buttonUseSuggestion.Click += new System.EventHandler(this.buttonUseSuggestion_Click);
            // 
            // buttonUseSuggestionAlways
            // 
            this.buttonUseSuggestionAlways.Location = new System.Drawing.Point(189, 17);
            this.buttonUseSuggestionAlways.Name = "buttonUseSuggestionAlways";
            this.buttonUseSuggestionAlways.Size = new System.Drawing.Size(90, 21);
            this.buttonUseSuggestionAlways.TabIndex = 1;
            this.buttonUseSuggestionAlways.Text = "Use always";
            this.buttonUseSuggestionAlways.UseVisualStyleBackColor = true;
            this.buttonUseSuggestionAlways.Click += new System.EventHandler(this.buttonUseSuggestionAlways_Click);
            // 
            // listBoxSuggestions
            // 
            this.listBoxSuggestions.FormattingEnabled = true;
            this.listBoxSuggestions.Location = new System.Drawing.Point(8, 44);
            this.listBoxSuggestions.Name = "listBoxSuggestions";
            this.listBoxSuggestions.Size = new System.Drawing.Size(272, 160);
            this.listBoxSuggestions.TabIndex = 2;
            this.listBoxSuggestions.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBoxSuggestions_MouseDoubleClick);
            // 
            // labelLanguage
            // 
            this.labelLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(503, 9);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(55, 13);
            this.labelLanguage.TabIndex = 10;
            this.labelLanguage.Text = "Language";
            this.labelLanguage.Visible = false;
            // 
            // groupBoxWordNotFound
            // 
            this.groupBoxWordNotFound.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxWordNotFound.Controls.Add(this.buttonAddToNamesEtcList);
            this.groupBoxWordNotFound.Controls.Add(this.buttonGoogleIt);
            this.groupBoxWordNotFound.Controls.Add(this.checkBoxUseNamesEtc);
            this.groupBoxWordNotFound.Controls.Add(this.buttonDelete);
            this.groupBoxWordNotFound.Controls.Add(this.buttonAddToDictionary);
            this.groupBoxWordNotFound.Controls.Add(this.buttonSkipOnce);
            this.groupBoxWordNotFound.Controls.Add(this.buttonChangeAll);
            this.groupBoxWordNotFound.Controls.Add(this.textBoxWord);
            this.groupBoxWordNotFound.Controls.Add(this.buttonSkipAll);
            this.groupBoxWordNotFound.Controls.Add(this.buttonChange);
            this.groupBoxWordNotFound.Location = new System.Drawing.Point(12, 323);
            this.groupBoxWordNotFound.Name = "groupBoxWordNotFound";
            this.groupBoxWordNotFound.Size = new System.Drawing.Size(292, 209);
            this.groupBoxWordNotFound.TabIndex = 6;
            this.groupBoxWordNotFound.TabStop = false;
            this.groupBoxWordNotFound.Text = "Spelling/grammar error";
            // 
            // buttonAddToNamesEtcList
            // 
            this.buttonAddToNamesEtcList.Location = new System.Drawing.Point(21, 156);
            this.buttonAddToNamesEtcList.Name = "buttonAddToNamesEtcList";
            this.buttonAddToNamesEtcList.Size = new System.Drawing.Size(250, 21);
            this.buttonAddToNamesEtcList.TabIndex = 8;
            this.buttonAddToNamesEtcList.Text = "Add to names/noise list (case sensitive)";
            this.buttonAddToNamesEtcList.UseVisualStyleBackColor = true;
            this.buttonAddToNamesEtcList.Visible = false;
            this.buttonAddToNamesEtcList.Click += new System.EventHandler(this.buttonAddToNamesEtcList_Click);
            // 
            // buttonGoogleIt
            // 
            this.buttonGoogleIt.Location = new System.Drawing.Point(149, 102);
            this.buttonGoogleIt.Name = "buttonGoogleIt";
            this.buttonGoogleIt.Size = new System.Drawing.Size(122, 21);
            this.buttonGoogleIt.TabIndex = 6;
            this.buttonGoogleIt.Text = "&Google it";
            this.buttonGoogleIt.UseVisualStyleBackColor = true;
            this.buttonGoogleIt.Click += new System.EventHandler(this.buttonGoogleIt_Click);
            // 
            // checkBoxUseNamesEtc
            // 
            this.checkBoxUseNamesEtc.AutoSize = true;
            this.checkBoxUseNamesEtc.Checked = true;
            this.checkBoxUseNamesEtc.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxUseNamesEtc.Location = new System.Drawing.Point(21, 183);
            this.checkBoxUseNamesEtc.Name = "checkBoxUseNamesEtc";
            this.checkBoxUseNamesEtc.Size = new System.Drawing.Size(164, 17);
            this.checkBoxUseNamesEtc.TabIndex = 9;
            this.checkBoxUseNamesEtc.Text = "Use names/noise list from SE";
            this.checkBoxUseNamesEtc.UseVisualStyleBackColor = true;
            // 
            // buttonDelete
            // 
            this.buttonDelete.Location = new System.Drawing.Point(21, 102);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(122, 21);
            this.buttonDelete.TabIndex = 5;
            this.buttonDelete.Text = "&Delete word";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonAddToDictionary
            // 
            this.buttonAddToDictionary.Location = new System.Drawing.Point(21, 129);
            this.buttonAddToDictionary.Name = "buttonAddToDictionary";
            this.buttonAddToDictionary.Size = new System.Drawing.Size(250, 21);
            this.buttonAddToDictionary.TabIndex = 7;
            this.buttonAddToDictionary.Text = "Add to user dictionary (not case sensitive)";
            this.buttonAddToDictionary.UseVisualStyleBackColor = true;
            this.buttonAddToDictionary.Click += new System.EventHandler(this.buttonAddToDictionary_Click);
            // 
            // buttonSkipOnce
            // 
            this.buttonSkipOnce.Location = new System.Drawing.Point(21, 75);
            this.buttonSkipOnce.Name = "buttonSkipOnce";
            this.buttonSkipOnce.Size = new System.Drawing.Size(122, 21);
            this.buttonSkipOnce.TabIndex = 3;
            this.buttonSkipOnce.Text = "Skip &once";
            this.buttonSkipOnce.UseVisualStyleBackColor = true;
            this.buttonSkipOnce.Click += new System.EventHandler(this.buttonSkipOnce_Click);
            // 
            // buttonChangeAll
            // 
            this.buttonChangeAll.Location = new System.Drawing.Point(150, 47);
            this.buttonChangeAll.Name = "buttonChangeAll";
            this.buttonChangeAll.Size = new System.Drawing.Size(122, 21);
            this.buttonChangeAll.TabIndex = 2;
            this.buttonChangeAll.Text = "Change all";
            this.buttonChangeAll.UseVisualStyleBackColor = true;
            this.buttonChangeAll.Click += new System.EventHandler(this.buttonChangeAll_Click);
            // 
            // textBoxWord
            // 
            this.textBoxWord.Location = new System.Drawing.Point(21, 20);
            this.textBoxWord.Name = "textBoxWord";
            this.textBoxWord.Size = new System.Drawing.Size(250, 20);
            this.textBoxWord.TabIndex = 0;
            this.textBoxWord.TextChanged += new System.EventHandler(this.textBoxWord_TextChanged);
            this.textBoxWord.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxWord_KeyDown);
            this.textBoxWord.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxWord_KeyUp);
            // 
            // buttonSkipAll
            // 
            this.buttonSkipAll.Location = new System.Drawing.Point(149, 75);
            this.buttonSkipAll.Name = "buttonSkipAll";
            this.buttonSkipAll.Size = new System.Drawing.Size(122, 21);
            this.buttonSkipAll.TabIndex = 4;
            this.buttonSkipAll.Text = "&Skip all";
            this.buttonSkipAll.UseVisualStyleBackColor = true;
            this.buttonSkipAll.Click += new System.EventHandler(this.buttonSkipAll_Click);
            // 
            // buttonChange
            // 
            this.buttonChange.Location = new System.Drawing.Point(22, 47);
            this.buttonChange.Name = "buttonChange";
            this.buttonChange.Size = new System.Drawing.Size(122, 21);
            this.buttonChange.TabIndex = 1;
            this.buttonChange.Text = "Change";
            this.buttonChange.UseVisualStyleBackColor = true;
            this.buttonChange.Click += new System.EventHandler(this.buttonChange_Click);
            // 
            // richTextBoxParagraph
            // 
            this.richTextBoxParagraph.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.richTextBoxParagraph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBoxParagraph.Location = new System.Drawing.Point(12, 258);
            this.richTextBoxParagraph.Name = "richTextBoxParagraph";
            this.richTextBoxParagraph.ReadOnly = true;
            this.richTextBoxParagraph.Size = new System.Drawing.Size(292, 54);
            this.richTextBoxParagraph.TabIndex = 1;
            this.richTextBoxParagraph.Text = "";
            // 
            // buttonEditWholeText
            // 
            this.buttonEditWholeText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonEditWholeText.Location = new System.Drawing.Point(314, 291);
            this.buttonEditWholeText.Name = "buttonEditWholeText";
            this.buttonEditWholeText.Size = new System.Drawing.Size(128, 21);
            this.buttonEditWholeText.TabIndex = 5;
            this.buttonEditWholeText.Text = "Edit whole text";
            this.buttonEditWholeText.UseVisualStyleBackColor = true;
            this.buttonEditWholeText.Click += new System.EventHandler(this.buttonEditWholeText_Click);
            // 
            // labelFullText
            // 
            this.labelFullText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelFullText.AutoSize = true;
            this.labelFullText.Location = new System.Drawing.Point(9, 241);
            this.labelFullText.Name = "labelFullText";
            this.labelFullText.Size = new System.Drawing.Size(43, 13);
            this.labelFullText.TabIndex = 108;
            this.labelFullText.Text = "Full text";
            // 
            // buttonEditWord
            // 
            this.buttonEditWord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonEditWord.Location = new System.Drawing.Point(314, 264);
            this.buttonEditWord.Name = "buttonEditWord";
            this.buttonEditWord.Size = new System.Drawing.Size(128, 21);
            this.buttonEditWord.TabIndex = 3;
            this.buttonEditWord.Text = "Edit word";
            this.buttonEditWord.UseVisualStyleBackColor = true;
            this.buttonEditWord.Visible = false;
            this.buttonEditWord.Click += new System.EventHandler(this.buttonEditWord_Click);
            // 
            // textBoxWholeText
            // 
            this.textBoxWholeText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxWholeText.Location = new System.Drawing.Point(506, 257);
            this.textBoxWholeText.Multiline = true;
            this.textBoxWholeText.Name = "textBoxWholeText";
            this.textBoxWholeText.Size = new System.Drawing.Size(292, 54);
            this.textBoxWholeText.TabIndex = 2;
            this.textBoxWholeText.Visible = false;
            // 
            // buttonUpdateWholeText
            // 
            this.buttonUpdateWholeText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonUpdateWholeText.Location = new System.Drawing.Point(448, 291);
            this.buttonUpdateWholeText.Name = "buttonUpdateWholeText";
            this.buttonUpdateWholeText.Size = new System.Drawing.Size(128, 21);
            this.buttonUpdateWholeText.TabIndex = 4;
            this.buttonUpdateWholeText.Text = "Update whole text";
            this.buttonUpdateWholeText.UseVisualStyleBackColor = true;
            this.buttonUpdateWholeText.Visible = false;
            this.buttonUpdateWholeText.Click += new System.EventHandler(this.buttonUpdateWholeText_Click);
            // 
            // PluginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(813, 560);
            this.Controls.Add(this.buttonUpdateWholeText);
            this.Controls.Add(this.textBoxWholeText);
            this.Controls.Add(this.buttonEditWord);
            this.Controls.Add(this.richTextBoxParagraph);
            this.Controls.Add(this.buttonEditWholeText);
            this.Controls.Add(this.labelFullText);
            this.Controls.Add(this.labelActionInfo);
            this.Controls.Add(this.comboBoxDictionaries);
            this.Controls.Add(this.groupBoxSuggestions);
            this.Controls.Add(this.labelLanguage);
            this.Controls.Add(this.groupBoxWordNotFound);
            this.Controls.Add(this.listViewSubtitle);
            this.Controls.Add(this.labelDescription);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.buttonOK);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(820, 525);
            this.Name = "PluginForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PluginForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PluginForm_FormClosing);
            this.Shown += new System.EventHandler(this.PluginForm_Shown);
            this.ResizeEnd += new System.EventHandler(this.PluginForm_ResizeEnd);
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBoxSuggestions.ResumeLayout(false);
            this.groupBoxWordNotFound.ResumeLayout(false);
            this.groupBoxWordNotFound.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.ListView listViewSubtitle;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.Label labelActionInfo;
        private System.Windows.Forms.ComboBox comboBoxDictionaries;
        private System.Windows.Forms.GroupBox groupBoxSuggestions;
        private System.Windows.Forms.Button buttonUseSuggestion;
        private System.Windows.Forms.Button buttonUseSuggestionAlways;
        private System.Windows.Forms.ListBox listBoxSuggestions;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.GroupBox groupBoxWordNotFound;
        private System.Windows.Forms.Button buttonAddToDictionary;
        private System.Windows.Forms.Button buttonSkipOnce;
        private System.Windows.Forms.Button buttonChangeAll;
        private System.Windows.Forms.TextBox textBoxWord;
        private System.Windows.Forms.Button buttonSkipAll;
        private System.Windows.Forms.Button buttonChange;
        private System.Windows.Forms.RichTextBox richTextBoxParagraph;
        private System.Windows.Forms.Button buttonEditWholeText;
        private System.Windows.Forms.Label labelFullText;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem startSpellCheckFromCurrentLineToolStripMenuItem;
        private System.Windows.Forms.Button buttonGoogleIt;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonEditWord;
        private System.Windows.Forms.TextBox textBoxWholeText;
        private System.Windows.Forms.Button buttonUpdateWholeText;
        private System.Windows.Forms.CheckBox checkBoxUseNamesEtc;
        private System.Windows.Forms.Button buttonAddToNamesEtcList;

    }
}