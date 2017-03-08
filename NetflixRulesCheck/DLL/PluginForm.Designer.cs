namespace SubtitleEdit
{
    sealed partial class PluginForm
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.labelDescription = new System.Windows.Forms.Label();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.labelTotal = new System.Windows.Forms.Label();
            this.buttonInverseSelection = new System.Windows.Forms.Button();
            this.buttonSelectAll = new System.Windows.Forms.Button();
            this.groupBoxRules = new System.Windows.Forms.GroupBox();
            this.checkBoxGapMinTwoFrames = new System.Windows.Forms.CheckBox();
            this.checkBoxCheckValidGlyphs = new System.Windows.Forms.CheckBox();
            this.checkBox17CharsPerSecond = new System.Windows.Forms.CheckBox();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.checkBoxWriteOutOneToTen = new System.Windows.Forms.CheckBox();
            this.checkBoxSpellOutStartNumbers = new System.Windows.Forms.CheckBox();
            this.checkBoxSquareBracketForHi = new System.Windows.Forms.CheckBox();
            this.checkBoxDialogHypenNoSpace = new System.Windows.Forms.CheckBox();
            this.checkBoxTwoLinesMax = new System.Windows.Forms.CheckBox();
            this.checkBoxMinDuration = new System.Windows.Forms.CheckBox();
            this.checkBoxMaxDuration = new System.Windows.Forms.CheckBox();
            this.groupBoxRules.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(1075, 697);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(1156, 697);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "C&ancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(12, 180);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(82, 13);
            this.labelDescription.TabIndex = 2;
            this.labelDescription.Text = "labelDescription";
            // 
            // listViewFixes
            // 
            this.listViewFixes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFixes.CheckBoxes = true;
            this.listViewFixes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8});
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.GridLines = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(12, 199);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(1219, 492);
            this.listViewFixes.TabIndex = 101;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Apply";
            this.columnHeader4.Width = 38;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Line#";
            this.columnHeader5.Width = 50;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Function";
            this.columnHeader6.Width = 169;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Before";
            this.columnHeader7.Width = 340;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "After";
            this.columnHeader8.Width = 318;
            // 
            // labelTotal
            // 
            this.labelTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTotal.AutoSize = true;
            this.labelTotal.Location = new System.Drawing.Point(9, 694);
            this.labelTotal.Name = "labelTotal";
            this.labelTotal.Size = new System.Drawing.Size(34, 13);
            this.labelTotal.TabIndex = 102;
            this.labelTotal.Text = "Total:";
            // 
            // buttonInverseSelection
            // 
            this.buttonInverseSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInverseSelection.Location = new System.Drawing.Point(1126, 170);
            this.buttonInverseSelection.Name = "buttonInverseSelection";
            this.buttonInverseSelection.Size = new System.Drawing.Size(105, 23);
            this.buttonInverseSelection.TabIndex = 103;
            this.buttonInverseSelection.Text = "Inverse selection";
            this.buttonInverseSelection.UseVisualStyleBackColor = true;
            this.buttonInverseSelection.Click += new System.EventHandler(this.buttonInverseSelection_Click);
            // 
            // buttonSelectAll
            // 
            this.buttonSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectAll.Location = new System.Drawing.Point(1045, 170);
            this.buttonSelectAll.Name = "buttonSelectAll";
            this.buttonSelectAll.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectAll.TabIndex = 104;
            this.buttonSelectAll.Text = "Select all";
            this.buttonSelectAll.UseVisualStyleBackColor = true;
            this.buttonSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
            // 
            // groupBoxRules
            // 
            this.groupBoxRules.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxRules.Controls.Add(this.checkBoxGapMinTwoFrames);
            this.groupBoxRules.Controls.Add(this.checkBoxCheckValidGlyphs);
            this.groupBoxRules.Controls.Add(this.checkBox17CharsPerSecond);
            this.groupBoxRules.Controls.Add(this.comboBoxLanguage);
            this.groupBoxRules.Controls.Add(this.labelLanguage);
            this.groupBoxRules.Controls.Add(this.checkBoxWriteOutOneToTen);
            this.groupBoxRules.Controls.Add(this.checkBoxSpellOutStartNumbers);
            this.groupBoxRules.Controls.Add(this.checkBoxSquareBracketForHi);
            this.groupBoxRules.Controls.Add(this.checkBoxDialogHypenNoSpace);
            this.groupBoxRules.Controls.Add(this.checkBoxTwoLinesMax);
            this.groupBoxRules.Controls.Add(this.checkBoxMinDuration);
            this.groupBoxRules.Controls.Add(this.checkBoxMaxDuration);
            this.groupBoxRules.Location = new System.Drawing.Point(15, 13);
            this.groupBoxRules.Name = "groupBoxRules";
            this.groupBoxRules.Size = new System.Drawing.Size(1216, 151);
            this.groupBoxRules.TabIndex = 105;
            this.groupBoxRules.TabStop = false;
            this.groupBoxRules.Text = "Rules";
            // 
            // checkBoxGapMinTwoFrames
            // 
            this.checkBoxGapMinTwoFrames.AutoSize = true;
            this.checkBoxGapMinTwoFrames.Checked = true;
            this.checkBoxGapMinTwoFrames.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxGapMinTwoFrames.Location = new System.Drawing.Point(20, 101);
            this.checkBoxGapMinTwoFrames.Name = "checkBoxGapMinTwoFrames";
            this.checkBoxGapMinTwoFrames.Size = new System.Drawing.Size(168, 17);
            this.checkBoxGapMinTwoFrames.TabIndex = 6;
            this.checkBoxGapMinTwoFrames.Text = "Frame gap:  minimum 2 frames";
            this.checkBoxGapMinTwoFrames.UseVisualStyleBackColor = true;
            this.checkBoxGapMinTwoFrames.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxCheckValidGlyphs
            // 
            this.checkBoxCheckValidGlyphs.AutoSize = true;
            this.checkBoxCheckValidGlyphs.Checked = true;
            this.checkBoxCheckValidGlyphs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCheckValidGlyphs.Location = new System.Drawing.Point(410, 124);
            this.checkBoxCheckValidGlyphs.Name = "checkBoxCheckValidGlyphs";
            this.checkBoxCheckValidGlyphs.Size = new System.Drawing.Size(330, 17);
            this.checkBoxCheckValidGlyphs.TabIndex = 21;
            this.checkBoxCheckValidGlyphs.Text = "Only text/characters included in the Netflix Glyph List (version 2) ";
            this.checkBoxCheckValidGlyphs.UseVisualStyleBackColor = true;
            this.checkBoxCheckValidGlyphs.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBox17CharsPerSecond
            // 
            this.checkBox17CharsPerSecond.AutoSize = true;
            this.checkBox17CharsPerSecond.Checked = true;
            this.checkBox17CharsPerSecond.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox17CharsPerSecond.Location = new System.Drawing.Point(20, 78);
            this.checkBox17CharsPerSecond.Name = "checkBox17CharsPerSecond";
            this.checkBox17CharsPerSecond.Size = new System.Drawing.Size(290, 17);
            this.checkBox17CharsPerSecond.TabIndex = 5;
            this.checkBox17CharsPerSecond.Text = "Maximum 17 characters per second (excl. white spaces)";
            this.checkBox17CharsPerSecond.UseVisualStyleBackColor = true;
            this.checkBox17CharsPerSecond.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(945, 36);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(196, 21);
            this.comboBoxLanguage.TabIndex = 30;
            // 
            // labelLanguage
            // 
            this.labelLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(942, 20);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(55, 13);
            this.labelLanguage.TabIndex = 8;
            this.labelLanguage.Text = "Language";
            // 
            // checkBoxWriteOutOneToTen
            // 
            this.checkBoxWriteOutOneToTen.AutoSize = true;
            this.checkBoxWriteOutOneToTen.Checked = true;
            this.checkBoxWriteOutOneToTen.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxWriteOutOneToTen.Location = new System.Drawing.Point(410, 101);
            this.checkBoxWriteOutOneToTen.Name = "checkBoxWriteOutOneToTen";
            this.checkBoxWriteOutOneToTen.Size = new System.Drawing.Size(335, 17);
            this.checkBoxWriteOutOneToTen.TabIndex = 17;
            this.checkBoxWriteOutOneToTen.Text = "From 1 to 10, numbers should be written out: One, two, three, etc.";
            this.checkBoxWriteOutOneToTen.UseVisualStyleBackColor = true;
            this.checkBoxWriteOutOneToTen.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxSpellOutStartNumbers
            // 
            this.checkBoxSpellOutStartNumbers.AutoSize = true;
            this.checkBoxSpellOutStartNumbers.Checked = true;
            this.checkBoxSpellOutStartNumbers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSpellOutStartNumbers.Location = new System.Drawing.Point(410, 78);
            this.checkBoxSpellOutStartNumbers.Name = "checkBoxSpellOutStartNumbers";
            this.checkBoxSpellOutStartNumbers.Size = new System.Drawing.Size(341, 17);
            this.checkBoxSpellOutStartNumbers.TabIndex = 15;
            this.checkBoxSpellOutStartNumbers.Text = "When a number begins a sentence, it should always be spelled out";
            this.checkBoxSpellOutStartNumbers.UseVisualStyleBackColor = true;
            this.checkBoxSpellOutStartNumbers.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxSquareBracketForHi
            // 
            this.checkBoxSquareBracketForHi.AutoSize = true;
            this.checkBoxSquareBracketForHi.Checked = true;
            this.checkBoxSquareBracketForHi.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSquareBracketForHi.Location = new System.Drawing.Point(410, 55);
            this.checkBoxSquareBracketForHi.Name = "checkBoxSquareBracketForHi";
            this.checkBoxSquareBracketForHi.Size = new System.Drawing.Size(286, 17);
            this.checkBoxSquareBracketForHi.TabIndex = 11;
            this.checkBoxSquareBracketForHi.Text = "Use brackets[] to enclose speaker IDs or sound effects";
            this.checkBoxSquareBracketForHi.UseVisualStyleBackColor = true;
            this.checkBoxSquareBracketForHi.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxDialogHypenNoSpace
            // 
            this.checkBoxDialogHypenNoSpace.AutoSize = true;
            this.checkBoxDialogHypenNoSpace.Checked = true;
            this.checkBoxDialogHypenNoSpace.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDialogHypenNoSpace.Location = new System.Drawing.Point(410, 31);
            this.checkBoxDialogHypenNoSpace.Name = "checkBoxDialogHypenNoSpace";
            this.checkBoxDialogHypenNoSpace.Size = new System.Drawing.Size(249, 17);
            this.checkBoxDialogHypenNoSpace.TabIndex = 9;
            this.checkBoxDialogHypenNoSpace.Text = " Dual Speakers: Use a hyphen without a space";
            this.checkBoxDialogHypenNoSpace.UseVisualStyleBackColor = true;
            this.checkBoxDialogHypenNoSpace.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxTwoLinesMax
            // 
            this.checkBoxTwoLinesMax.AutoSize = true;
            this.checkBoxTwoLinesMax.Checked = true;
            this.checkBoxTwoLinesMax.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTwoLinesMax.Location = new System.Drawing.Point(20, 124);
            this.checkBoxTwoLinesMax.Name = "checkBoxTwoLinesMax";
            this.checkBoxTwoLinesMax.Size = new System.Drawing.Size(117, 17);
            this.checkBoxTwoLinesMax.TabIndex = 7;
            this.checkBoxTwoLinesMax.Text = "Two lines maximum";
            this.checkBoxTwoLinesMax.UseVisualStyleBackColor = true;
            this.checkBoxTwoLinesMax.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxMinDuration
            // 
            this.checkBoxMinDuration.AutoSize = true;
            this.checkBoxMinDuration.Checked = true;
            this.checkBoxMinDuration.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMinDuration.Location = new System.Drawing.Point(20, 31);
            this.checkBoxMinDuration.Name = "checkBoxMinDuration";
            this.checkBoxMinDuration.Size = new System.Drawing.Size(212, 17);
            this.checkBoxMinDuration.TabIndex = 1;
            this.checkBoxMinDuration.Text = "Minimum duration: 5/6 second (833 ms)";
            this.checkBoxMinDuration.UseVisualStyleBackColor = true;
            this.checkBoxMinDuration.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxMaxDuration
            // 
            this.checkBoxMaxDuration.AutoSize = true;
            this.checkBoxMaxDuration.Checked = true;
            this.checkBoxMaxDuration.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMaxDuration.Location = new System.Drawing.Point(20, 55);
            this.checkBoxMaxDuration.Name = "checkBoxMaxDuration";
            this.checkBoxMaxDuration.Size = new System.Drawing.Size(250, 17);
            this.checkBoxMaxDuration.TabIndex = 3;
            this.checkBoxMaxDuration.Text = "Maximum duration: 7 seconds per subtitle event";
            this.checkBoxMaxDuration.UseVisualStyleBackColor = true;
            this.checkBoxMaxDuration.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // PluginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1243, 732);
            this.Controls.Add(this.groupBoxRules);
            this.Controls.Add(this.buttonSelectAll);
            this.Controls.Add(this.buttonInverseSelection);
            this.Controls.Add(this.labelTotal);
            this.Controls.Add(this.listViewFixes);
            this.Controls.Add(this.labelDescription);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.buttonOK);
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(979, 528);
            this.Name = "PluginForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PluginForm";
            this.groupBoxRules.ResumeLayout(false);
            this.groupBoxRules.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.Label labelTotal;
        private System.Windows.Forms.Button buttonInverseSelection;
        private System.Windows.Forms.Button buttonSelectAll;
        private System.Windows.Forms.GroupBox groupBoxRules;
        private System.Windows.Forms.CheckBox checkBoxSquareBracketForHi;
        private System.Windows.Forms.CheckBox checkBoxDialogHypenNoSpace;
        private System.Windows.Forms.CheckBox checkBoxTwoLinesMax;
        private System.Windows.Forms.CheckBox checkBoxMinDuration;
        private System.Windows.Forms.CheckBox checkBoxMaxDuration;
        private System.Windows.Forms.CheckBox checkBoxSpellOutStartNumbers;
        private System.Windows.Forms.CheckBox checkBoxWriteOutOneToTen;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.CheckBox checkBox17CharsPerSecond;
        private System.Windows.Forms.CheckBox checkBoxCheckValidGlyphs;
        private System.Windows.Forms.CheckBox checkBoxGapMinTwoFrames;
    }
}