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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PluginForm));
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.checkAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uncheckAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.invertCheckedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.linkLabelGithub = new System.Windows.Forms.LinkLabel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxSkipDialog = new System.Windows.Forms.CheckBox();
            this.checkBoxSkipNarrator = new System.Windows.Forms.CheckBox();
            this.checkBoxMoods = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.labelTotal = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reportProblemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBoxDonate = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDonate)).BeginInit();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.CheckBoxes = true;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader5,
            this.columnHeader3,
            this.columnHeader4});
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(7, 168);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(891, 340);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.Resize += new System.EventHandler(this.ListView1_Resize);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Accept";
            this.columnHeader1.Width = 54;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Line#";
            this.columnHeader2.Width = 52;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Length";
            this.columnHeader5.Width = 45;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Before";
            this.columnHeader3.Width = 334;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "After";
            this.columnHeader4.Width = 350;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkAllToolStripMenuItem,
            this.uncheckAllToolStripMenuItem,
            this.invertCheckedToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(152, 70);
            // 
            // checkAllToolStripMenuItem
            // 
            this.checkAllToolStripMenuItem.Name = "checkAllToolStripMenuItem";
            this.checkAllToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.checkAllToolStripMenuItem.Text = "Check all";
            this.checkAllToolStripMenuItem.Click += new System.EventHandler(this.CheckAllToolStripMenuItem_Click);
            // 
            // uncheckAllToolStripMenuItem
            // 
            this.uncheckAllToolStripMenuItem.Name = "uncheckAllToolStripMenuItem";
            this.uncheckAllToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.uncheckAllToolStripMenuItem.Text = "Uncheck all";
            this.uncheckAllToolStripMenuItem.Click += new System.EventHandler(this.UncheckAllToolStripMenuItem_Click);
            // 
            // invertCheckedToolStripMenuItem
            // 
            this.invertCheckedToolStripMenuItem.Name = "invertCheckedToolStripMenuItem";
            this.invertCheckedToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.invertCheckedToolStripMenuItem.Text = "Invert checked";
            this.invertCheckedToolStripMenuItem.Click += new System.EventHandler(this.InvertCheckedToolStripMenuItem_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(797, 514);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(101, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(690, 514);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(101, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.pictureBoxDonate);
            this.groupBox1.Controls.Add(this.linkLabelGithub);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Location = new System.Drawing.Point(7, 32);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(891, 130);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            // 
            // linkLabelGithub
            // 
            this.linkLabelGithub.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelGithub.AutoSize = true;
            this.linkLabelGithub.Location = new System.Drawing.Point(822, 106);
            this.linkLabelGithub.Name = "linkLabelGithub";
            this.linkLabelGithub.Size = new System.Drawing.Size(63, 13);
            this.linkLabelGithub.TabIndex = 12;
            this.linkLabelGithub.TabStop = true;
            this.linkLabelGithub.Tag = "https://github.com/ivandrofly";
            this.linkLabelGithub.Text = "@ivandrofly";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.groupBox2.Controls.Add(this.checkBoxSkipDialog);
            this.groupBox2.Controls.Add(this.checkBoxSkipNarrator);
            this.groupBox2.Controls.Add(this.checkBoxMoods);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.numericUpDown1);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(6, 19);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(647, 100);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Unbreak Rules:";
            // 
            // checkBoxSkipDialog
            // 
            this.checkBoxSkipDialog.AutoSize = true;
            this.checkBoxSkipDialog.Checked = true;
            this.checkBoxSkipDialog.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSkipDialog.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxSkipDialog.Location = new System.Drawing.Point(14, 70);
            this.checkBoxSkipDialog.Name = "checkBoxSkipDialog";
            this.checkBoxSkipDialog.Size = new System.Drawing.Size(98, 21);
            this.checkBoxSkipDialog.TabIndex = 6;
            this.checkBoxSkipDialog.Text = "Skip Dialog";
            this.checkBoxSkipDialog.UseVisualStyleBackColor = true;
            // 
            // checkBoxSkipNarrator
            // 
            this.checkBoxSkipNarrator.AutoSize = true;
            this.checkBoxSkipNarrator.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxSkipNarrator.Location = new System.Drawing.Point(14, 22);
            this.checkBoxSkipNarrator.Name = "checkBoxSkipNarrator";
            this.checkBoxSkipNarrator.Size = new System.Drawing.Size(111, 21);
            this.checkBoxSkipNarrator.TabIndex = 7;
            this.checkBoxSkipNarrator.Text = "Skip Narrator";
            this.checkBoxSkipNarrator.UseVisualStyleBackColor = true;
            // 
            // checkBoxMoods
            // 
            this.checkBoxMoods.AutoSize = true;
            this.checkBoxMoods.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxMoods.Location = new System.Drawing.Point(14, 46);
            this.checkBoxMoods.Name = "checkBoxMoods";
            this.checkBoxMoods.Size = new System.Drawing.Size(100, 21);
            this.checkBoxMoods.TabIndex = 8;
            this.checkBoxMoods.Text = "Skip Moods";
            this.checkBoxMoods.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(173, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(292, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Unbreak lines break shorter than or equal to:";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(176, 68);
            this.numericUpDown1.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(81, 23);
            this.numericUpDown1.TabIndex = 3;
            this.numericUpDown1.Value = new decimal(new int[] {
            35,
            0,
            0,
            0});
            // 
            // labelTotal
            // 
            this.labelTotal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTotal.AutoSize = true;
            this.labelTotal.Location = new System.Drawing.Point(5, 519);
            this.labelTotal.Name = "labelTotal";
            this.labelTotal.Size = new System.Drawing.Size(51, 13);
            this.labelTotal.TabIndex = 4;
            this.labelTotal.Text = "Total: {0}";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(910, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.reportProblemToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // reportProblemToolStripMenuItem
            // 
            this.reportProblemToolStripMenuItem.Name = "reportProblemToolStripMenuItem";
            this.reportProblemToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.reportProblemToolStripMenuItem.Text = "Report problem / request feature";
            this.reportProblemToolStripMenuItem.Click += new System.EventHandler(this.ReportProblemToolStripMenuItem_Click);
            // 
            // pictureBoxDonate
            // 
            this.pictureBoxDonate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxDonate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxDonate.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxDonate.Image")));
            this.pictureBoxDonate.Location = new System.Drawing.Point(785, 19);
            this.pictureBoxDonate.Name = "pictureBoxDonate";
            this.pictureBoxDonate.Size = new System.Drawing.Size(100, 67);
            this.pictureBoxDonate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxDonate.TabIndex = 18;
            this.pictureBoxDonate.TabStop = false;
            // 
            // PluginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(910, 544);
            this.Controls.Add(this.labelTotal);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.menuStrip1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(820, 583);
            this.Name = "PluginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Lines Unbreaker";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PluginForm_KeyDown);
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDonate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label labelTotal;
        private System.Windows.Forms.CheckBox checkBoxSkipDialog;
        private System.Windows.Forms.CheckBox checkBoxMoods;
        private System.Windows.Forms.CheckBox checkBoxSkipNarrator;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.LinkLabel linkLabelGithub;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reportProblemToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem checkAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uncheckAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem invertCheckedToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBoxDonate;
    }
}