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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.textBoxCustomCharNotFound = new System.Windows.Forms.TextBox();
            this.labelCustomCharNotFound = new System.Windows.Forms.Label();
            this.labelLongestLine = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButtonUseCustom = new System.Windows.Forms.RadioButton();
            this.radioButtonUseFont = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.labelFont = new System.Windows.Forms.Label();
            this.buttonEditCustomCharWidthList = new System.Windows.Forms.Button();
            this.buttonChooseFont = new System.Windows.Forms.Button();
            this.fontDialog1 = new System.Windows.Forms.FontDialog();
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.labelText = new System.Windows.Forms.Label();
            this.buttonAutoBreak = new System.Windows.Forms.Button();
            this.labelErrorCount = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.subtitleListView1 = new Nikse.SubtitleEdit.PluginLogic.Controls.SubtitleListView();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(923, 565);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(101, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(816, 565);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(101, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Location = new System.Drawing.Point(7, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1017, 202);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.textBoxCustomCharNotFound);
            this.groupBox3.Controls.Add(this.labelCustomCharNotFound);
            this.groupBox3.Controls.Add(this.labelLongestLine);
            this.groupBox3.Controls.Add(this.labelStatus);
            this.groupBox3.Location = new System.Drawing.Point(328, 10);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(683, 175);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Status";
            // 
            // textBoxCustomCharNotFound
            // 
            this.textBoxCustomCharNotFound.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCustomCharNotFound.Location = new System.Drawing.Point(9, 97);
            this.textBoxCustomCharNotFound.Multiline = true;
            this.textBoxCustomCharNotFound.Name = "textBoxCustomCharNotFound";
            this.textBoxCustomCharNotFound.ReadOnly = true;
            this.textBoxCustomCharNotFound.Size = new System.Drawing.Size(668, 72);
            this.textBoxCustomCharNotFound.TabIndex = 3;
            // 
            // labelCustomCharNotFound
            // 
            this.labelCustomCharNotFound.AutoSize = true;
            this.labelCustomCharNotFound.Location = new System.Drawing.Point(6, 80);
            this.labelCustomCharNotFound.Name = "labelCustomCharNotFound";
            this.labelCustomCharNotFound.Size = new System.Drawing.Size(133, 13);
            this.labelCustomCharNotFound.TabIndex = 2;
            this.labelCustomCharNotFound.Text = "labelCustomCharNotFound";
            // 
            // labelLongestLine
            // 
            this.labelLongestLine.AutoSize = true;
            this.labelLongestLine.Location = new System.Drawing.Point(6, 51);
            this.labelLongestLine.Name = "labelLongestLine";
            this.labelLongestLine.Size = new System.Drawing.Size(87, 13);
            this.labelLongestLine.TabIndex = 1;
            this.labelLongestLine.Text = "labelLongestLine";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(6, 24);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 0;
            this.labelStatus.Text = "labelStatus";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.groupBox2.Controls.Add(this.radioButtonUseCustom);
            this.groupBox2.Controls.Add(this.radioButtonUseFont);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.numericUpDown1);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.labelFont);
            this.groupBox2.Controls.Add(this.buttonEditCustomCharWidthList);
            this.groupBox2.Controls.Add(this.buttonChooseFont);
            this.groupBox2.Location = new System.Drawing.Point(6, 10);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(316, 175);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Settings";
            // 
            // radioButtonUseCustom
            // 
            this.radioButtonUseCustom.AutoSize = true;
            this.radioButtonUseCustom.Location = new System.Drawing.Point(7, 119);
            this.radioButtonUseCustom.Name = "radioButtonUseCustom";
            this.radioButtonUseCustom.Size = new System.Drawing.Size(150, 17);
            this.radioButtonUseCustom.TabIndex = 6;
            this.radioButtonUseCustom.Text = "Use custom char/width list";
            this.radioButtonUseCustom.UseVisualStyleBackColor = true;
            // 
            // radioButtonUseFont
            // 
            this.radioButtonUseFont.AutoSize = true;
            this.radioButtonUseFont.Checked = true;
            this.radioButtonUseFont.Location = new System.Drawing.Point(7, 54);
            this.radioButtonUseFont.Name = "radioButtonUseFont";
            this.radioButtonUseFont.Size = new System.Drawing.Size(65, 17);
            this.radioButtonUseFont.TabIndex = 3;
            this.radioButtonUseFont.TabStop = true;
            this.radioButtonUseFont.Text = "Use font";
            this.radioButtonUseFont.UseVisualStyleBackColor = true;
            this.radioButtonUseFont.CheckedChanged += new System.EventHandler(this.radioButtonUseFont_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(182, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "pixels";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(131, 20);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(45, 20);
            this.numericUpDown1.TabIndex = 1;
            this.numericUpDown1.Value = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Highlight lines long than";
            // 
            // labelFont
            // 
            this.labelFont.AutoSize = true;
            this.labelFont.Location = new System.Drawing.Point(149, 79);
            this.labelFont.Name = "labelFont";
            this.labelFont.Size = new System.Drawing.Size(50, 13);
            this.labelFont.TabIndex = 5;
            this.labelFont.Text = "labelFont";
            // 
            // buttonEditCustomCharWidthList
            // 
            this.buttonEditCustomCharWidthList.Location = new System.Drawing.Point(36, 142);
            this.buttonEditCustomCharWidthList.Name = "buttonEditCustomCharWidthList";
            this.buttonEditCustomCharWidthList.Size = new System.Drawing.Size(106, 23);
            this.buttonEditCustomCharWidthList.TabIndex = 7;
            this.buttonEditCustomCharWidthList.Text = "Edit...";
            this.buttonEditCustomCharWidthList.UseVisualStyleBackColor = true;
            this.buttonEditCustomCharWidthList.Click += new System.EventHandler(this.buttonEditChars_Click);
            // 
            // buttonChooseFont
            // 
            this.buttonChooseFont.Location = new System.Drawing.Point(36, 76);
            this.buttonChooseFont.Name = "buttonChooseFont";
            this.buttonChooseFont.Size = new System.Drawing.Size(106, 23);
            this.buttonChooseFont.TabIndex = 4;
            this.buttonChooseFont.Text = "Choose font...";
            this.buttonChooseFont.UseVisualStyleBackColor = true;
            this.buttonChooseFont.Click += new System.EventHandler(this.buttonChooseFont_Click);
            // 
            // textBoxText
            // 
            this.textBoxText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxText.Location = new System.Drawing.Point(232, 536);
            this.textBoxText.Multiline = true;
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.Size = new System.Drawing.Size(361, 52);
            this.textBoxText.TabIndex = 2;
            this.textBoxText.TextChanged += new System.EventHandler(this.textBoxText_TextChanged);
            this.textBoxText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxText_KeyDown);
            // 
            // labelText
            // 
            this.labelText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelText.AutoSize = true;
            this.labelText.Location = new System.Drawing.Point(198, 539);
            this.labelText.Name = "labelText";
            this.labelText.Size = new System.Drawing.Size(28, 13);
            this.labelText.TabIndex = 1;
            this.labelText.Text = "Text";
            // 
            // buttonAutoBreak
            // 
            this.buttonAutoBreak.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAutoBreak.Location = new System.Drawing.Point(599, 536);
            this.buttonAutoBreak.Name = "buttonAutoBreak";
            this.buttonAutoBreak.Size = new System.Drawing.Size(193, 21);
            this.buttonAutoBreak.TabIndex = 3;
            this.buttonAutoBreak.Text = "Auto-break via pixel width";
            this.buttonAutoBreak.UseVisualStyleBackColor = true;
            this.buttonAutoBreak.Click += new System.EventHandler(this.buttonUnBreak_Click);
            // 
            // labelErrorCount
            // 
            this.labelErrorCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelErrorCount.AutoSize = true;
            this.labelErrorCount.Location = new System.Drawing.Point(4, 575);
            this.labelErrorCount.Name = "labelErrorCount";
            this.labelErrorCount.Size = new System.Drawing.Size(65, 13);
            this.labelErrorCount.TabIndex = 6;
            this.labelErrorCount.Text = "Error count: ";
            // 
            // timer1
            // 
            this.timer1.Interval = 200;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // subtitleListView1
            // 
            this.subtitleListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.subtitleListView1.DisplayExtraFromExtra = false;
            this.subtitleListView1.FirstVisibleIndex = -1;
            this.subtitleListView1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListView1.FullRowSelect = true;
            this.subtitleListView1.GridLines = true;
            this.subtitleListView1.Location = new System.Drawing.Point(7, 233);
            this.subtitleListView1.Name = "subtitleListView1";
            this.subtitleListView1.OwnerDraw = true;
            this.subtitleListView1.Size = new System.Drawing.Size(1017, 297);
            this.subtitleListView1.SubtitleFontBold = false;
            this.subtitleListView1.SubtitleFontName = "Tahoma";
            this.subtitleListView1.SubtitleFontSize = 8;
            this.subtitleListView1.TabIndex = 0;
            this.subtitleListView1.UseCompatibleStateImageBehavior = false;
            this.subtitleListView1.UseSyntaxColoring = true;
            this.subtitleListView1.View = System.Windows.Forms.View.Details;
            this.subtitleListView1.SelectedIndexChanged += new System.EventHandler(this.subtitleListView1_SelectedIndexChanged);
            this.subtitleListView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.subtitleListView1_KeyDown);
            // 
            // PluginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1036, 595);
            this.Controls.Add(this.labelErrorCount);
            this.Controls.Add(this.buttonAutoBreak);
            this.Controls.Add(this.textBoxText);
            this.Controls.Add(this.labelText);
            this.Controls.Add(this.subtitleListView1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(834, 583);
            this.Name = "PluginForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Check line width";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PluginForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PluginForm_KeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonChooseFont;
        private System.Windows.Forms.FontDialog fontDialog1;
        private System.Windows.Forms.Label labelFont;
        private System.Windows.Forms.Button buttonEditCustomCharWidthList;
        private Controls.SubtitleListView subtitleListView1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label labelLongestLine;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.Label labelText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioButtonUseCustom;
        private System.Windows.Forms.RadioButton radioButtonUseFont;
        private System.Windows.Forms.Button buttonAutoBreak;
        private System.Windows.Forms.Label labelCustomCharNotFound;
        private System.Windows.Forms.TextBox textBoxCustomCharNotFound;
        private System.Windows.Forms.Label labelErrorCount;
        private System.Windows.Forms.Timer timer1;
    }
}