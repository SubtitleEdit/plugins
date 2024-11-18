namespace ColorToDialog
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
            this.labelDialogDash = new System.Windows.Forms.Label();
            this.comboBoxDash = new System.Windows.Forms.ComboBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.labelTextAfter = new System.Windows.Forms.Label();
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
            this.listView1.Location = new System.Drawing.Point(15, 40);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(939, 393);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
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
            this.columnHeaderTarget.Text = "After";
            this.columnHeaderTarget.Width = 335;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(798, 525);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 7;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(879, 525);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelDialogDash
            // 
            this.labelDialogDash.AutoSize = true;
            this.labelDialogDash.Location = new System.Drawing.Point(12, 15);
            this.labelDialogDash.Name = "labelDialogDash";
            this.labelDialogDash.Size = new System.Drawing.Size(66, 13);
            this.labelDialogDash.TabIndex = 0;
            this.labelDialogDash.Text = "Dialog dash:";
            // 
            // comboBoxDash
            // 
            this.comboBoxDash.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDash.FormattingEnabled = true;
            this.comboBoxDash.Items.AddRange(new object[] {
            "Dash on all lines, with space",
            "Dash on all lines, without space"});
            this.comboBoxDash.Location = new System.Drawing.Point(84, 12);
            this.comboBoxDash.Name = "comboBoxDash";
            this.comboBoxDash.Size = new System.Drawing.Size(226, 21);
            this.comboBoxDash.TabIndex = 1;
            this.comboBoxDash.SelectedIndexChanged += new System.EventHandler(this.comboBoxDash_SelectedIndexChanged);
            this.comboBoxDash.TextChanged += new System.EventHandler(this.comboBoxDash_TextChanged);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox1.Location = new System.Drawing.Point(15, 472);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(532, 69);
            this.textBox1.TabIndex = 9;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // labelTextAfter
            // 
            this.labelTextAfter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTextAfter.AutoSize = true;
            this.labelTextAfter.Location = new System.Drawing.Point(15, 453);
            this.labelTextAfter.Name = "labelTextAfter";
            this.labelTextAfter.Size = new System.Drawing.Size(52, 13);
            this.labelTextAfter.TabIndex = 10;
            this.labelTextAfter.Text = "Text after";
            // 
            // MainForm
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(966, 556);
            this.Controls.Add(this.labelTextAfter);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.comboBoxDash);
            this.Controls.Add(this.labelDialogDash);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.listView1);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(870, 500);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Color to dialog";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
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
        private System.Windows.Forms.Label labelDialogDash;
        private System.Windows.Forms.ComboBox comboBoxDash;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label labelTextAfter;
    }
}