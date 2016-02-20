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
            this.button1 = new System.Windows.Forms.Button();
            this.labelDescription = new System.Windows.Forms.Label();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.invertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelTotal = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.radioButtonBuiltInList = new System.Windows.Forms.RadioButton();
            this.radioButtonLocalList = new System.Windows.Forms.RadioButton();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageLocalwordsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewBuiltinWordsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
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
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(779, 437);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "C&ancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.buttonCancel_Click);
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
            this.columnHeader5.Text = "Line#";
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
            this.selectAllToolStripMenuItem,
            this.invertToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(155, 48);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.selectAllToolStripMenuItem.Text = "Select all";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // invertToolStripMenuItem
            // 
            this.invertToolStripMenuItem.Name = "invertToolStripMenuItem";
            this.invertToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.invertToolStripMenuItem.Text = "Invert selection";
            this.invertToolStripMenuItem.Click += new System.EventHandler(this.invertToolStripMenuItem_Click);
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
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(12, 451);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(111, 13);
            this.linkLabel1.TabIndex = 6;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Report missing word...";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // linkLabel2
            // 
            this.linkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(129, 451);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(80, 13);
            this.linkLabel2.TabIndex = 5;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "View word list...";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // radioButtonBuiltInList
            // 
            this.radioButtonBuiltInList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonBuiltInList.AutoSize = true;
            this.radioButtonBuiltInList.Checked = true;
            this.radioButtonBuiltInList.Location = new System.Drawing.Point(566, 23);
            this.radioButtonBuiltInList.Name = "radioButtonBuiltInList";
            this.radioButtonBuiltInList.Size = new System.Drawing.Size(92, 17);
            this.radioButtonBuiltInList.TabIndex = 9;
            this.radioButtonBuiltInList.TabStop = true;
            this.radioButtonBuiltInList.Text = "Use built-in list";
            this.radioButtonBuiltInList.UseVisualStyleBackColor = true;
            this.radioButtonBuiltInList.Click += new System.EventHandler(this.radioButtonBuiltInList_Click);
            // 
            // radioButtonLocalList
            // 
            this.radioButtonLocalList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonLocalList.AutoSize = true;
            this.radioButtonLocalList.Location = new System.Drawing.Point(664, 23);
            this.radioButtonLocalList.Name = "radioButtonLocalList";
            this.radioButtonLocalList.Size = new System.Drawing.Size(84, 17);
            this.radioButtonLocalList.TabIndex = 10;
            this.radioButtonLocalList.Text = "Use local list";
            this.radioButtonLocalList.UseVisualStyleBackColor = true;
            this.radioButtonLocalList.Click += new System.EventHandler(this.radioButtonLocalList_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(866, 24);
            this.menuStrip1.TabIndex = 11;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageLocalwordsToolStripMenuItem,
            this.viewBuiltinWordsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // manageLocalwordsToolStripMenuItem
            // 
            this.manageLocalwordsToolStripMenuItem.Name = "manageLocalwordsToolStripMenuItem";
            this.manageLocalwordsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.manageLocalwordsToolStripMenuItem.Text = "Manage local-words";
            this.manageLocalwordsToolStripMenuItem.Click += new System.EventHandler(this.manageLocalwordsToolStripMenuItem_Click);
            // 
            // viewBuiltinWordsToolStripMenuItem
            // 
            this.viewBuiltinWordsToolStripMenuItem.Name = "viewBuiltinWordsToolStripMenuItem";
            this.viewBuiltinWordsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.viewBuiltinWordsToolStripMenuItem.Text = "View built-in words";
            this.viewBuiltinWordsToolStripMenuItem.Click += new System.EventHandler(this.viewBuiltinWordsToolStripMenuItem_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(758, 23);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(96, 17);
            this.checkBox1.TabIndex = 12;
            this.checkBox1.Text = "Disalble built-in";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // PluginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(866, 473);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.radioButtonLocalList);
            this.Controls.Add(this.radioButtonBuiltInList);
            this.Controls.Add(this.linkLabel2);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.labelTotal);
            this.Controls.Add(this.listViewFixes);
            this.Controls.Add(this.labelDescription);
            this.Controls.Add(this.button1);
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
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.Label labelTotal;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.RadioButton radioButtonBuiltInList;
        private System.Windows.Forms.RadioButton radioButtonLocalList;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageLocalwordsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewBuiltinWordsToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem invertToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}