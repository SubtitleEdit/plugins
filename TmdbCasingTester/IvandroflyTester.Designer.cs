namespace TmdbCasingTester
{
    partial class IvandroflyTester
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonRun = new System.Windows.Forms.Button();
            this.radioButtonLinesunbreaker = new System.Windows.Forms.RadioButton();
            this.radioButtonTmdbCasing = new System.Windows.Forms.RadioButton();
            this.radioButtonHI2UC = new System.Windows.Forms.RadioButton();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(293, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // buttonRun
            // 
            this.buttonRun.Location = new System.Drawing.Point(191, 105);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 23);
            this.buttonRun.TabIndex = 1;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // radioButtonLinesunbreaker
            // 
            this.radioButtonLinesunbreaker.AutoSize = true;
            this.radioButtonLinesunbreaker.Location = new System.Drawing.Point(12, 70);
            this.radioButtonLinesunbreaker.Name = "radioButtonLinesunbreaker";
            this.radioButtonLinesunbreaker.Size = new System.Drawing.Size(100, 17);
            this.radioButtonLinesunbreaker.TabIndex = 2;
            this.radioButtonLinesunbreaker.Text = "LinesUnbreaker";
            this.radioButtonLinesunbreaker.UseVisualStyleBackColor = true;
            // 
            // radioButtonTmdbCasing
            // 
            this.radioButtonTmdbCasing.AutoSize = true;
            this.radioButtonTmdbCasing.Location = new System.Drawing.Point(12, 93);
            this.radioButtonTmdbCasing.Name = "radioButtonTmdbCasing";
            this.radioButtonTmdbCasing.Size = new System.Drawing.Size(84, 17);
            this.radioButtonTmdbCasing.TabIndex = 3;
            this.radioButtonTmdbCasing.Text = "TmdbCasing";
            this.radioButtonTmdbCasing.UseVisualStyleBackColor = true;
            // 
            // radioButtonHI2UC
            // 
            this.radioButtonHI2UC.AutoSize = true;
            this.radioButtonHI2UC.Checked = true;
            this.radioButtonHI2UC.Location = new System.Drawing.Point(12, 47);
            this.radioButtonHI2UC.Name = "radioButtonHI2UC";
            this.radioButtonHI2UC.Size = new System.Drawing.Size(57, 17);
            this.radioButtonHI2UC.TabIndex = 4;
            this.radioButtonHI2UC.TabStop = true;
            this.radioButtonHI2UC.Text = "HI2UC";
            this.radioButtonHI2UC.UseVisualStyleBackColor = true;
            // 
            // IvandroflyTester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(293, 140);
            this.Controls.Add(this.radioButtonHI2UC);
            this.Controls.Add(this.radioButtonTmdbCasing);
            this.Controls.Add(this.radioButtonLinesunbreaker);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "IvandroflyTester";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "IvandroflyTester";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.RadioButton radioButtonLinesunbreaker;
        private System.Windows.Forms.RadioButton radioButtonTmdbCasing;
        private System.Windows.Forms.RadioButton radioButtonHI2UC;
    }
}

