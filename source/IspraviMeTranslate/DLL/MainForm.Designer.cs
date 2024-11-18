namespace SubtitleEdit
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
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.linkLabelPoweredBy = new System.Windows.Forms.LinkLabel();
            this.buttonTranslate = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.buttonCancelTranslate = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.richTextBoxParagraph = new System.Windows.Forms.RichTextBox();
            this.groupBoxSuggestions = new System.Windows.Forms.GroupBox();
            this.buttonUseSuggestion = new System.Windows.Forms.Button();
            this.buttonUseSuggestionAlways = new System.Windows.Forms.Button();
            this.listBoxSuggestions = new System.Windows.Forms.ListBox();
            this.labelFullText = new System.Windows.Forms.Label();
            this.groupBoxWordNotFound = new System.Windows.Forms.GroupBox();
            this.buttonGoogleIt = new System.Windows.Forms.Button();
            this.buttonUndo = new System.Windows.Forms.Button();
            this.buttonSkipOnce = new System.Windows.Forms.Button();
            this.buttonChangeAll = new System.Windows.Forms.Button();
            this.textBoxWord = new System.Windows.Forms.TextBox();
            this.buttonSkipAll = new System.Windows.Forms.Button();
            this.buttonChange = new System.Windows.Forms.Button();
            this.groupBoxSuggestions.SuspendLayout();
            this.groupBoxWordNotFound.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 12);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(737, 294);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.Resize += new System.EventHandler(this.listView1_Resize);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Linija #";
            this.columnHeader1.Width = 48;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Text";
            this.columnHeader2.Width = 410;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Sumnjive riječi";
            this.columnHeader3.Width = 250;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(593, 626);
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
            this.buttonCancel.Location = new System.Drawing.Point(674, 626);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "Otkaži";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // linkLabelPoweredBy
            // 
            this.linkLabelPoweredBy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelPoweredBy.AutoSize = true;
            this.linkLabelPoweredBy.Location = new System.Drawing.Point(12, 609);
            this.linkLabelPoweredBy.Name = "linkLabelPoweredBy";
            this.linkLabelPoweredBy.Size = new System.Drawing.Size(71, 13);
            this.linkLabelPoweredBy.TabIndex = 6;
            this.linkLabelPoweredBy.TabStop = true;
            this.linkLabelPoweredBy.Text = "Omogućuje x";
            this.linkLabelPoweredBy.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // buttonTranslate
            // 
            this.buttonTranslate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonTranslate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTranslate.Location = new System.Drawing.Point(326, 356);
            this.buttonTranslate.Name = "buttonTranslate";
            this.buttonTranslate.Size = new System.Drawing.Size(127, 23);
            this.buttonTranslate.TabIndex = 2;
            this.buttonTranslate.Text = "Nađi zatipke";
            this.buttonTranslate.UseVisualStyleBackColor = true;
            this.buttonTranslate.Click += new System.EventHandler(this.buttonTranslate_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(12, 12);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(737, 294);
            this.textBox1.TabIndex = 5;
            // 
            // buttonCancelTranslate
            // 
            this.buttonCancelTranslate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCancelTranslate.Location = new System.Drawing.Point(459, 356);
            this.buttonCancelTranslate.Name = "buttonCancelTranslate";
            this.buttonCancelTranslate.Size = new System.Drawing.Size(99, 23);
            this.buttonCancelTranslate.TabIndex = 3;
            this.buttonCancelTranslate.Text = "Stani";
            this.buttonCancelTranslate.UseVisualStyleBackColor = true;
            this.buttonCancelTranslate.Click += new System.EventHandler(this.buttonCancelTranslate_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(15, 626);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(572, 23);
            this.progressBar1.TabIndex = 4;
            this.progressBar1.Visible = false;
            // 
            // richTextBoxParagraph
            // 
            this.richTextBoxParagraph.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.richTextBoxParagraph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBoxParagraph.Location = new System.Drawing.Point(15, 356);
            this.richTextBoxParagraph.Name = "richTextBoxParagraph";
            this.richTextBoxParagraph.ReadOnly = true;
            this.richTextBoxParagraph.Size = new System.Drawing.Size(292, 54);
            this.richTextBoxParagraph.TabIndex = 12;
            this.richTextBoxParagraph.Text = "";
            // 
            // groupBoxSuggestions
            // 
            this.groupBoxSuggestions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSuggestions.Controls.Add(this.buttonUseSuggestion);
            this.groupBoxSuggestions.Controls.Add(this.buttonUseSuggestionAlways);
            this.groupBoxSuggestions.Controls.Add(this.listBoxSuggestions);
            this.groupBoxSuggestions.Enabled = false;
            this.groupBoxSuggestions.Location = new System.Drawing.Point(319, 431);
            this.groupBoxSuggestions.Name = "groupBoxSuggestions";
            this.groupBoxSuggestions.Size = new System.Drawing.Size(430, 170);
            this.groupBoxSuggestions.TabIndex = 10;
            this.groupBoxSuggestions.TabStop = false;
            this.groupBoxSuggestions.Text = "Prijedlozi";
            // 
            // buttonUseSuggestion
            // 
            this.buttonUseSuggestion.Location = new System.Drawing.Point(7, 15);
            this.buttonUseSuggestion.Name = "buttonUseSuggestion";
            this.buttonUseSuggestion.Size = new System.Drawing.Size(90, 21);
            this.buttonUseSuggestion.TabIndex = 0;
            this.buttonUseSuggestion.Text = "Koristi";
            this.buttonUseSuggestion.UseVisualStyleBackColor = true;
            this.buttonUseSuggestion.Click += new System.EventHandler(this.buttonUseSuggestion_Click);
            // 
            // buttonUseSuggestionAlways
            // 
            this.buttonUseSuggestionAlways.Location = new System.Drawing.Point(103, 15);
            this.buttonUseSuggestionAlways.Name = "buttonUseSuggestionAlways";
            this.buttonUseSuggestionAlways.Size = new System.Drawing.Size(90, 21);
            this.buttonUseSuggestionAlways.TabIndex = 1;
            this.buttonUseSuggestionAlways.Text = "Koristi uvijek";
            this.buttonUseSuggestionAlways.UseVisualStyleBackColor = true;
            this.buttonUseSuggestionAlways.Click += new System.EventHandler(this.buttonUseSuggestionAlways_Click);
            // 
            // listBoxSuggestions
            // 
            this.listBoxSuggestions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxSuggestions.FormattingEnabled = true;
            this.listBoxSuggestions.Location = new System.Drawing.Point(7, 42);
            this.listBoxSuggestions.Name = "listBoxSuggestions";
            this.listBoxSuggestions.Size = new System.Drawing.Size(411, 108);
            this.listBoxSuggestions.TabIndex = 2;
            // 
            // labelFullText
            // 
            this.labelFullText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelFullText.AutoSize = true;
            this.labelFullText.Location = new System.Drawing.Point(12, 337);
            this.labelFullText.Name = "labelFullText";
            this.labelFullText.Size = new System.Drawing.Size(43, 13);
            this.labelFullText.TabIndex = 11;
            this.labelFullText.Text = "Pun tekst";
            // 
            // groupBoxWordNotFound
            // 
            this.groupBoxWordNotFound.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxWordNotFound.Controls.Add(this.buttonGoogleIt);
            this.groupBoxWordNotFound.Controls.Add(this.buttonUndo);
            this.groupBoxWordNotFound.Controls.Add(this.buttonSkipOnce);
            this.groupBoxWordNotFound.Controls.Add(this.buttonChangeAll);
            this.groupBoxWordNotFound.Controls.Add(this.textBoxWord);
            this.groupBoxWordNotFound.Controls.Add(this.buttonSkipAll);
            this.groupBoxWordNotFound.Controls.Add(this.buttonChange);
            this.groupBoxWordNotFound.Enabled = false;
            this.groupBoxWordNotFound.Location = new System.Drawing.Point(15, 431);
            this.groupBoxWordNotFound.Name = "groupBoxWordNotFound";
            this.groupBoxWordNotFound.Size = new System.Drawing.Size(292, 170);
            this.groupBoxWordNotFound.TabIndex = 9;
            this.groupBoxWordNotFound.TabStop = false;
            this.groupBoxWordNotFound.Text = "Sumnjiva riječ";
            // 
            // buttonGoogleIt
            // 
            this.buttonGoogleIt.Location = new System.Drawing.Point(6, 102);
            this.buttonGoogleIt.Name = "buttonGoogleIt";
            this.buttonGoogleIt.Size = new System.Drawing.Size(280, 21);
            this.buttonGoogleIt.TabIndex = 7;
            this.buttonGoogleIt.Text = "&Google it";
            this.buttonGoogleIt.UseVisualStyleBackColor = true;
            this.buttonGoogleIt.Click += new System.EventHandler(this.buttonGoogleIt_Click);
            // 
            // buttonUndo
            // 
            this.buttonUndo.Location = new System.Drawing.Point(6, 129);
            this.buttonUndo.Name = "buttonUndo";
            this.buttonUndo.Size = new System.Drawing.Size(280, 21);
            this.buttonUndo.TabIndex = 8;
            this.buttonUndo.Text = "Poništi: preskoči sve \'A\'";
            this.buttonUndo.UseVisualStyleBackColor = true;
            this.buttonUndo.Visible = false;
            // 
            // buttonSkipOnce
            // 
            this.buttonSkipOnce.Location = new System.Drawing.Point(6, 75);
            this.buttonSkipOnce.Name = "buttonSkipOnce";
            this.buttonSkipOnce.Size = new System.Drawing.Size(136, 21);
            this.buttonSkipOnce.TabIndex = 3;
            this.buttonSkipOnce.Text = "Preskoči";
            this.buttonSkipOnce.UseVisualStyleBackColor = true;
            this.buttonSkipOnce.Click += new System.EventHandler(this.buttonSkipOnce_Click);
            // 
            // buttonChangeAll
            // 
            this.buttonChangeAll.Location = new System.Drawing.Point(148, 47);
            this.buttonChangeAll.Name = "buttonChangeAll";
            this.buttonChangeAll.Size = new System.Drawing.Size(138, 21);
            this.buttonChangeAll.TabIndex = 2;
            this.buttonChangeAll.Text = "Primijeni na sve";
            this.buttonChangeAll.UseVisualStyleBackColor = true;
            this.buttonChangeAll.Click += new System.EventHandler(this.buttonChangeAll_Click);
            // 
            // textBoxWord
            // 
            this.textBoxWord.Location = new System.Drawing.Point(6, 20);
            this.textBoxWord.Name = "textBoxWord";
            this.textBoxWord.Size = new System.Drawing.Size(280, 20);
            this.textBoxWord.TabIndex = 0;
            this.textBoxWord.TextChanged += new System.EventHandler(this.textBoxWord_TextChanged);
            // 
            // buttonSkipAll
            // 
            this.buttonSkipAll.Location = new System.Drawing.Point(148, 75);
            this.buttonSkipAll.Name = "buttonSkipAll";
            this.buttonSkipAll.Size = new System.Drawing.Size(138, 21);
            this.buttonSkipAll.TabIndex = 4;
            this.buttonSkipAll.Text = "&Preskoči sve";
            this.buttonSkipAll.UseVisualStyleBackColor = true;
            this.buttonSkipAll.Click += new System.EventHandler(this.buttonSkipAll_Click);
            // 
            // buttonChange
            // 
            this.buttonChange.Location = new System.Drawing.Point(6, 47);
            this.buttonChange.Name = "buttonChange";
            this.buttonChange.Size = new System.Drawing.Size(136, 21);
            this.buttonChange.TabIndex = 1;
            this.buttonChange.Text = "Primijeni";
            this.buttonChange.UseVisualStyleBackColor = true;
            this.buttonChange.Click += new System.EventHandler(this.buttonChange_Click);
            // 
            // MainForm
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(761, 657);
            this.Controls.Add(this.richTextBoxParagraph);
            this.Controls.Add(this.groupBoxSuggestions);
            this.Controls.Add(this.labelFullText);
            this.Controls.Add(this.groupBoxWordNotFound);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonCancelTranslate);
            this.Controls.Add(this.buttonTranslate);
            this.Controls.Add(this.linkLabelPoweredBy);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.textBox1);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(733, 478);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MainForm";
            this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.groupBoxSuggestions.ResumeLayout(false);
            this.groupBoxWordNotFound.ResumeLayout(false);
            this.groupBoxWordNotFound.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.LinkLabel linkLabelPoweredBy;
        private System.Windows.Forms.Button buttonTranslate;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button buttonCancelTranslate;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.RichTextBox richTextBoxParagraph;
        private System.Windows.Forms.GroupBox groupBoxSuggestions;
        private System.Windows.Forms.Button buttonUseSuggestion;
        private System.Windows.Forms.Button buttonUseSuggestionAlways;
        private System.Windows.Forms.ListBox listBoxSuggestions;
        private System.Windows.Forms.Label labelFullText;
        private System.Windows.Forms.GroupBox groupBoxWordNotFound;
        private System.Windows.Forms.Button buttonGoogleIt;
        private System.Windows.Forms.Button buttonUndo;
        private System.Windows.Forms.Button buttonSkipOnce;
        private System.Windows.Forms.Button buttonChangeAll;
        private System.Windows.Forms.TextBox textBoxWord;
        private System.Windows.Forms.Button buttonSkipAll;
        private System.Windows.Forms.Button buttonChange;
    }
}
