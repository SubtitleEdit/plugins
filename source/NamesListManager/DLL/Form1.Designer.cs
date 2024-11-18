namespace Nikse.SubtitleEdit.PluginLogic
{
    partial class Form1
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
            this.listBoxNames = new System.Windows.Forms.ListBox();
            this.textBoxFileName = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.textBoxNewWord = new System.Windows.Forms.TextBox();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.labelCount = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.buttonImport = new System.Windows.Forms.Button();
            this.listBoxImport = new System.Windows.Forms.ListBox();
            this.labelImportCount = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.labelTransfered = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteCurrentNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.removeNamesContainedInFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBoxNames
            // 
            this.listBoxNames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxNames.FormattingEnabled = true;
            this.listBoxNames.Location = new System.Drawing.Point(12, 91);
            this.listBoxNames.Name = "listBoxNames";
            this.listBoxNames.Size = new System.Drawing.Size(289, 472);
            this.listBoxNames.TabIndex = 0;
            this.listBoxNames.DoubleClick += new System.EventHandler(this.listBoxNames_DoubleClick);
            this.listBoxNames.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxNames_KeyDown);
            // 
            // textBoxFileName
            // 
            this.textBoxFileName.Location = new System.Drawing.Point(13, 39);
            this.textBoxFileName.Name = "textBoxFileName";
            this.textBoxFileName.ReadOnly = true;
            this.textBoxFileName.Size = new System.Drawing.Size(485, 20);
            this.textBoxFileName.TabIndex = 1;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(451, 10);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(47, 23);
            this.buttonBrowse.TabIndex = 2;
            this.buttonBrowse.Text = "...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // textBoxNewWord
            // 
            this.textBoxNewWord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxNewWord.Location = new System.Drawing.Point(13, 589);
            this.textBoxNewWord.Name = "textBoxNewWord";
            this.textBoxNewWord.Size = new System.Drawing.Size(129, 20);
            this.textBoxNewWord.TabIndex = 3;
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAdd.Location = new System.Drawing.Point(148, 589);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(76, 23);
            this.buttonAdd.TabIndex = 4;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // labelCount
            // 
            this.labelCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCount.Location = new System.Drawing.Point(196, 75);
            this.labelCount.Name = "labelCount";
            this.labelCount.Size = new System.Drawing.Size(109, 13);
            this.labelCount.TabIndex = 5;
            this.labelCount.Text = "labelCount";
            this.labelCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(229, 589);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(76, 23);
            this.buttonSave.TabIndex = 6;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // buttonImport
            // 
            this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonImport.Location = new System.Drawing.Point(586, 39);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(122, 23);
            this.buttonImport.TabIndex = 7;
            this.buttonImport.Text = "Import names...";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
            // 
            // listBoxImport
            // 
            this.listBoxImport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxImport.FormattingEnabled = true;
            this.listBoxImport.Location = new System.Drawing.Point(503, 91);
            this.listBoxImport.Name = "listBoxImport";
            this.listBoxImport.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxImport.Size = new System.Drawing.Size(205, 472);
            this.listBoxImport.TabIndex = 8;
            this.listBoxImport.DoubleClick += new System.EventHandler(this.listBoxNames_DoubleClick);
            // 
            // labelImportCount
            // 
            this.labelImportCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelImportCount.Location = new System.Drawing.Point(599, 75);
            this.labelImportCount.Name = "labelImportCount";
            this.labelImportCount.Size = new System.Drawing.Size(109, 13);
            this.labelImportCount.TabIndex = 9;
            this.labelImportCount.Text = "labelImportCount";
            this.labelImportCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(307, 306);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(190, 65);
            this.button1.TabIndex = 10;
            this.button1.Text = "<< Transfer selected";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // labelTransfered
            // 
            this.labelTransfered.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTransfered.AutoSize = true;
            this.labelTransfered.Location = new System.Drawing.Point(308, 378);
            this.labelTransfered.Name = "labelTransfered";
            this.labelTransfered.Size = new System.Drawing.Size(80, 13);
            this.labelTransfered.TabIndex = 11;
            this.labelTransfered.Text = "labelTransfered";
            // 
            // timer1
            // 
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteCurrentNameToolStripMenuItem,
            this.toolStripSeparator1,
            this.removeNamesContainedInFileToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(253, 54);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // deleteCurrentNameToolStripMenuItem
            // 
            this.deleteCurrentNameToolStripMenuItem.Name = "deleteCurrentNameToolStripMenuItem";
            this.deleteCurrentNameToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.deleteCurrentNameToolStripMenuItem.Text = "Delete current name...";
            this.deleteCurrentNameToolStripMenuItem.Click += new System.EventHandler(this.deleteCurrentNameToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(249, 6);
            // 
            // removeNamesContainedInFileToolStripMenuItem
            // 
            this.removeNamesContainedInFileToolStripMenuItem.Name = "removeNamesContainedInFileToolStripMenuItem";
            this.removeNamesContainedInFileToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.removeNamesContainedInFileToolStripMenuItem.Text = "Remove names contained in file...";
            this.removeNamesContainedInFileToolStripMenuItem.Click += new System.EventHandler(this.removeNamesContainedInFileToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 621);
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.labelTransfered);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.labelImportCount);
            this.Controls.Add(this.listBoxImport);
            this.Controls.Add(this.buttonImport);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.labelCount);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.textBoxNewWord);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.textBoxFileName);
            this.Controls.Add(this.listBoxNames);
            this.MinimumSize = new System.Drawing.Size(750, 560);
            this.Name = "Form1";
            this.Text = "SE \"name list\" manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxNames;
        private System.Windows.Forms.TextBox textBoxFileName;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.TextBox textBoxNewWord;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.ListBox listBoxImport;
        private System.Windows.Forms.Label labelImportCount;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label labelTransfered;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem removeNamesContainedInFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteCurrentNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}

