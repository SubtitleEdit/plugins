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
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
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
            this.contextMenuStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
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
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader7,
            this.columnHeader8});
            this.listViewFixes.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.GridLines = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(12, 46);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(842, 385);
            this.listViewFixes.TabIndex = 0;
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
            this.columnHeader5.Text = "Line #";
            this.columnHeader5.Width = 50;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Before";
            this.columnHeader7.Width = 378;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "After";
            this.columnHeader8.Width = 371;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSelectAll,
            this.toolStripMenuItemInvert});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(155, 48);
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
            this.labelTotal.Size = new System.Drawing.Size(34, 13);
            this.labelTotal.TabIndex = 1;
            this.labelTotal.Text = "Total:";
            // 
            // linkLabelIssues
            // 
            this.linkLabelIssues.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelIssues.AutoSize = true;
            this.linkLabelIssues.Location = new System.Drawing.Point(12, 451);
            this.linkLabelIssues.Name = "linkLabelIssues";
            this.linkLabelIssues.Size = new System.Drawing.Size(111, 13);
            this.linkLabelIssues.TabIndex = 5;
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
            this.linkLabelWordList.Size = new System.Drawing.Size(80, 13);
            this.linkLabelWordList.TabIndex = 6;
            this.linkLabelWordList.TabStop = true;
            this.linkLabelWordList.Text = "View word list...";
            this.linkLabelWordList.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelWordList_LinkClicked);
            // 
            // radioButtonBuiltInList
            // 
            this.radioButtonBuiltInList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonBuiltInList.AutoSize = true;
            this.radioButtonBuiltInList.Checked = true;
            this.radioButtonBuiltInList.Location = new System.Drawing.Point(578, 23);
            this.radioButtonBuiltInList.Name = "radioButtonBuiltInList";
            this.radioButtonBuiltInList.Size = new System.Drawing.Size(92, 17);
            this.radioButtonBuiltInList.TabIndex = 7;
            this.radioButtonBuiltInList.TabStop = true;
            this.radioButtonBuiltInList.Text = "Use built-in list";
            this.radioButtonBuiltInList.UseVisualStyleBackColor = true;
            this.radioButtonBuiltInList.Click += new System.EventHandler(this.radioButtonBuiltInList_Click);
            // 
            // radioButtonLocalList
            // 
            this.radioButtonLocalList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonLocalList.AutoSize = true;
            this.radioButtonLocalList.Location = new System.Drawing.Point(676, 23);
            this.radioButtonLocalList.Name = "radioButtonLocalList";
            this.radioButtonLocalList.Size = new System.Drawing.Size(84, 17);
            this.radioButtonLocalList.TabIndex = 8;
            this.radioButtonLocalList.Text = "Use local list";
            this.radioButtonLocalList.UseVisualStyleBackColor = true;
            this.radioButtonLocalList.Click += new System.EventHandler(this.radioButtonLocalList_Click);
            // 
            // radioButtonBothLists
            // 
            this.radioButtonBothLists.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonBothLists.AutoSize = true;
            this.radioButtonBothLists.Location = new System.Drawing.Point(766, 23);
            this.radioButtonBothLists.Name = "radioButtonBothLists";
            this.radioButtonBothLists.Size = new System.Drawing.Size(88, 17);
            this.radioButtonBothLists.TabIndex = 9;
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
            // PluginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(866, 473);
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
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 428);
            this.Name = "PluginForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PluginForm";
            this.Load += new System.EventHandler(this.PluginForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PluginForm_KeyDown);
            this.Resize += new System.EventHandler(this.PluginForm_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
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
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelectAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInvert;
    }
}