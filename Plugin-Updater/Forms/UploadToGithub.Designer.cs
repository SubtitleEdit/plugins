namespace Plugin_Updater
{
    partial class UploadToGithub
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonUpload = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBoxPath = new System.Windows.Forms.ComboBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.textBoxToken = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSaveToken = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(272, 193);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(353, 193);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonUpload
            // 
            this.buttonUpload.Enabled = false;
            this.buttonUpload.Location = new System.Drawing.Point(19, 193);
            this.buttonUpload.Name = "buttonUpload";
            this.buttonUpload.Size = new System.Drawing.Size(75, 23);
            this.buttonUpload.TabIndex = 3;
            this.buttonUpload.Text = "Upload";
            this.buttonUpload.UseVisualStyleBackColor = true;
            this.buttonUpload.Click += new System.EventHandler(this.ButtonUpload_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBoxPath);
            this.groupBox1.Controls.Add(this.buttonBrowse);
            this.groupBox1.Location = new System.Drawing.Point(19, 61);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(409, 97);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Plugin upload:";
            // 
            // comboBoxPath
            // 
            this.comboBoxPath.FormattingEnabled = true;
            this.comboBoxPath.Location = new System.Drawing.Point(13, 33);
            this.comboBoxPath.Name = "comboBoxPath";
            this.comboBoxPath.Size = new System.Drawing.Size(382, 21);
            this.comboBoxPath.TabIndex = 2;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(320, 60);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowse.TabIndex = 1;
            this.buttonBrowse.Text = "Browse";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.ButtonBrowse_Click);
            // 
            // textBoxToken
            // 
            this.textBoxToken.Location = new System.Drawing.Point(80, 18);
            this.textBoxToken.Name = "textBoxToken";
            this.textBoxToken.PasswordChar = '*';
            this.textBoxToken.Size = new System.Drawing.Size(348, 20);
            this.textBoxToken.TabIndex = 8;
            this.textBoxToken.UseSystemPasswordChar = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Auth token:";
            // 
            // buttonSaveToken
            // 
            this.buttonSaveToken.Location = new System.Drawing.Point(353, 42);
            this.buttonSaveToken.Name = "buttonSaveToken";
            this.buttonSaveToken.Size = new System.Drawing.Size(75, 23);
            this.buttonSaveToken.TabIndex = 9;
            this.buttonSaveToken.Text = "Save token";
            this.buttonSaveToken.UseVisualStyleBackColor = true;
            this.buttonSaveToken.Click += new System.EventHandler(this.buttonSaveToken_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(101, 206);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(165, 10);
            this.progressBar1.Step = 45;
            this.progressBar1.TabIndex = 10;
            this.progressBar1.Visible = false;
            // 
            // UploadToGithub
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(446, 228);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonSaveToken);
            this.Controls.Add(this.textBoxToken);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonUpload);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Name = "UploadToGithub";
            this.Text = "UploadToGithub";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonUpload;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.TextBox textBoxToken;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonSaveToken;
        private System.Windows.Forms.ComboBox comboBoxPath;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}