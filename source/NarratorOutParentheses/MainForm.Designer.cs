namespace Nikse.SubtitleEdit.PluginLogic
{
    partial class MainForm
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
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeaderApply = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLine = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderActual = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderAfter = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.checkAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uncheckAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.invertCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonGetNames = new System.Windows.Forms.Button();
            this.buttonToNarrator = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonApply = new System.Windows.Forms.Button();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewFixes
            // 
            this.listViewFixes.AllowColumnReorder = true;
            this.listViewFixes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFixes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listViewFixes.CheckBoxes = true;
            this.listViewFixes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderApply,
            this.columnHeaderLine,
            this.columnHeaderActual,
            this.columnHeaderAfter});
            this.listViewFixes.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.GridLines = true;
            this.listViewFixes.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(8, 100);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.ShowGroups = false;
            this.listViewFixes.Size = new System.Drawing.Size(950, 354);
            this.listViewFixes.TabIndex = 1;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderApply
            // 
            this.columnHeaderApply.Text = "Apply";
            this.columnHeaderApply.Width = 45;
            // 
            // columnHeaderLine
            // 
            this.columnHeaderLine.Text = "Line #";
            this.columnHeaderLine.Width = 50;
            // 
            // columnHeaderActual
            // 
            this.columnHeaderActual.Text = "Actual Status";
            this.columnHeaderActual.Width = 425;
            // 
            // columnHeaderAfter
            // 
            this.columnHeaderAfter.Text = "After";
            this.columnHeaderAfter.Width = 420;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkAllToolStripMenuItem,
            this.uncheckAllToolStripMenuItem,
            this.invertCheckToolStripMenuItem,
            this.copyToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(139, 92);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // checkAllToolStripMenuItem
            // 
            this.checkAllToolStripMenuItem.Name = "checkAllToolStripMenuItem";
            this.checkAllToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.checkAllToolStripMenuItem.Text = "Check all";
            this.checkAllToolStripMenuItem.Click += new System.EventHandler(this.checkAllToolStripMenuItem_Click);
            // 
            // uncheckAllToolStripMenuItem
            // 
            this.uncheckAllToolStripMenuItem.Name = "uncheckAllToolStripMenuItem";
            this.uncheckAllToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.uncheckAllToolStripMenuItem.Text = "Uncheck all";
            this.uncheckAllToolStripMenuItem.Click += new System.EventHandler(this.checkAllToolStripMenuItem_Click);
            // 
            // invertCheckToolStripMenuItem
            // 
            this.invertCheckToolStripMenuItem.Name = "invertCheckToolStripMenuItem";
            this.invertCheckToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.invertCheckToolStripMenuItem.Text = "Invert check";
            this.invertCheckToolStripMenuItem.Click += new System.EventHandler(this.checkAllToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.checkAllToolStripMenuItem_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.buttonGetNames);
            this.groupBox1.Controls.Add(this.buttonToNarrator);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBoxName);
            this.groupBox1.Location = new System.Drawing.Point(8, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(950, 81);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Misc:";
            // 
            // buttonGetNames
            // 
            this.buttonGetNames.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonGetNames.Location = new System.Drawing.Point(778, 52);
            this.buttonGetNames.Name = "buttonGetNames";
            this.buttonGetNames.Size = new System.Drawing.Size(166, 23);
            this.buttonGetNames.TabIndex = 3;
            this.buttonGetNames.Text = "Extract names from subtitile";
            this.buttonGetNames.UseVisualStyleBackColor = true;
            this.buttonGetNames.Click += new System.EventHandler(this.buttonGetNames_Click);
            // 
            // buttonToNarrator
            // 
            this.buttonToNarrator.Location = new System.Drawing.Point(197, 45);
            this.buttonToNarrator.Name = "buttonToNarrator";
            this.buttonToNarrator.Size = new System.Drawing.Size(52, 23);
            this.buttonToNarrator.TabIndex = 2;
            this.buttonToNarrator.Text = "A&dd";
            this.buttonToNarrator.UseVisualStyleBackColor = true;
            this.buttonToNarrator.Click += new System.EventHandler(this.buttonToNarrator_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Add a name to list:";
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(15, 47);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(176, 20);
            this.textBoxName.TabIndex = 0;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(746, 460);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(103, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(855, 460);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(103, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonApply
            // 
            this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApply.Location = new System.Drawing.Point(637, 460);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(103, 23);
            this.buttonApply.TabIndex = 3;
            this.buttonApply.Text = "&Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(970, 491);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listViewFixes);
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MainForm";
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeaderApply;
        private System.Windows.Forms.ColumnHeader columnHeaderLine;
        private System.Windows.Forms.ColumnHeader columnHeaderActual;
        private System.Windows.Forms.ColumnHeader columnHeaderAfter;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonToNarrator;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonGetNames;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem checkAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uncheckAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem invertCheckToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;

    }
}