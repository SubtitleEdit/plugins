namespace Nikse.SubtitleEdit.PluginLogic
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
            this.components = new System.ComponentModel.Container();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelDescription = new System.Windows.Forms.Label();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripFixes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemReset = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorFixes1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInvert = new System.Windows.Forms.ToolStripMenuItem();
            this.labelTotal = new System.Windows.Forms.Label();
            this.linkLabelIssues = new System.Windows.Forms.LinkLabel();
            this.linkLabelWordList = new System.Windows.Forms.LinkLabel();
            this.radioButtonBuiltInList = new System.Windows.Forms.RadioButton();
            this.radioButtonLocalList = new System.Windows.Forms.RadioButton();
            this.radioButtonBothLists = new System.Windows.Forms.RadioButton();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItemFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemManageLocalWords = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemViewBuiltInWords = new System.Windows.Forms.ToolStripMenuItem();
            this.panelEditAfter = new System.Windows.Forms.Panel();
            this.textBoxEditAfter = new System.Windows.Forms.TextBox();
            this.contextMenuStripFixes.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.panelEditAfter.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(698, 437);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(779, 437);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(12, 27);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(82, 13);
            this.labelDescription.TabIndex = 4;
            this.labelDescription.Text = "labelDescription";
            // 
            // listViewFixes
            // 
            this.listViewFixes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFixes.CheckBoxes = true;
            this.listViewFixes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listViewFixes.ContextMenuStrip = this.contextMenuStripFixes;
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.GridLines = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(12, 46);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(842, 385);
            this.listViewFixes.TabIndex = 0;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            this.listViewFixes.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewFixes_KeyDown);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Apply";
            this.columnHeader1.Width = 40;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Line #";
            this.columnHeader2.Width = 50;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Before";
            this.columnHeader3.Width = 364;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "After";
            this.columnHeader4.Width = 364;
            // 
            // contextMenuStripFixes
            // 
            this.contextMenuStripFixes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemEdit,
            this.toolStripMenuItemReset,
            this.toolStripSeparatorFixes1,
            this.toolStripMenuItemSelectAll,
            this.toolStripMenuItemInvert});
            this.contextMenuStripFixes.Name = "contextMenuStripFixes";
            this.contextMenuStripFixes.Size = new System.Drawing.Size(155, 98);
            this.contextMenuStripFixes.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripFixes_Opening);
            // 
            // toolStripMenuItemEdit
            // 
            this.toolStripMenuItemEdit.Name = "toolStripMenuItemEdit";
            this.toolStripMenuItemEdit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.toolStripMenuItemEdit.Size = new System.Drawing.Size(154, 22);
            this.toolStripMenuItemEdit.Text = "Edit";
            this.toolStripMenuItemEdit.Click += new System.EventHandler(this.toolStripMenuItemEdit_Click);
            // 
            // toolStripMenuItemReset
            // 
            this.toolStripMenuItemReset.Name = "toolStripMenuItemReset";
            this.toolStripMenuItemReset.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.toolStripMenuItemReset.Size = new System.Drawing.Size(154, 22);
            this.toolStripMenuItemReset.Text = "Reset";
            this.toolStripMenuItemReset.Click += new System.EventHandler(this.toolStripMenuItemReset_Click);
            // 
            // toolStripSeparatorFixes1
            // 
            this.toolStripSeparatorFixes1.Name = "toolStripSeparatorFixes1";
            this.toolStripSeparatorFixes1.Size = new System.Drawing.Size(151, 6);
            // 
            // toolStripMenuItemSelectAll
            // 
            this.toolStripMenuItemSelectAll.Name = "toolStripMenuItemSelectAll";
            this.toolStripMenuItemSelectAll.Size = new System.Drawing.Size(154, 22);
            this.toolStripMenuItemSelectAll.Text = "Select all";
            this.toolStripMenuItemSelectAll.Click += new System.EventHandler(this.toolStripMenuItemSelectAll_Click);
            // 
            // toolStripMenuItemInvert
            // 
            this.toolStripMenuItemInvert.Name = "toolStripMenuItemInvert";
            this.toolStripMenuItemInvert.Size = new System.Drawing.Size(154, 22);
            this.toolStripMenuItemInvert.Text = "Invert selection";
            this.toolStripMenuItemInvert.Click += new System.EventHandler(this.toolStripMenuItemInvert_Click);
            // 
            // labelTotal
            // 
            this.labelTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTotal.AutoSize = true;
            this.labelTotal.Location = new System.Drawing.Point(9, 434);
            this.labelTotal.Name = "labelTotal";
            this.labelTotal.Size = new System.Drawing.Size(35, 13);
            this.labelTotal.TabIndex = 1;
            this.labelTotal.Text = "Total:";
            // 
            // linkLabelIssues
            // 
            this.linkLabelIssues.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelIssues.AutoSize = true;
            this.linkLabelIssues.Location = new System.Drawing.Point(12, 451);
            this.linkLabelIssues.Name = "linkLabelIssues";
            this.linkLabelIssues.Size = new System.Drawing.Size(116, 13);
            this.linkLabelIssues.TabIndex = 8;
            this.linkLabelIssues.TabStop = true;
            this.linkLabelIssues.Text = "Report missing word...";
            this.linkLabelIssues.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelIssues_LinkClicked);
            // 
            // linkLabelWordList
            // 
            this.linkLabelWordList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelWordList.AutoSize = true;
            this.linkLabelWordList.Location = new System.Drawing.Point(129, 451);
            this.linkLabelWordList.Name = "linkLabelWordList";
            this.linkLabelWordList.Size = new System.Drawing.Size(84, 13);
            this.linkLabelWordList.TabIndex = 9;
            this.linkLabelWordList.TabStop = true;
            this.linkLabelWordList.Text = "View word list...";
            this.linkLabelWordList.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelWordList_LinkClicked);
            // 
            // radioButtonBuiltInList
            // 
            this.radioButtonBuiltInList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonBuiltInList.AutoSize = true;
            this.radioButtonBuiltInList.Checked = true;
            this.radioButtonBuiltInList.Location = new System.Drawing.Point(576, 23);
            this.radioButtonBuiltInList.Name = "radioButtonBuiltInList";
            this.radioButtonBuiltInList.Size = new System.Drawing.Size(94, 17);
            this.radioButtonBuiltInList.TabIndex = 5;
            this.radioButtonBuiltInList.TabStop = true;
            this.radioButtonBuiltInList.Text = "Use built-in list";
            this.radioButtonBuiltInList.UseVisualStyleBackColor = true;
            this.radioButtonBuiltInList.Click += new System.EventHandler(this.radioButtonBuiltInList_Click);
            // 
            // radioButtonLocalList
            // 
            this.radioButtonLocalList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonLocalList.AutoSize = true;
            this.radioButtonLocalList.Location = new System.Drawing.Point(677, 23);
            this.radioButtonLocalList.Name = "radioButtonLocalList";
            this.radioButtonLocalList.Size = new System.Drawing.Size(83, 17);
            this.radioButtonLocalList.TabIndex = 6;
            this.radioButtonLocalList.Text = "Use local list";
            this.radioButtonLocalList.UseVisualStyleBackColor = true;
            this.radioButtonLocalList.Click += new System.EventHandler(this.radioButtonLocalList_Click);
            // 
            // radioButtonBothLists
            // 
            this.radioButtonBothLists.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonBothLists.AutoSize = true;
            this.radioButtonBothLists.Location = new System.Drawing.Point(765, 23);
            this.radioButtonBothLists.Name = "radioButtonBothLists";
            this.radioButtonBothLists.Size = new System.Drawing.Size(89, 17);
            this.radioButtonBothLists.TabIndex = 7;
            this.radioButtonBothLists.Text = "Use both lists";
            this.radioButtonBothLists.UseVisualStyleBackColor = true;
            this.radioButtonBothLists.Click += new System.EventHandler(this.radioButtonBothLists_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemFile});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(866, 24);
            this.menuStrip1.TabIndex = 10;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItemFile
            // 
            this.toolStripMenuItemFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemManageLocalWords,
            this.toolStripMenuItemViewBuiltInWords});
            this.toolStripMenuItemFile.Name = "toolStripMenuItemFile";
            this.toolStripMenuItemFile.Size = new System.Drawing.Size(37, 20);
            this.toolStripMenuItemFile.Text = "File";
            // 
            // toolStripMenuItemManageLocalWords
            // 
            this.toolStripMenuItemManageLocalWords.Name = "toolStripMenuItemManageLocalWords";
            this.toolStripMenuItemManageLocalWords.Size = new System.Drawing.Size(193, 22);
            this.toolStripMenuItemManageLocalWords.Text = "Manage local word list";
            this.toolStripMenuItemManageLocalWords.Click += new System.EventHandler(this.toolStripMenuItemManageLocalwords_Click);
            // 
            // toolStripMenuItemViewBuiltInWords
            // 
            this.toolStripMenuItemViewBuiltInWords.Name = "toolStripMenuItemViewBuiltInWords";
            this.toolStripMenuItemViewBuiltInWords.Size = new System.Drawing.Size(193, 22);
            this.toolStripMenuItemViewBuiltInWords.Text = "View built-in word list";
            this.toolStripMenuItemViewBuiltInWords.Click += new System.EventHandler(this.toolStripMenuItemViewBuiltInWords_Click);
            // 
            // panelEditAfter
            // 
            this.panelEditAfter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelEditAfter.Controls.Add(this.textBoxEditAfter);
            this.panelEditAfter.Location = new System.Drawing.Point(32, 86);
            this.panelEditAfter.Name = "panelEditAfter";
            this.panelEditAfter.Padding = new System.Windows.Forms.Padding(5);
            this.panelEditAfter.Size = new System.Drawing.Size(460, 83);
            this.panelEditAfter.TabIndex = 11;
            this.panelEditAfter.Visible = false;
            this.panelEditAfter.Leave += new System.EventHandler(this.panelEditAfter_Leave);
            // 
            // textBoxEditAfter
            // 
            this.textBoxEditAfter.AcceptsReturn = true;
            this.textBoxEditAfter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxEditAfter.Location = new System.Drawing.Point(5, 5);
            this.textBoxEditAfter.Multiline = true;
            this.textBoxEditAfter.Name = "textBoxEditAfter";
            this.textBoxEditAfter.Size = new System.Drawing.Size(448, 71);
            this.textBoxEditAfter.TabIndex = 0;
            this.textBoxEditAfter.WordWrap = false;
            this.textBoxEditAfter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxEditAfter_KeyDown);
            // 
            // PluginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(866, 473);
            this.Controls.Add(this.panelEditAfter);
            this.Controls.Add(this.radioButtonBothLists);
            this.Controls.Add(this.radioButtonLocalList);
            this.Controls.Add(this.radioButtonBuiltInList);
            this.Controls.Add(this.linkLabelWordList);
            this.Controls.Add(this.linkLabelIssues);
            this.Controls.Add(this.labelTotal);
            this.Controls.Add(this.listViewFixes);
            this.Controls.Add(this.labelDescription);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 428);
            this.Name = "PluginForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PluginForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PluginForm_FormClosed);
            this.Shown += new System.EventHandler(this.PluginForm_Shown);
            this.SizeChanged += new System.EventHandler(this.PluginForm_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PluginForm_KeyDown);
            this.contextMenuStripFixes.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panelEditAfter.ResumeLayout(false);
            this.panelEditAfter.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label labelTotal;
        private System.Windows.Forms.LinkLabel linkLabelIssues;
        private System.Windows.Forms.LinkLabel linkLabelWordList;
        private System.Windows.Forms.RadioButton radioButtonBuiltInList;
        private System.Windows.Forms.RadioButton radioButtonLocalList;
        private System.Windows.Forms.RadioButton radioButtonBothLists;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFile;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemManageLocalWords;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemViewBuiltInWords;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFixes;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemEdit;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemReset;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorFixes1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelectAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInvert;
        private System.Windows.Forms.Panel panelEditAfter;
        private System.Windows.Forms.TextBox textBoxEditAfter;
    }
}