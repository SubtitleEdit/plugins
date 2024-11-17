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
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.numericUpDownFadeOut = new System.Windows.Forms.NumericUpDown();
            this.labelFadeOut = new System.Windows.Forms.Label();
            this.labelFadeIn = new System.Windows.Forms.Label();
            this.numericUpDownFadeIn = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelAdvancedSelection = new System.Windows.Forms.Label();
            this.buttonAdvancedSelection = new System.Windows.Forms.Button();
            this.radioButtonAdvancedSelection = new System.Windows.Forms.RadioButton();
            this.radioButtonAllLines = new System.Windows.Forms.RadioButton();
            this.radioButtonSelectedLines = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFadeOut)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFadeIn)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(277, 153);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 3;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(358, 153);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // numericUpDownFadeOut
            // 
            this.numericUpDownFadeOut.Location = new System.Drawing.Point(85, 41);
            this.numericUpDownFadeOut.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownFadeOut.Name = "numericUpDownFadeOut";
            this.numericUpDownFadeOut.Size = new System.Drawing.Size(78, 20);
            this.numericUpDownFadeOut.TabIndex = 7;
            this.numericUpDownFadeOut.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // labelFadeOut
            // 
            this.labelFadeOut.AutoSize = true;
            this.labelFadeOut.Location = new System.Drawing.Point(9, 43);
            this.labelFadeOut.Name = "labelFadeOut";
            this.labelFadeOut.Size = new System.Drawing.Size(65, 13);
            this.labelFadeOut.TabIndex = 8;
            this.labelFadeOut.Text = "Fade out ms";
            // 
            // labelFadeIn
            // 
            this.labelFadeIn.AutoSize = true;
            this.labelFadeIn.Location = new System.Drawing.Point(9, 16);
            this.labelFadeIn.Name = "labelFadeIn";
            this.labelFadeIn.Size = new System.Drawing.Size(58, 13);
            this.labelFadeIn.TabIndex = 10;
            this.labelFadeIn.Text = "Fade in ms";
            // 
            // numericUpDownFadeIn
            // 
            this.numericUpDownFadeIn.Location = new System.Drawing.Point(85, 14);
            this.numericUpDownFadeIn.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownFadeIn.Name = "numericUpDownFadeIn";
            this.numericUpDownFadeIn.Size = new System.Drawing.Size(78, 20);
            this.numericUpDownFadeIn.TabIndex = 9;
            this.numericUpDownFadeIn.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.labelAdvancedSelection);
            this.groupBox1.Controls.Add(this.buttonAdvancedSelection);
            this.groupBox1.Controls.Add(this.radioButtonAdvancedSelection);
            this.groupBox1.Controls.Add(this.radioButtonAllLines);
            this.groupBox1.Controls.Add(this.radioButtonSelectedLines);
            this.groupBox1.Location = new System.Drawing.Point(187, 14);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(246, 121);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Apply to";
            // 
            // labelAdvancedSelection
            // 
            this.labelAdvancedSelection.AutoSize = true;
            this.labelAdvancedSelection.Location = new System.Drawing.Point(24, 87);
            this.labelAdvancedSelection.Name = "labelAdvancedSelection";
            this.labelAdvancedSelection.Size = new System.Drawing.Size(122, 13);
            this.labelAdvancedSelection.TabIndex = 7;
            this.labelAdvancedSelection.Text = "labelAdvancedSelection";
            // 
            // buttonAdvancedSelection
            // 
            this.buttonAdvancedSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdvancedSelection.Location = new System.Drawing.Point(132, 64);
            this.buttonAdvancedSelection.Name = "buttonAdvancedSelection";
            this.buttonAdvancedSelection.Size = new System.Drawing.Size(29, 23);
            this.buttonAdvancedSelection.TabIndex = 7;
            this.buttonAdvancedSelection.Text = "...";
            this.buttonAdvancedSelection.UseVisualStyleBackColor = true;
            this.buttonAdvancedSelection.Click += new System.EventHandler(this.buttonAdvancedSelection_Click);
            // 
            // radioButtonAdvancedSelection
            // 
            this.radioButtonAdvancedSelection.AutoSize = true;
            this.radioButtonAdvancedSelection.Location = new System.Drawing.Point(7, 67);
            this.radioButtonAdvancedSelection.Name = "radioButtonAdvancedSelection";
            this.radioButtonAdvancedSelection.Size = new System.Drawing.Size(119, 17);
            this.radioButtonAdvancedSelection.TabIndex = 2;
            this.radioButtonAdvancedSelection.TabStop = true;
            this.radioButtonAdvancedSelection.Text = "Advanced selection";
            this.radioButtonAdvancedSelection.UseVisualStyleBackColor = true;
            this.radioButtonAdvancedSelection.CheckedChanged += new System.EventHandler(this.radioButtonAdvancedSelection_CheckedChanged);
            // 
            // radioButtonAllLines
            // 
            this.radioButtonAllLines.AutoSize = true;
            this.radioButtonAllLines.Location = new System.Drawing.Point(7, 44);
            this.radioButtonAllLines.Name = "radioButtonAllLines";
            this.radioButtonAllLines.Size = new System.Drawing.Size(60, 17);
            this.radioButtonAllLines.TabIndex = 1;
            this.radioButtonAllLines.TabStop = true;
            this.radioButtonAllLines.Text = "All lines";
            this.radioButtonAllLines.UseVisualStyleBackColor = true;
            // 
            // radioButtonSelectedLines
            // 
            this.radioButtonSelectedLines.AutoSize = true;
            this.radioButtonSelectedLines.Location = new System.Drawing.Point(7, 20);
            this.radioButtonSelectedLines.Name = "radioButtonSelectedLines";
            this.radioButtonSelectedLines.Size = new System.Drawing.Size(102, 17);
            this.radioButtonSelectedLines.TabIndex = 0;
            this.radioButtonSelectedLines.TabStop = true;
            this.radioButtonSelectedLines.Text = "Selected lines: x";
            this.radioButtonSelectedLines.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(445, 184);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.labelFadeIn);
            this.Controls.Add(this.numericUpDownFadeIn);
            this.Controls.Add(this.labelFadeOut);
            this.Controls.Add(this.numericUpDownFadeOut);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MainForm";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFadeOut)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFadeIn)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.NumericUpDown numericUpDownFadeOut;
        private System.Windows.Forms.Label labelFadeOut;
        private System.Windows.Forms.Label labelFadeIn;
        private System.Windows.Forms.NumericUpDown numericUpDownFadeIn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelAdvancedSelection;
        private System.Windows.Forms.Button buttonAdvancedSelection;
        private System.Windows.Forms.RadioButton radioButtonAdvancedSelection;
        private System.Windows.Forms.RadioButton radioButtonAllLines;
        private System.Windows.Forms.RadioButton radioButtonSelectedLines;
    }
}