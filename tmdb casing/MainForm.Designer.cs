namespace Nikse
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
            this.labelStatus = new System.Windows.Forms.Label();
            this.listViewParagraphs = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listViewNames = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBoxFindwhat = new System.Windows.Forms.TextBox();
            this.textBoxReplacewith = new System.Windows.Forms.TextBox();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.radioButtonRegex = new System.Windows.Forms.RadioButton();
            this.radioButtonCaseSensitive = new System.Windows.Forms.RadioButton();
            this.radioButtonNormal = new System.Windows.Forms.RadioButton();
            this.groupBoxReplaceOptions = new System.Windows.Forms.GroupBox();
            this.groupBoxReplaceOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(-110, 27);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(57, 13);
            this.labelStatus.TabIndex = 10;
            this.labelStatus.Text = "Status: {0}";
            // 
            // listViewParagraphs
            // 
            this.listViewParagraphs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewParagraphs.CheckBoxes = true;
            this.listViewParagraphs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.listViewParagraphs.FullRowSelect = true;
            this.listViewParagraphs.HideSelection = false;
            this.listViewParagraphs.Location = new System.Drawing.Point(22, 209);
            this.listViewParagraphs.Name = "listViewParagraphs";
            this.listViewParagraphs.Size = new System.Drawing.Size(794, 221);
            this.listViewParagraphs.TabIndex = 9;
            this.listViewParagraphs.UseCompatibleStateImageBehavior = false;
            this.listViewParagraphs.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Apply";
            this.columnHeader3.Width = 38;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Line #";
            this.columnHeader4.Width = 42;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Before";
            this.columnHeader5.Width = 340;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "After";
            this.columnHeader6.Width = 365;
            // 
            // listViewNames
            // 
            this.listViewNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewNames.CheckBoxes = true;
            this.listViewNames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listViewNames.FullRowSelect = true;
            this.listViewNames.GridLines = true;
            this.listViewNames.Location = new System.Drawing.Point(22, 23);
            this.listViewNames.Name = "listViewNames";
            this.listViewNames.Size = new System.Drawing.Size(489, 180);
            this.listViewNames.TabIndex = 8;
            this.listViewNames.UseCompatibleStateImageBehavior = false;
            this.listViewNames.View = System.Windows.Forms.View.Details;
            this.listViewNames.SelectedIndexChanged += new System.EventHandler(this.listViewNames_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Enabled";
            this.columnHeader1.Width = 61;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Name";
            this.columnHeader2.Width = 412;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(598, 436);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(106, 23);
            this.buttonOK.TabIndex = 11;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(710, 436);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(106, 23);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // textBoxFindwhat
            // 
            this.textBoxFindwhat.Location = new System.Drawing.Point(9, 44);
            this.textBoxFindwhat.Name = "textBoxFindwhat";
            this.textBoxFindwhat.Size = new System.Drawing.Size(269, 20);
            this.textBoxFindwhat.TabIndex = 13;
            // 
            // textBoxReplacewith
            // 
            this.textBoxReplacewith.Location = new System.Drawing.Point(9, 90);
            this.textBoxReplacewith.Name = "textBoxReplacewith";
            this.textBoxReplacewith.Size = new System.Drawing.Size(269, 20);
            this.textBoxReplacewith.TabIndex = 14;
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Location = new System.Drawing.Point(172, 151);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(106, 23);
            this.buttonUpdate.TabIndex = 15;
            this.buttonUpdate.Text = "Update";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Find what?!";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Replace with:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 436);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "Total paragraphs: {0}";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(85, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "Total names: {0}";
            // 
            // radioButtonRegex
            // 
            this.radioButtonRegex.AutoSize = true;
            this.radioButtonRegex.Location = new System.Drawing.Point(107, 116);
            this.radioButtonRegex.Name = "radioButtonRegex";
            this.radioButtonRegex.Size = new System.Drawing.Size(116, 17);
            this.radioButtonRegex.TabIndex = 20;
            this.radioButtonRegex.TabStop = true;
            this.radioButtonRegex.Text = "Regular Expression";
            this.radioButtonRegex.UseVisualStyleBackColor = true;
            // 
            // radioButtonCaseSensitive
            // 
            this.radioButtonCaseSensitive.AutoSize = true;
            this.radioButtonCaseSensitive.Location = new System.Drawing.Point(6, 116);
            this.radioButtonCaseSensitive.Name = "radioButtonCaseSensitive";
            this.radioButtonCaseSensitive.Size = new System.Drawing.Size(95, 17);
            this.radioButtonCaseSensitive.TabIndex = 21;
            this.radioButtonCaseSensitive.TabStop = true;
            this.radioButtonCaseSensitive.Text = "Case Sensitive";
            this.radioButtonCaseSensitive.UseVisualStyleBackColor = true;
            // 
            // radioButtonNormal
            // 
            this.radioButtonNormal.AutoSize = true;
            this.radioButtonNormal.Location = new System.Drawing.Point(6, 139);
            this.radioButtonNormal.Name = "radioButtonNormal";
            this.radioButtonNormal.Size = new System.Drawing.Size(58, 17);
            this.radioButtonNormal.TabIndex = 22;
            this.radioButtonNormal.TabStop = true;
            this.radioButtonNormal.Text = "Normal";
            this.radioButtonNormal.UseVisualStyleBackColor = true;
            // 
            // groupBoxReplaceOptions
            // 
            this.groupBoxReplaceOptions.Controls.Add(this.radioButtonRegex);
            this.groupBoxReplaceOptions.Controls.Add(this.radioButtonNormal);
            this.groupBoxReplaceOptions.Controls.Add(this.textBoxFindwhat);
            this.groupBoxReplaceOptions.Controls.Add(this.radioButtonCaseSensitive);
            this.groupBoxReplaceOptions.Controls.Add(this.textBoxReplacewith);
            this.groupBoxReplaceOptions.Controls.Add(this.buttonUpdate);
            this.groupBoxReplaceOptions.Controls.Add(this.label1);
            this.groupBoxReplaceOptions.Controls.Add(this.label2);
            this.groupBoxReplaceOptions.Location = new System.Drawing.Point(517, 23);
            this.groupBoxReplaceOptions.Name = "groupBoxReplaceOptions";
            this.groupBoxReplaceOptions.Size = new System.Drawing.Size(299, 180);
            this.groupBoxReplaceOptions.TabIndex = 23;
            this.groupBoxReplaceOptions.TabStop = false;
            this.groupBoxReplaceOptions.Text = "Replace options:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(839, 469);
            this.Controls.Add(this.groupBoxReplaceOptions);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.listViewParagraphs);
            this.Controls.Add(this.listViewNames);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MainForm";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.groupBoxReplaceOptions.ResumeLayout(false);
            this.groupBoxReplaceOptions.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ListView listViewParagraphs;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ListView listViewNames;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textBoxFindwhat;
        private System.Windows.Forms.TextBox textBoxReplacewith;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton radioButtonRegex;
        private System.Windows.Forms.RadioButton radioButtonCaseSensitive;
        private System.Windows.Forms.RadioButton radioButtonNormal;
        private System.Windows.Forms.GroupBox groupBoxReplaceOptions;

    }
}