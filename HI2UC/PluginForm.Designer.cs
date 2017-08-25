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
            this.labelDesc = new System.Windows.Forms.Label();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.buttonConvert = new System.Windows.Forms.Button();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.checkAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uncheckAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.invertCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBoxNames = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.comboBoxStyle = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.checkBoxRemoveSpaces = new System.Windows.Forms.CheckBox();
            this.checkBoxMoods = new System.Windows.Forms.CheckBox();
            this.checkBoxSingleLineNarrator = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelDesc
            // 
            this.labelDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelDesc.AutoSize = true;
            this.labelDesc.Location = new System.Drawing.Point(12, 524);
            this.labelDesc.Name = "labelDesc";
            this.labelDesc.Size = new System.Drawing.Size(83, 13);
            this.labelDesc.TabIndex = 6;
            this.labelDesc.Text = "description label";
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Location = new System.Drawing.Point(806, 527);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(100, 23);
            this.btn_Cancel.TabIndex = 1;
            this.btn_Cancel.Text = "&Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // buttonConvert
            // 
            this.buttonConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConvert.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonConvert.Location = new System.Drawing.Point(700, 527);
            this.buttonConvert.Name = "buttonConvert";
            this.buttonConvert.Size = new System.Drawing.Size(100, 23);
            this.buttonConvert.TabIndex = 0;
            this.buttonConvert.Text = "&OK";
            this.buttonConvert.UseVisualStyleBackColor = true;
            this.buttonConvert.Click += new System.EventHandler(this.btn_Run_Click);
            // 
            // listViewFixes
            // 
            this.listViewFixes.AllowColumnReorder = true;
            this.listViewFixes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listViewFixes.CheckBoxes = true;
            this.listViewFixes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader1,
            this.columnHeader5,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewFixes.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewFixes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.GridLines = true;
            this.listViewFixes.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(3, 16);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.ShowGroups = false;
            this.listViewFixes.Size = new System.Drawing.Size(891, 446);
            this.listViewFixes.TabIndex = 0;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            this.listViewFixes.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewFixes_ItemChecked);
            this.listViewFixes.Resize += new System.EventHandler(this.listViewFixes_Resize);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Apply";
            this.columnHeader4.Width = 45;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Line #";
            this.columnHeader1.Width = 50;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Match";
            this.columnHeader5.Width = 81;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Actual Status";
            this.columnHeader2.Width = 371;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "After";
            this.columnHeader3.Width = 341;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkAllToolStripMenuItem,
            this.uncheckAllToolStripMenuItem,
            this.invertCheckToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.deleteLineToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 114);
            // 
            // checkAllToolStripMenuItem
            // 
            this.checkAllToolStripMenuItem.Name = "checkAllToolStripMenuItem";
            this.checkAllToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.checkAllToolStripMenuItem.Text = "Check all";
            this.checkAllToolStripMenuItem.Click += new System.EventHandler(this.CheckTypeStyle);
            // 
            // uncheckAllToolStripMenuItem
            // 
            this.uncheckAllToolStripMenuItem.Name = "uncheckAllToolStripMenuItem";
            this.uncheckAllToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.uncheckAllToolStripMenuItem.Text = "Uncheck all";
            this.uncheckAllToolStripMenuItem.Click += new System.EventHandler(this.CheckTypeStyle);
            // 
            // invertCheckToolStripMenuItem
            // 
            this.invertCheckToolStripMenuItem.Name = "invertCheckToolStripMenuItem";
            this.invertCheckToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.invertCheckToolStripMenuItem.Text = "Invert check";
            this.invertCheckToolStripMenuItem.Click += new System.EventHandler(this.CheckTypeStyle);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.CheckTypeStyle);
            // 
            // deleteLineToolStripMenuItem
            // 
            this.deleteLineToolStripMenuItem.Name = "deleteLineToolStripMenuItem";
            this.deleteLineToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.deleteLineToolStripMenuItem.Text = "Remove Line/s";
            this.deleteLineToolStripMenuItem.Click += new System.EventHandler(this.CheckTypeStyle);
            // 
            // checkBoxNames
            // 
            this.checkBoxNames.AutoSize = true;
            this.checkBoxNames.Checked = true;
            this.checkBoxNames.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxNames.Location = new System.Drawing.Point(15, 10);
            this.checkBoxNames.Name = "checkBoxNames";
            this.checkBoxNames.Size = new System.Drawing.Size(206, 17);
            this.checkBoxNames.TabIndex = 3;
            this.checkBoxNames.Text = "Narrator. John: Hello! => JOHN: Hello!";
            this.toolTip1.SetToolTip(this.checkBoxNames, "This will change the Narrator Text\r\nEx: Harry: Hello! -> HARRY: Hello!");
            this.checkBoxNames.UseVisualStyleBackColor = true;
            this.checkBoxNames.CheckedChanged += new System.EventHandler(this.checkBoxNarrator_CheckedChanged);
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // comboBoxStyle
            // 
            this.comboBoxStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStyle.FormattingEnabled = true;
            this.comboBoxStyle.Items.AddRange(new object[] {
            "Uppercase (HELLO)",
            "Lowercase (hello)",
            "First Char in Word (Hello Hello)",
            "UpperLower (HeLlO)"});
            this.comboBoxStyle.Location = new System.Drawing.Point(756, 38);
            this.comboBoxStyle.MaxDropDownItems = 5;
            this.comboBoxStyle.Name = "comboBoxStyle";
            this.comboBoxStyle.Size = new System.Drawing.Size(153, 21);
            this.comboBoxStyle.TabIndex = 2;
            this.comboBoxStyle.SelectedIndexChanged += new System.EventHandler(this.comboBoxStyle_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(752, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Feelings and Moods Style:";
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(12, 540);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(236, 13);
            this.linkLabel1.TabIndex = 7;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Report Bugs | Suggestion: ivandrofly@gmail.com";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.listViewFixes);
            this.groupBox1.Location = new System.Drawing.Point(12, 56);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(897, 465);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // buttonApply
            // 
            this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApply.Location = new System.Drawing.Point(594, 527);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(100, 23);
            this.buttonApply.TabIndex = 8;
            this.buttonApply.Text = "&Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // checkBoxRemoveSpaces
            // 
            this.checkBoxRemoveSpaces.AutoSize = true;
            this.checkBoxRemoveSpaces.Location = new System.Drawing.Point(15, 33);
            this.checkBoxRemoveSpaces.Name = "checkBoxRemoveSpaces";
            this.checkBoxRemoveSpaces.Size = new System.Drawing.Size(229, 17);
            this.checkBoxRemoveSpaces.TabIndex = 9;
            this.checkBoxRemoveSpaces.Text = "Remove Extra Spaces \'( music )\' => (music)";
            this.checkBoxRemoveSpaces.UseVisualStyleBackColor = true;
            this.checkBoxRemoveSpaces.CheckedChanged += new System.EventHandler(this.checkBoxRemoveSpaces_CheckedChanged);
            // 
            // checkBoxMoods
            // 
            this.checkBoxMoods.AutoSize = true;
            this.checkBoxMoods.Checked = true;
            this.checkBoxMoods.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMoods.Location = new System.Drawing.Point(260, 10);
            this.checkBoxMoods.Name = "checkBoxMoods";
            this.checkBoxMoods.Size = new System.Drawing.Size(189, 17);
            this.checkBoxMoods.TabIndex = 10;
            this.checkBoxMoods.Text = "Moods. e.g: (foobar) => (FOOBAR)";
            this.checkBoxMoods.UseVisualStyleBackColor = true;
            this.checkBoxMoods.CheckedChanged += new System.EventHandler(this.checkBoxMoods_CheckedChanged);
            // 
            // checkBoxSingleLineNarrator
            // 
            this.checkBoxSingleLineNarrator.AutoSize = true;
            this.checkBoxSingleLineNarrator.Checked = true;
            this.checkBoxSingleLineNarrator.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSingleLineNarrator.Location = new System.Drawing.Point(260, 33);
            this.checkBoxSingleLineNarrator.Name = "checkBoxSingleLineNarrator";
            this.checkBoxSingleLineNarrator.Size = new System.Drawing.Size(226, 17);
            this.checkBoxSingleLineNarrator.TabIndex = 11;
            this.checkBoxSingleLineNarrator.Text = "Lena:<br/>A ring?! => LENA:<br/>A ring?!";
            this.checkBoxSingleLineNarrator.UseVisualStyleBackColor = true;
            this.checkBoxSingleLineNarrator.CheckedChanged += new System.EventHandler(this.checkBoxSingleLineNarrator_CheckedChanged);
            // 
            // PluginForm
            // 
            this.AcceptButton = this.buttonConvert;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(918, 562);
            this.Controls.Add(this.checkBoxSingleLineNarrator);
            this.Controls.Add(this.checkBoxMoods);
            this.Controls.Add(this.checkBoxRemoveSpaces);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.checkBoxNames);
            this.Controls.Add(this.comboBoxStyle);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.buttonConvert);
            this.Controls.Add(this.labelDesc);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(711, 425);
            this.Name = "PluginForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Hearing Impaired to Uppercase";
            this.Load += new System.EventHandler(this.PluginForm_Load);
            this.Shown += new System.EventHandler(this.PluginForm_Shown);
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelDesc;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button buttonConvert;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.CheckBox checkBoxNames;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem checkAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uncheckAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem invertCheckToolStripMenuItem;
        private System.Windows.Forms.ComboBox comboBoxStyle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteLineToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.CheckBox checkBoxRemoveSpaces;
        private System.Windows.Forms.CheckBox checkBoxMoods;
        private System.Windows.Forms.CheckBox checkBoxSingleLineNarrator;
    }
}