namespace WebViewTranslate
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeaderNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSource = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderTarget = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxLanguageFrom = new System.Windows.Forms.ComboBox();
            this.buttonTranslate = new System.Windows.Forms.Button();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.buttonCancelTranslate = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.comboBoxLanguageTo = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownMaxBytes = new System.Windows.Forms.NumericUpDown();
            this.checkBoxAutoCopyToClipboard = new System.Windows.Forms.CheckBox();
            this.labelLineSeparator = new System.Windows.Forms.Label();
            this.textBoxLineSeparator = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxBytes)).BeginInit();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderNumber,
            this.columnHeaderSource,
            this.columnHeaderTarget});
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(15, 91);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(876, 428);
            this.listView1.TabIndex = 11;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.Resize += new System.EventHandler(this.listView1_Resize);
            // 
            // columnHeaderNumber
            // 
            this.columnHeaderNumber.Text = "Line #";
            this.columnHeaderNumber.Width = 48;
            // 
            // columnHeaderSource
            // 
            this.columnHeaderSource.Text = "Original";
            this.columnHeaderSource.Width = 350;
            // 
            // columnHeaderTarget
            // 
            this.columnHeaderTarget.Text = "Translation";
            this.columnHeaderTarget.Width = 335;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(735, 525);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 12;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(816, 525);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 13;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Translation language pair:";
            // 
            // comboBoxLanguageFrom
            // 
            this.comboBoxLanguageFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguageFrom.FormattingEnabled = true;
            this.comboBoxLanguageFrom.Location = new System.Drawing.Point(147, 12);
            this.comboBoxLanguageFrom.Name = "comboBoxLanguageFrom";
            this.comboBoxLanguageFrom.Size = new System.Drawing.Size(112, 21);
            this.comboBoxLanguageFrom.TabIndex = 1;
            // 
            // buttonTranslate
            // 
            this.buttonTranslate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTranslate.Location = new System.Drawing.Point(383, 10);
            this.buttonTranslate.Name = "buttonTranslate";
            this.buttonTranslate.Size = new System.Drawing.Size(113, 23);
            this.buttonTranslate.TabIndex = 3;
            this.buttonTranslate.Text = "Translate";
            this.buttonTranslate.UseVisualStyleBackColor = true;
            this.buttonTranslate.Click += new System.EventHandler(this.buttonTranslate_Click);
            // 
            // textBoxLog
            // 
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.Location = new System.Drawing.Point(15, 91);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLog.Size = new System.Drawing.Size(876, 428);
            this.textBoxLog.TabIndex = 5;
            // 
            // buttonCancelTranslate
            // 
            this.buttonCancelTranslate.Location = new System.Drawing.Point(502, 10);
            this.buttonCancelTranslate.Name = "buttonCancelTranslate";
            this.buttonCancelTranslate.Size = new System.Drawing.Size(99, 23);
            this.buttonCancelTranslate.TabIndex = 4;
            this.buttonCancelTranslate.Text = "Cancel";
            this.buttonCancelTranslate.UseVisualStyleBackColor = true;
            this.buttonCancelTranslate.Click += new System.EventHandler(this.buttonCancelTranslate_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(607, 11);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(212, 23);
            this.progressBar1.TabIndex = 5;
            this.progressBar1.Visible = false;
            // 
            // comboBoxLanguageTo
            // 
            this.comboBoxLanguageTo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguageTo.FormattingEnabled = true;
            this.comboBoxLanguageTo.Location = new System.Drawing.Point(265, 11);
            this.comboBoxLanguageTo.Name = "comboBoxLanguageTo";
            this.comboBoxLanguageTo.Size = new System.Drawing.Size(112, 21);
            this.comboBoxLanguageTo.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label3.Location = new System.Drawing.Point(72, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Max text size";
            // 
            // numericUpDownMaxBytes
            // 
            this.numericUpDownMaxBytes.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownMaxBytes.Location = new System.Drawing.Point(147, 46);
            this.numericUpDownMaxBytes.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownMaxBytes.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownMaxBytes.Name = "numericUpDownMaxBytes";
            this.numericUpDownMaxBytes.Size = new System.Drawing.Size(57, 20);
            this.numericUpDownMaxBytes.TabIndex = 7;
            this.numericUpDownMaxBytes.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            // 
            // checkBoxAutoCopyToClipboard
            // 
            this.checkBoxAutoCopyToClipboard.AutoSize = true;
            this.checkBoxAutoCopyToClipboard.Checked = true;
            this.checkBoxAutoCopyToClipboard.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoCopyToClipboard.Location = new System.Drawing.Point(230, 48);
            this.checkBoxAutoCopyToClipboard.Name = "checkBoxAutoCopyToClipboard";
            this.checkBoxAutoCopyToClipboard.Size = new System.Drawing.Size(132, 17);
            this.checkBoxAutoCopyToClipboard.TabIndex = 8;
            this.checkBoxAutoCopyToClipboard.Text = "Auto-copy to clipboard";
            this.checkBoxAutoCopyToClipboard.UseVisualStyleBackColor = true;
            // 
            // labelLineSeparator
            // 
            this.labelLineSeparator.AutoSize = true;
            this.labelLineSeparator.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelLineSeparator.Location = new System.Drawing.Point(380, 49);
            this.labelLineSeparator.Name = "labelLineSeparator";
            this.labelLineSeparator.Size = new System.Drawing.Size(74, 13);
            this.labelLineSeparator.TabIndex = 9;
            this.labelLineSeparator.Text = "Line separator";
            // 
            // textBoxLineSeparator
            // 
            this.textBoxLineSeparator.Location = new System.Drawing.Point(455, 45);
            this.textBoxLineSeparator.Name = "textBoxLineSeparator";
            this.textBoxLineSeparator.Size = new System.Drawing.Size(41, 20);
            this.textBoxLineSeparator.TabIndex = 10;
            this.textBoxLineSeparator.Text = ".";
            // 
            // MainForm
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(903, 556);
            this.Controls.Add(this.textBoxLineSeparator);
            this.Controls.Add(this.labelLineSeparator);
            this.Controls.Add(this.checkBoxAutoCopyToClipboard);
            this.Controls.Add(this.numericUpDownMaxBytes);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBoxLanguageTo);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonCancelTranslate);
            this.Controls.Add(this.buttonTranslate);
            this.Controls.Add(this.comboBoxLanguageFrom);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.textBoxLog);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(870, 500);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxBytes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeaderNumber;
        private System.Windows.Forms.ColumnHeader columnHeaderSource;
        private System.Windows.Forms.ColumnHeader columnHeaderTarget;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxLanguageFrom;
        private System.Windows.Forms.Button buttonTranslate;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Button buttonCancelTranslate;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ComboBox comboBoxLanguageTo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxBytes;
        private System.Windows.Forms.CheckBox checkBoxAutoCopyToClipboard;
        private System.Windows.Forms.Label labelLineSeparator;
        private System.Windows.Forms.TextBox textBoxLineSeparator;
    }
}