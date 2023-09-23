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
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.checkAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uncheckAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.invertCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.checkBoxNames = new System.Windows.Forms.CheckBox();
            this.labelDesc = new System.Windows.Forms.Label();
            this.buttonConvert = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxStyle = new System.Windows.Forms.ComboBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.checkBoxMoods = new System.Windows.Forms.CheckBox();
            this.groupBoxOptions = new System.Windows.Forms.GroupBox();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.pictureBoxDonate = new System.Windows.Forms.PictureBox();
            this.labelExample = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBoxOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDonate)).BeginInit();
            this.SuspendLayout();
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
            // 
            // uncheckAllToolStripMenuItem
            // 
            this.uncheckAllToolStripMenuItem.Name = "uncheckAllToolStripMenuItem";
            this.uncheckAllToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.uncheckAllToolStripMenuItem.Text = "Uncheck all";
            // 
            // invertCheckToolStripMenuItem
            // 
            this.invertCheckToolStripMenuItem.Name = "invertCheckToolStripMenuItem";
            this.invertCheckToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.invertCheckToolStripMenuItem.Text = "Invert check";
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // deleteLineToolStripMenuItem
            // 
            this.deleteLineToolStripMenuItem.Name = "deleteLineToolStripMenuItem";
            this.deleteLineToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.deleteLineToolStripMenuItem.Text = "Remove Line/s";
            this.deleteLineToolStripMenuItem.Click += new System.EventHandler(this.deleteLineToolStripMenuItem_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // checkBoxNames
            // 
            this.checkBoxNames.AutoSize = true;
            this.checkBoxNames.Checked = true;
            this.checkBoxNames.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxNames.Location = new System.Drawing.Point(16, 33);
            this.checkBoxNames.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxNames.Name = "checkBoxNames";
            this.checkBoxNames.Size = new System.Drawing.Size(233, 19);
            this.checkBoxNames.TabIndex = 3;
            this.checkBoxNames.Text = "Narrator. John: Hello! => JOHN: Hello!";
            this.toolTip1.SetToolTip(this.checkBoxNames, "This will change the Narrator Text\r\nEx: Harry: Hello! -> HARRY: Hello!");
            this.checkBoxNames.UseVisualStyleBackColor = true;
            this.checkBoxNames.CheckedChanged += new System.EventHandler(this.CheckBoxNarrator_CheckedChanged);
            // 
            // labelDesc
            // 
            this.labelDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelDesc.AutoSize = true;
            this.labelDesc.BackColor = System.Drawing.Color.Transparent;
            this.labelDesc.Location = new System.Drawing.Point(16, 556);
            this.labelDesc.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDesc.Name = "labelDesc";
            this.labelDesc.Size = new System.Drawing.Size(97, 15);
            this.labelDesc.TabIndex = 6;
            this.labelDesc.Text = "description label";
            // 
            // buttonConvert
            // 
            this.buttonConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConvert.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonConvert.Location = new System.Drawing.Point(636, 560);
            this.buttonConvert.Margin = new System.Windows.Forms.Padding(4);
            this.buttonConvert.Name = "buttonConvert";
            this.buttonConvert.Size = new System.Drawing.Size(101, 26);
            this.buttonConvert.TabIndex = 0;
            this.buttonConvert.Text = "&OK";
            this.buttonConvert.Click += new System.EventHandler(this.Btn_Run_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Location = new System.Drawing.Point(745, 560);
            this.btn_Cancel.Margin = new System.Windows.Forms.Padding(4);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(101, 26);
            this.btn_Cancel.TabIndex = 1;
            this.btn_Cancel.Text = "&Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.Btn_Cancel_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(665, 84);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Style:";
            // 
            // comboBoxStyle
            // 
            this.comboBoxStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStyle.FormattingEnabled = true;
            this.comboBoxStyle.Location = new System.Drawing.Point(668, 102);
            this.comboBoxStyle.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxStyle.MaxDropDownItems = 5;
            this.comboBoxStyle.Name = "comboBoxStyle";
            this.comboBoxStyle.Size = new System.Drawing.Size(178, 23);
            this.comboBoxStyle.TabIndex = 2;
            this.comboBoxStyle.SelectedIndexChanged += new System.EventHandler(this.ComboBoxStyle_SelectedIndexChanged);
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.BackColor = System.Drawing.Color.Transparent;
            this.linkLabel1.Location = new System.Drawing.Point(16, 574);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(273, 15);
            this.linkLabel1.TabIndex = 7;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Report Bugs | Suggestion: ivandrofly@gmail.com";
            // 
            // checkBoxMoods
            // 
            this.checkBoxMoods.AutoSize = true;
            this.checkBoxMoods.Checked = true;
            this.checkBoxMoods.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMoods.Location = new System.Drawing.Point(16, 60);
            this.checkBoxMoods.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxMoods.Name = "checkBoxMoods";
            this.checkBoxMoods.Size = new System.Drawing.Size(213, 19);
            this.checkBoxMoods.TabIndex = 10;
            this.checkBoxMoods.Text = "Moods. e.g: (foobar) => (FOOBAR)";
            this.checkBoxMoods.UseVisualStyleBackColor = true;
            this.checkBoxMoods.CheckedChanged += new System.EventHandler(this.CheckBoxMoods_CheckedChanged);
            // 
            // groupBoxOptions
            // 
            this.groupBoxOptions.BackColor = System.Drawing.Color.Transparent;
            this.groupBoxOptions.Controls.Add(this.checkBoxNames);
            this.groupBoxOptions.Controls.Add(this.checkBoxMoods);
            this.groupBoxOptions.Location = new System.Drawing.Point(14, 14);
            this.groupBoxOptions.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxOptions.Size = new System.Drawing.Size(559, 111);
            this.groupBoxOptions.TabIndex = 12;
            this.groupBoxOptions.TabStop = false;
            this.groupBoxOptions.Text = "Options:";
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
            // listViewFixes
            // 
            this.listViewFixes.AllowColumnReorder = true;
            this.listViewFixes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFixes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listViewFixes.CheckBoxes = true;
            this.listViewFixes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader1,
            this.columnHeader5,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewFixes.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.GridLines = true;
            this.listViewFixes.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(14, 133);
            this.listViewFixes.Margin = new System.Windows.Forms.Padding(4);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.ShowGroups = false;
            this.listViewFixes.Size = new System.Drawing.Size(832, 349);
            this.listViewFixes.TabIndex = 0;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            this.listViewFixes.Resize += new System.EventHandler(this.ListViewFixes_Resize);
            // 
            // pictureBoxDonate
            // 
            this.pictureBoxDonate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxDonate.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.pictureBoxDonate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxDonate.Image = global::Nikse.SubtitleEdit.PluginLogic.Properties.Resources.paypal1;
            this.pictureBoxDonate.Location = new System.Drawing.Point(758, 14);
            this.pictureBoxDonate.Name = "pictureBoxDonate";
            this.pictureBoxDonate.Size = new System.Drawing.Size(88, 67);
            this.pictureBoxDonate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxDonate.TabIndex = 17;
            this.pictureBoxDonate.TabStop = false;
            // 
            // labelExample
            // 
            this.labelExample.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelExample.AutoSize = true;
            this.labelExample.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelExample.ForeColor = System.Drawing.Color.SeaGreen;
            this.labelExample.Location = new System.Drawing.Point(13, 490);
            this.labelExample.Name = "labelExample";
            this.labelExample.Size = new System.Drawing.Size(59, 15);
            this.labelExample.TabIndex = 18;
            this.labelExample.Text = "Example:";
            // 
            // PluginForm
            // 
            this.AcceptButton = this.buttonConvert;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(861, 599);
            this.Controls.Add(this.labelExample);
            this.Controls.Add(this.pictureBoxDonate);
            this.Controls.Add(this.listViewFixes);
            this.Controls.Add(this.groupBoxOptions);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.comboBoxStyle);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.buttonConvert);
            this.Controls.Add(this.labelDesc);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.Location = new System.Drawing.Point(15, 15);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(877, 319);
            this.Name = "PluginForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBoxOptions.ResumeLayout(false);
            this.groupBoxOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDonate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label labelDesc;
        private System.Windows.Forms.Button buttonConvert;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxStyle;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.CheckBox checkBoxMoods;
        private System.Windows.Forms.CheckBox checkBoxNames;
        private System.Windows.Forms.GroupBox groupBoxOptions;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.PictureBox pictureBoxDonate;

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem checkAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uncheckAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem invertCheckToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteLineToolStripMenuItem;
        private System.Windows.Forms.Label labelExample;
    }
}