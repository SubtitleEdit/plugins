namespace plugin_tester
{
    partial class Form1
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.buttonFromTextBox = new System.Windows.Forms.Button();
            this.buttonWithFile = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(13, 13);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(329, 138);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = "17\r\n00:00:35,125 --> 00:00:37,393\r\n(Walters) Ronnie was vaporized\r\nin the acceler" +
    "ator\r\n";
            // 
            // buttonFromTextBox
            // 
            this.buttonFromTextBox.Location = new System.Drawing.Point(348, 13);
            this.buttonFromTextBox.Name = "buttonFromTextBox";
            this.buttonFromTextBox.Size = new System.Drawing.Size(75, 23);
            this.buttonFromTextBox.TabIndex = 1;
            this.buttonFromTextBox.Text = "From textbox";
            this.buttonFromTextBox.UseVisualStyleBackColor = true;
            this.buttonFromTextBox.Click += new System.EventHandler(this.buttonFromTextBox_Click);
            // 
            // buttonWithFile
            // 
            this.buttonWithFile.Location = new System.Drawing.Point(345, 42);
            this.buttonWithFile.Name = "buttonWithFile";
            this.buttonWithFile.Size = new System.Drawing.Size(75, 23);
            this.buttonWithFile.TabIndex = 2;
            this.buttonWithFile.Text = "With file";
            this.buttonWithFile.UseVisualStyleBackColor = true;
            this.buttonWithFile.Click += new System.EventHandler(this.buttonWithFile_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 164);
            this.Controls.Add(this.buttonWithFile);
            this.Controls.Add(this.buttonFromTextBox);
            this.Controls.Add(this.textBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button buttonFromTextBox;
        private System.Windows.Forms.Button buttonWithFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}

