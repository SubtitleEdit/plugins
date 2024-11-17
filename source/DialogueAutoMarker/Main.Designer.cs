namespace Nikse.SubtitleEdit.PluginLogic
{
    partial class Main
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
            this.listViewDialogue = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonPrev = new System.Windows.Forms.Button();
            this.buttonNext = new System.Windows.Forms.Button();
            this.buttonDash = new System.Windows.Forms.Button();
            this.buttonItalic = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.buttonSingleDash = new System.Windows.Forms.Button();
            this.buttonBold = new System.Windows.Forms.Button();
            this.buttonUpper = new System.Windows.Forms.Button();
            this.buttonLower = new System.Windows.Forms.Button();
            this.buttonBreakCursorPos = new System.Windows.Forms.Button();
            this.buttonUnBreak = new System.Windows.Forms.Button();
            this.labelSingleLineLength = new System.Windows.Forms.Label();
            this.labelLinesTotalLength = new System.Windows.Forms.Label();
            this.checkBoxColorChanged = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // listViewDialogue
            // 
            this.listViewDialogue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewDialogue.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listViewDialogue.FullRowSelect = true;
            this.listViewDialogue.GridLines = true;
            this.listViewDialogue.Location = new System.Drawing.Point(12, 38);
            this.listViewDialogue.Name = "listViewDialogue";
            this.listViewDialogue.Size = new System.Drawing.Size(937, 393);
            this.listViewDialogue.TabIndex = 0;
            this.listViewDialogue.UseCompatibleStateImageBehavior = false;
            this.listViewDialogue.View = System.Windows.Forms.View.Details;
            this.listViewDialogue.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewDialogue_ItemSelectionChanged);
            this.listViewDialogue.Enter += new System.EventHandler(this.listViewDialogue_Enter);
            this.listViewDialogue.Leave += new System.EventHandler(this.listViewDialogue_Leave);
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Line #";
            this.columnHeader2.Width = 52;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Before";
            this.columnHeader3.Width = 433;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "After";
            this.columnHeader4.Width = 433;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(12, 461);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(629, 66);
            this.textBox1.TabIndex = 1;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 445);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Text:";
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(855, 474);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(94, 23);
            this.buttonOk.TabIndex = 3;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(855, 504);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(94, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonPrev
            // 
            this.buttonPrev.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonPrev.Location = new System.Drawing.Point(485, 435);
            this.buttonPrev.Name = "buttonPrev";
            this.buttonPrev.Size = new System.Drawing.Size(75, 23);
            this.buttonPrev.TabIndex = 5;
            this.buttonPrev.Text = "<< Prev";
            this.buttonPrev.UseVisualStyleBackColor = true;
            this.buttonPrev.Click += new System.EventHandler(this.buttonPrev_Click);
            // 
            // buttonNext
            // 
            this.buttonNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonNext.Location = new System.Drawing.Point(566, 435);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(75, 23);
            this.buttonNext.TabIndex = 6;
            this.buttonNext.Text = "Next >>";
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // buttonDash
            // 
            this.buttonDash.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDash.Location = new System.Drawing.Point(186, 435);
            this.buttonDash.Name = "buttonDash";
            this.buttonDash.Size = new System.Drawing.Size(39, 23);
            this.buttonDash.TabIndex = 1;
            this.buttonDash.Text = "- \\n -";
            this.buttonDash.UseVisualStyleBackColor = true;
            this.buttonDash.Click += new System.EventHandler(this.buttonDash_Click);
            // 
            // buttonItalic
            // 
            this.buttonItalic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonItalic.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonItalic.Location = new System.Drawing.Point(366, 435);
            this.buttonItalic.Name = "buttonItalic";
            this.buttonItalic.Size = new System.Drawing.Size(39, 23);
            this.buttonItalic.TabIndex = 2;
            this.buttonItalic.Text = "I";
            this.buttonItalic.UseVisualStyleBackColor = true;
            this.buttonItalic.Click += new System.EventHandler(this.buttonItalic_Click);
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonClear.ForeColor = System.Drawing.Color.Red;
            this.buttonClear.Location = new System.Drawing.Point(141, 435);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(39, 23);
            this.buttonClear.TabIndex = 7;
            this.buttonClear.Text = "C";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // buttonSingleDash
            // 
            this.buttonSingleDash.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSingleDash.Location = new System.Drawing.Point(231, 435);
            this.buttonSingleDash.Name = "buttonSingleDash";
            this.buttonSingleDash.Size = new System.Drawing.Size(39, 23);
            this.buttonSingleDash.TabIndex = 8;
            this.buttonSingleDash.Text = "\\n-";
            this.buttonSingleDash.UseVisualStyleBackColor = true;
            this.buttonSingleDash.Click += new System.EventHandler(this.buttonSingleDash_Click);
            // 
            // buttonBold
            // 
            this.buttonBold.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonBold.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonBold.Location = new System.Drawing.Point(411, 435);
            this.buttonBold.Name = "buttonBold";
            this.buttonBold.Size = new System.Drawing.Size(39, 23);
            this.buttonBold.TabIndex = 9;
            this.buttonBold.Text = "B";
            this.buttonBold.UseVisualStyleBackColor = true;
            this.buttonBold.Click += new System.EventHandler(this.buttonBold_Click);
            // 
            // buttonUpper
            // 
            this.buttonUpper.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonUpper.Location = new System.Drawing.Point(276, 435);
            this.buttonUpper.Name = "buttonUpper";
            this.buttonUpper.Size = new System.Drawing.Size(39, 23);
            this.buttonUpper.TabIndex = 10;
            this.buttonUpper.Text = "U";
            this.buttonUpper.UseVisualStyleBackColor = true;
            this.buttonUpper.Click += new System.EventHandler(this.buttonUpper_Click);
            // 
            // buttonLower
            // 
            this.buttonLower.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonLower.Location = new System.Drawing.Point(321, 435);
            this.buttonLower.Name = "buttonLower";
            this.buttonLower.Size = new System.Drawing.Size(39, 23);
            this.buttonLower.TabIndex = 11;
            this.buttonLower.Text = "L";
            this.buttonLower.UseVisualStyleBackColor = true;
            this.buttonLower.Click += new System.EventHandler(this.buttonLower_Click);
            // 
            // buttonBreakCursorPos
            // 
            this.buttonBreakCursorPos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonBreakCursorPos.Location = new System.Drawing.Point(96, 435);
            this.buttonBreakCursorPos.Name = "buttonBreakCursorPos";
            this.buttonBreakCursorPos.Size = new System.Drawing.Size(39, 23);
            this.buttonBreakCursorPos.TabIndex = 12;
            this.buttonBreakCursorPos.Text = "<br>";
            this.buttonBreakCursorPos.UseVisualStyleBackColor = true;
            this.buttonBreakCursorPos.Click += new System.EventHandler(this.buttonBreakCursorPos_Click);
            // 
            // buttonUnBreak
            // 
            this.buttonUnBreak.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonUnBreak.Location = new System.Drawing.Point(51, 435);
            this.buttonUnBreak.Name = "buttonUnBreak";
            this.buttonUnBreak.Size = new System.Drawing.Size(39, 23);
            this.buttonUnBreak.TabIndex = 13;
            this.buttonUnBreak.Text = "Unbr";
            this.buttonUnBreak.UseVisualStyleBackColor = true;
            this.buttonUnBreak.Click += new System.EventHandler(this.buttonUnBreak_Click);
            // 
            // labelSingleLineLength
            // 
            this.labelSingleLineLength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelSingleLineLength.AutoSize = true;
            this.labelSingleLineLength.Location = new System.Drawing.Point(647, 491);
            this.labelSingleLineLength.Name = "labelSingleLineLength";
            this.labelSingleLineLength.Size = new System.Drawing.Size(90, 13);
            this.labelSingleLineLength.TabIndex = 14;
            this.labelSingleLineLength.Text = "Single line length:";
            this.labelSingleLineLength.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelLinesTotalLength
            // 
            this.labelLinesTotalLength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelLinesTotalLength.AutoSize = true;
            this.labelLinesTotalLength.Location = new System.Drawing.Point(647, 512);
            this.labelLinesTotalLength.Name = "labelLinesTotalLength";
            this.labelLinesTotalLength.Size = new System.Drawing.Size(90, 13);
            this.labelLinesTotalLength.TabIndex = 15;
            this.labelLinesTotalLength.Text = "Lines total length:";
            this.labelLinesTotalLength.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // checkBoxColorChanged
            // 
            this.checkBoxColorChanged.AutoSize = true;
            this.checkBoxColorChanged.Location = new System.Drawing.Point(12, 15);
            this.checkBoxColorChanged.Name = "checkBoxColorChanged";
            this.checkBoxColorChanged.Size = new System.Drawing.Size(115, 17);
            this.checkBoxColorChanged.TabIndex = 16;
            this.checkBoxColorChanged.Text = "Color changed text";
            this.checkBoxColorChanged.UseVisualStyleBackColor = true;
            // 
            // Main
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(961, 534);
            this.Controls.Add(this.checkBoxColorChanged);
            this.Controls.Add(this.labelLinesTotalLength);
            this.Controls.Add(this.labelSingleLineLength);
            this.Controls.Add(this.buttonUnBreak);
            this.Controls.Add(this.buttonBreakCursorPos);
            this.Controls.Add(this.buttonLower);
            this.Controls.Add(this.buttonUpper);
            this.Controls.Add(this.buttonBold);
            this.Controls.Add(this.buttonSingleDash);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.buttonItalic);
            this.Controls.Add(this.buttonDash);
            this.Controls.Add(this.buttonNext);
            this.Controls.Add(this.buttonPrev);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.listViewDialogue);
            this.KeyPreview = true;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dialogue AutoMarker";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Main_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewDialogue;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonPrev;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.Button buttonItalic;
        private System.Windows.Forms.Button buttonDash;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonSingleDash;
        private System.Windows.Forms.Button buttonBold;
        private System.Windows.Forms.Button buttonUpper;
        private System.Windows.Forms.Button buttonLower;
        private System.Windows.Forms.Button buttonBreakCursorPos;
        private System.Windows.Forms.Button buttonUnBreak;
        private System.Windows.Forms.Label labelSingleLineLength;
        private System.Windows.Forms.Label labelLinesTotalLength;
        private System.Windows.Forms.CheckBox checkBoxColorChanged;
    }
}

