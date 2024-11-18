namespace Nikse.SubtitleEdit.PluginLogic
{
    partial class ManageWordsForm
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeaderAmerican = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBritish = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemRemoveSelected = new System.Windows.Forms.ToolStripMenuItem();
            this.labelSource = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelTotalWords = new System.Windows.Forms.Label();
            this.labelAmerican = new System.Windows.Forms.Label();
            this.labelBritish = new System.Windows.Forms.Label();
            this.textBoxAmerican = new System.Windows.Forms.TextBox();
            this.textBoxBritish = new System.Windows.Forms.TextBox();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderAmerican,
            this.columnHeaderBritish});
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 40);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(620, 283);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listView1_KeyDown);
            // 
            // columnHeaderAmerican
            // 
            this.columnHeaderAmerican.Text = "American";
            this.columnHeaderAmerican.Width = 300;
            // 
            // columnHeaderBritish
            // 
            this.columnHeaderBritish.Text = "British";
            this.columnHeaderBritish.Width = 299;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemRemoveSelected});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(164, 26);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // toolStripMenuItemRemoveSelected
            // 
            this.toolStripMenuItemRemoveSelected.Name = "toolStripMenuItemRemoveSelected";
            this.toolStripMenuItemRemoveSelected.Size = new System.Drawing.Size(163, 22);
            this.toolStripMenuItemRemoveSelected.Text = "Remove selected";
            this.toolStripMenuItemRemoveSelected.Click += new System.EventHandler(this.toolStripMenuItemRemoveSelected_Click);
            // 
            // labelSource
            // 
            this.labelSource.AutoSize = true;
            this.labelSource.Location = new System.Drawing.Point(13, 20);
            this.labelSource.Name = "labelSource";
            this.labelSource.Size = new System.Drawing.Size(44, 13);
            this.labelSource.TabIndex = 1;
            this.labelSource.Text = "Source:";
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(557, 332);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelTotalWords
            // 
            this.labelTotalWords.AutoSize = true;
            this.labelTotalWords.Location = new System.Drawing.Point(12, 332);
            this.labelTotalWords.Name = "labelTotalWords";
            this.labelTotalWords.Size = new System.Drawing.Size(65, 13);
            this.labelTotalWords.TabIndex = 3;
            this.labelTotalWords.Text = "Total words:";
            // 
            // labelAmerican
            // 
            this.labelAmerican.AutoSize = true;
            this.labelAmerican.Location = new System.Drawing.Point(220, 14);
            this.labelAmerican.Name = "labelAmerican";
            this.labelAmerican.Size = new System.Drawing.Size(54, 13);
            this.labelAmerican.TabIndex = 4;
            this.labelAmerican.Text = "American:";
            // 
            // labelBritish
            // 
            this.labelBritish.AutoSize = true;
            this.labelBritish.Location = new System.Drawing.Point(408, 14);
            this.labelBritish.Name = "labelBritish";
            this.labelBritish.Size = new System.Drawing.Size(38, 13);
            this.labelBritish.TabIndex = 5;
            this.labelBritish.Text = "British:";
            // 
            // textBoxAmerican
            // 
            this.textBoxAmerican.Location = new System.Drawing.Point(280, 11);
            this.textBoxAmerican.Name = "textBoxAmerican";
            this.textBoxAmerican.Size = new System.Drawing.Size(125, 20);
            this.textBoxAmerican.TabIndex = 6;
            this.textBoxAmerican.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxBritishAmerican_KeyDown);
            // 
            // textBoxBritish
            // 
            this.textBoxBritish.Location = new System.Drawing.Point(451, 11);
            this.textBoxBritish.Name = "textBoxBritish";
            this.textBoxBritish.Size = new System.Drawing.Size(125, 20);
            this.textBoxBritish.TabIndex = 7;
            this.textBoxBritish.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxBritishAmerican_KeyDown);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(585, 10);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(47, 21);
            this.buttonAdd.TabIndex = 8;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // ManageWordsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(654, 366);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.textBoxBritish);
            this.Controls.Add(this.textBoxAmerican);
            this.Controls.Add(this.labelBritish);
            this.Controls.Add(this.labelAmerican);
            this.Controls.Add(this.labelTotalWords);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelSource);
            this.Controls.Add(this.listView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(660, 394);
            this.Name = "ManageWordsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Word list manager";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ManageWordsForm_FormClosed);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ManageWordsForm_KeyDown);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeaderAmerican;
        private System.Windows.Forms.ColumnHeader columnHeaderBritish;
        private System.Windows.Forms.Label labelSource;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelTotalWords;
        private System.Windows.Forms.Label labelAmerican;
        private System.Windows.Forms.Label labelBritish;
        private System.Windows.Forms.TextBox textBoxAmerican;
        private System.Windows.Forms.TextBox textBoxBritish;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRemoveSelected;
    }
}