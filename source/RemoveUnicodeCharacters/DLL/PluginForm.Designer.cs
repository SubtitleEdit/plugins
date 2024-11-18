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
            this.buttonOK = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.labelDescription = new System.Windows.Forms.Label();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.labelTotal = new System.Windows.Forms.Label();
            this.buttonInverseSelection = new System.Windows.Forms.Button();
            this.buttonSelectAll = new System.Windows.Forms.Button();
            this.checkBoxUseReplaceList = new System.Windows.Forms.CheckBox();
            this.buttonEditReplaceList = new System.Windows.Forms.Button();
            this.buttonGoogleIt = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(795, 472);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(876, 472);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
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
            this.columnHeader8,
            this.columnHeader1});
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.GridLines = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(12, 46);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(939, 399);
            this.listViewFixes.TabIndex = 0;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            this.listViewFixes.SelectedIndexChanged += new System.EventHandler(this.listViewFixes_SelectedIndexChanged);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Remove";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Count";
            this.columnHeader5.Width = 50;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Unicode character";
            this.columnHeader6.Width = 150;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Before";
            this.columnHeader7.Width = 50;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "After";
            this.columnHeader8.Width = 50;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Lines";
            this.columnHeader1.Width = 575;
            // 
            // labelTotal
            // 
            this.labelTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTotal.AutoSize = true;
            this.labelTotal.Location = new System.Drawing.Point(12, 485);
            this.labelTotal.Name = "labelTotal";
            this.labelTotal.Size = new System.Drawing.Size(34, 13);
            this.labelTotal.TabIndex = 102;
            this.labelTotal.Text = "Total:";
            // 
            // buttonInverseSelection
            // 
            this.buttonInverseSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonInverseSelection.Location = new System.Drawing.Point(93, 451);
            this.buttonInverseSelection.Name = "buttonInverseSelection";
            this.buttonInverseSelection.Size = new System.Drawing.Size(105, 23);
            this.buttonInverseSelection.TabIndex = 2;
            this.buttonInverseSelection.Text = "Inverse selection";
            this.buttonInverseSelection.UseVisualStyleBackColor = true;
            this.buttonInverseSelection.Click += new System.EventHandler(this.buttonInverseSelection_Click);
            // 
            // buttonSelectAll
            // 
            this.buttonSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSelectAll.Location = new System.Drawing.Point(12, 451);
            this.buttonSelectAll.Name = "buttonSelectAll";
            this.buttonSelectAll.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectAll.TabIndex = 1;
            this.buttonSelectAll.Text = "Select all";
            this.buttonSelectAll.UseVisualStyleBackColor = true;
            this.buttonSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
            // 
            // checkBoxUseReplaceList
            // 
            this.checkBoxUseReplaceList.AutoSize = true;
            this.checkBoxUseReplaceList.Location = new System.Drawing.Point(729, 21);
            this.checkBoxUseReplaceList.Name = "checkBoxUseReplaceList";
            this.checkBoxUseReplaceList.Size = new System.Drawing.Size(98, 17);
            this.checkBoxUseReplaceList.TabIndex = 6;
            this.checkBoxUseReplaceList.Text = "Use replace list";
            this.checkBoxUseReplaceList.UseVisualStyleBackColor = true;
            this.checkBoxUseReplaceList.CheckedChanged += new System.EventHandler(this.checkBoxUseReplaceList_CheckedChanged);
            // 
            // buttonEditReplaceList
            // 
            this.buttonEditReplaceList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEditReplaceList.Location = new System.Drawing.Point(833, 17);
            this.buttonEditReplaceList.Name = "buttonEditReplaceList";
            this.buttonEditReplaceList.Size = new System.Drawing.Size(118, 23);
            this.buttonEditReplaceList.TabIndex = 7;
            this.buttonEditReplaceList.Text = "Edit replace list";
            this.buttonEditReplaceList.UseVisualStyleBackColor = true;
            this.buttonEditReplaceList.Click += new System.EventHandler(this.EditReplaceList_Click);
            // 
            // buttonGoogleIt
            // 
            this.buttonGoogleIt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonGoogleIt.Location = new System.Drawing.Point(204, 451);
            this.buttonGoogleIt.Name = "buttonGoogleIt";
            this.buttonGoogleIt.Size = new System.Drawing.Size(162, 23);
            this.buttonGoogleIt.TabIndex = 3;
            this.buttonGoogleIt.Text = "Google it";
            this.buttonGoogleIt.UseVisualStyleBackColor = true;
            this.buttonGoogleIt.Click += new System.EventHandler(this.buttonGoogleIt_Click);
            // 
            // PluginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(963, 507);
            this.Controls.Add(this.buttonGoogleIt);
            this.Controls.Add(this.buttonEditReplaceList);
            this.Controls.Add(this.checkBoxUseReplaceList);
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
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PluginForm_FormClosing);
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
        private System.Windows.Forms.CheckBox checkBoxUseReplaceList;
        private System.Windows.Forms.Button buttonEditReplaceList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button buttonGoogleIt;
    }
}