namespace OpenSubtitlesUpload
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
            this.buttonUpload = new System.Windows.Forms.Button();
            this.textBoxSubtitleFileName = new System.Windows.Forms.TextBox();
            this.linkLabelUploadManually = new System.Windows.Forms.LinkLabel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelSubtitleFileName = new System.Windows.Forms.Label();
            this.labelMovieFileName = new System.Windows.Forms.Label();
            this.textBoxMovieFileName = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelUserName = new System.Windows.Forms.Label();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.groupBoxLogin = new System.Windows.Forms.GroupBox();
            this.linkLabelRegister = new System.Windows.Forms.LinkLabel();
            this.groupBoxSubtitleInfo = new System.Windows.Forms.GroupBox();
            this.comboBoxFrameRate = new System.Windows.Forms.ComboBox();
            this.labelFrameRate = new System.Windows.Forms.Label();
            this.comboBoxEncoding = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBoxHD = new System.Windows.Forms.CheckBox();
            this.checkBoxTextForHI = new System.Windows.Forms.CheckBox();
            this.buttonSearchIMDb = new System.Windows.Forms.Button();
            this.buttonOpenVideo = new System.Windows.Forms.Button();
            this.linkLabelSearchImdb = new System.Windows.Forms.LinkLabel();
            this.textBoxComment = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxReleaseName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxImdbId = new System.Windows.Forms.TextBox();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.openFileDialogVideo = new System.Windows.Forms.OpenFileDialog();
            this.groupBoxLogin.SuspendLayout();
            this.groupBoxSubtitleInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonUpload
            // 
            this.buttonUpload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUpload.Location = new System.Drawing.Point(301, 465);
            this.buttonUpload.Name = "buttonUpload";
            this.buttonUpload.Size = new System.Drawing.Size(75, 23);
            this.buttonUpload.TabIndex = 4;
            this.buttonUpload.Text = "Upload";
            this.buttonUpload.UseVisualStyleBackColor = true;
            this.buttonUpload.Click += new System.EventHandler(this.UploadClick);
            // 
            // textBoxSubtitleFileName
            // 
            this.textBoxSubtitleFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSubtitleFileName.Location = new System.Drawing.Point(116, 107);
            this.textBoxSubtitleFileName.Name = "textBoxSubtitleFileName";
            this.textBoxSubtitleFileName.Size = new System.Drawing.Size(317, 20);
            this.textBoxSubtitleFileName.TabIndex = 9;
            // 
            // linkLabelUploadManually
            // 
            this.linkLabelUploadManually.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelUploadManually.AutoSize = true;
            this.linkLabelUploadManually.Location = new System.Drawing.Point(13, 473);
            this.linkLabelUploadManually.Name = "linkLabelUploadManually";
            this.linkLabelUploadManually.Size = new System.Drawing.Size(85, 13);
            this.linkLabelUploadManually.TabIndex = 3;
            this.linkLabelUploadManually.TabStop = true;
            this.linkLabelUploadManually.Text = "Upload manually";
            this.linkLabelUploadManually.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelUploadManually_LinkClicked);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(382, 465);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelSubtitleFileName
            // 
            this.labelSubtitleFileName.AutoSize = true;
            this.labelSubtitleFileName.Location = new System.Drawing.Point(6, 110);
            this.labelSubtitleFileName.Name = "labelSubtitleFileName";
            this.labelSubtitleFileName.Size = new System.Drawing.Size(52, 13);
            this.labelSubtitleFileName.TabIndex = 8;
            this.labelSubtitleFileName.Text = "File name";
            // 
            // labelMovieFileName
            // 
            this.labelMovieFileName.AutoSize = true;
            this.labelMovieFileName.Location = new System.Drawing.Point(6, 163);
            this.labelMovieFileName.Name = "labelMovieFileName";
            this.labelMovieFileName.Size = new System.Drawing.Size(81, 13);
            this.labelMovieFileName.TabIndex = 12;
            this.labelMovieFileName.Text = "Movie file name";
            // 
            // textBoxMovieFileName
            // 
            this.textBoxMovieFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMovieFileName.Location = new System.Drawing.Point(116, 160);
            this.textBoxMovieFileName.Name = "textBoxMovieFileName";
            this.textBoxMovieFileName.Size = new System.Drawing.Size(287, 20);
            this.textBoxMovieFileName.TabIndex = 13;
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(14, 58);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(53, 13);
            this.labelPassword.TabIndex = 3;
            this.labelPassword.Text = "Password";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPassword.Location = new System.Drawing.Point(114, 55);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(179, 20);
            this.textBoxPassword.TabIndex = 4;
            // 
            // labelUserName
            // 
            this.labelUserName.AutoSize = true;
            this.labelUserName.Location = new System.Drawing.Point(14, 32);
            this.labelUserName.Name = "labelUserName";
            this.labelUserName.Size = new System.Drawing.Size(58, 13);
            this.labelUserName.TabIndex = 0;
            this.labelUserName.Text = "User name";
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUserName.Location = new System.Drawing.Point(114, 29);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(179, 20);
            this.textBoxUserName.TabIndex = 1;
            // 
            // groupBoxLogin
            // 
            this.groupBoxLogin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLogin.Controls.Add(this.linkLabelRegister);
            this.groupBoxLogin.Controls.Add(this.labelUserName);
            this.groupBoxLogin.Controls.Add(this.labelPassword);
            this.groupBoxLogin.Controls.Add(this.textBoxPassword);
            this.groupBoxLogin.Controls.Add(this.textBoxUserName);
            this.groupBoxLogin.Location = new System.Drawing.Point(16, 353);
            this.groupBoxLogin.Name = "groupBoxLogin";
            this.groupBoxLogin.Size = new System.Drawing.Size(438, 89);
            this.groupBoxLogin.TabIndex = 1;
            this.groupBoxLogin.TabStop = false;
            this.groupBoxLogin.Text = "OpenSubtitles.org login";
            // 
            // linkLabelRegister
            // 
            this.linkLabelRegister.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelRegister.AutoSize = true;
            this.linkLabelRegister.Location = new System.Drawing.Point(299, 32);
            this.linkLabelRegister.Name = "linkLabelRegister";
            this.linkLabelRegister.Size = new System.Drawing.Size(95, 13);
            this.linkLabelRegister.TabIndex = 2;
            this.linkLabelRegister.TabStop = true;
            this.linkLabelRegister.Text = "Register  new user";
            this.linkLabelRegister.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelRegister_LinkClicked);
            // 
            // groupBoxSubtitleInfo
            // 
            this.groupBoxSubtitleInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSubtitleInfo.Controls.Add(this.comboBoxFrameRate);
            this.groupBoxSubtitleInfo.Controls.Add(this.labelFrameRate);
            this.groupBoxSubtitleInfo.Controls.Add(this.comboBoxEncoding);
            this.groupBoxSubtitleInfo.Controls.Add(this.label5);
            this.groupBoxSubtitleInfo.Controls.Add(this.checkBoxHD);
            this.groupBoxSubtitleInfo.Controls.Add(this.checkBoxTextForHI);
            this.groupBoxSubtitleInfo.Controls.Add(this.buttonSearchIMDb);
            this.groupBoxSubtitleInfo.Controls.Add(this.buttonOpenVideo);
            this.groupBoxSubtitleInfo.Controls.Add(this.linkLabelSearchImdb);
            this.groupBoxSubtitleInfo.Controls.Add(this.textBoxComment);
            this.groupBoxSubtitleInfo.Controls.Add(this.label4);
            this.groupBoxSubtitleInfo.Controls.Add(this.label3);
            this.groupBoxSubtitleInfo.Controls.Add(this.textBoxReleaseName);
            this.groupBoxSubtitleInfo.Controls.Add(this.label2);
            this.groupBoxSubtitleInfo.Controls.Add(this.textBoxImdbId);
            this.groupBoxSubtitleInfo.Controls.Add(this.comboBoxLanguage);
            this.groupBoxSubtitleInfo.Controls.Add(this.label1);
            this.groupBoxSubtitleInfo.Controls.Add(this.labelSubtitleFileName);
            this.groupBoxSubtitleInfo.Controls.Add(this.textBoxSubtitleFileName);
            this.groupBoxSubtitleInfo.Controls.Add(this.labelMovieFileName);
            this.groupBoxSubtitleInfo.Controls.Add(this.textBoxMovieFileName);
            this.groupBoxSubtitleInfo.Location = new System.Drawing.Point(12, 12);
            this.groupBoxSubtitleInfo.Name = "groupBoxSubtitleInfo";
            this.groupBoxSubtitleInfo.Size = new System.Drawing.Size(442, 335);
            this.groupBoxSubtitleInfo.TabIndex = 0;
            this.groupBoxSubtitleInfo.TabStop = false;
            this.groupBoxSubtitleInfo.Text = "Subtitle info";
            // 
            // comboBoxFrameRate
            // 
            this.comboBoxFrameRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFrameRate.FormattingEnabled = true;
            this.comboBoxFrameRate.Items.AddRange(new object[] {
            "Select",
            "23.976",
            "23.980",
            "24.000",
            "25.000",
            "29.970",
            "30.000"});
            this.comboBoxFrameRate.Location = new System.Drawing.Point(116, 185);
            this.comboBoxFrameRate.Name = "comboBoxFrameRate";
            this.comboBoxFrameRate.Size = new System.Drawing.Size(315, 21);
            this.comboBoxFrameRate.TabIndex = 16;
            // 
            // labelFrameRate
            // 
            this.labelFrameRate.AutoSize = true;
            this.labelFrameRate.Location = new System.Drawing.Point(6, 189);
            this.labelFrameRate.Name = "labelFrameRate";
            this.labelFrameRate.Size = new System.Drawing.Size(27, 13);
            this.labelFrameRate.TabIndex = 15;
            this.labelFrameRate.Text = "FPS";
            // 
            // comboBoxEncoding
            // 
            this.comboBoxEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEncoding.FormattingEnabled = true;
            this.comboBoxEncoding.Location = new System.Drawing.Point(116, 133);
            this.comboBoxEncoding.Name = "comboBoxEncoding";
            this.comboBoxEncoding.Size = new System.Drawing.Size(317, 21);
            this.comboBoxEncoding.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 136);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Text encoding";
            // 
            // checkBoxHD
            // 
            this.checkBoxHD.AutoSize = true;
            this.checkBoxHD.Location = new System.Drawing.Point(227, 309);
            this.checkBoxHD.Name = "checkBoxHD";
            this.checkBoxHD.Size = new System.Drawing.Size(118, 17);
            this.checkBoxHD.TabIndex = 20;
            this.checkBoxHD.Text = "High-definition (HD)";
            this.checkBoxHD.UseVisualStyleBackColor = true;
            // 
            // checkBoxTextForHI
            // 
            this.checkBoxTextForHI.AutoSize = true;
            this.checkBoxTextForHI.Location = new System.Drawing.Point(116, 309);
            this.checkBoxTextForHI.Name = "checkBoxTextForHI";
            this.checkBoxTextForHI.Size = new System.Drawing.Size(106, 17);
            this.checkBoxTextForHI.TabIndex = 19;
            this.checkBoxTextForHI.Text = "Hearing Impaired";
            this.checkBoxTextForHI.UseVisualStyleBackColor = true;
            // 
            // buttonSearchIMDb
            // 
            this.buttonSearchIMDb.Location = new System.Drawing.Point(207, 53);
            this.buttonSearchIMDb.Name = "buttonSearchIMDb";
            this.buttonSearchIMDb.Size = new System.Drawing.Size(24, 23);
            this.buttonSearchIMDb.TabIndex = 4;
            this.buttonSearchIMDb.Text = "...";
            this.buttonSearchIMDb.UseVisualStyleBackColor = true;
            this.buttonSearchIMDb.Click += new System.EventHandler(this.buttonSearchIMDb_Click);
            // 
            // buttonOpenVideo
            // 
            this.buttonOpenVideo.Location = new System.Drawing.Point(409, 158);
            this.buttonOpenVideo.Name = "buttonOpenVideo";
            this.buttonOpenVideo.Size = new System.Drawing.Size(24, 23);
            this.buttonOpenVideo.TabIndex = 14;
            this.buttonOpenVideo.Text = "...";
            this.buttonOpenVideo.UseVisualStyleBackColor = true;
            this.buttonOpenVideo.Click += new System.EventHandler(this.buttonOpenVideo_Click);
            // 
            // linkLabelSearchImdb
            // 
            this.linkLabelSearchImdb.AutoSize = true;
            this.linkLabelSearchImdb.Location = new System.Drawing.Point(236, 58);
            this.linkLabelSearchImdb.Name = "linkLabelSearchImdb";
            this.linkLabelSearchImdb.Size = new System.Drawing.Size(72, 13);
            this.linkLabelSearchImdb.TabIndex = 5;
            this.linkLabelSearchImdb.TabStop = true;
            this.linkLabelSearchImdb.Text = "Search online";
            this.linkLabelSearchImdb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelSearchImdb_LinkClicked);
            // 
            // textBoxComment
            // 
            this.textBoxComment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxComment.Location = new System.Drawing.Point(116, 212);
            this.textBoxComment.Multiline = true;
            this.textBoxComment.Name = "textBoxComment";
            this.textBoxComment.Size = new System.Drawing.Size(317, 91);
            this.textBoxComment.TabIndex = 18;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 215);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Comment";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Release name";
            // 
            // textBoxReleaseName
            // 
            this.textBoxReleaseName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxReleaseName.Location = new System.Drawing.Point(116, 81);
            this.textBoxReleaseName.Name = "textBoxReleaseName";
            this.textBoxReleaseName.Size = new System.Drawing.Size(317, 20);
            this.textBoxReleaseName.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "IMDb id";
            // 
            // textBoxImdbId
            // 
            this.textBoxImdbId.Location = new System.Drawing.Point(116, 55);
            this.textBoxImdbId.Name = "textBoxImdbId";
            this.textBoxImdbId.Size = new System.Drawing.Size(85, 20);
            this.textBoxImdbId.TabIndex = 3;
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(116, 28);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(317, 21);
            this.comboBoxLanguage.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Language";
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(14, 445);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 2;
            this.labelStatus.Text = "labelStatus";
            // 
            // openFileDialogVideo
            // 
            this.openFileDialogVideo.FileName = "openFileDialog1";
            // 
            // PluginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 500);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.groupBoxSubtitleInfo);
            this.Controls.Add(this.groupBoxLogin);
            this.Controls.Add(this.buttonUpload);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.linkLabelUploadManually);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PluginForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Upload to OpenSubtitles.org";
            this.Load += new System.EventHandler(this.PluginForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PluginForm_KeyDown);
            this.groupBoxLogin.ResumeLayout(false);
            this.groupBoxLogin.PerformLayout();
            this.groupBoxSubtitleInfo.ResumeLayout(false);
            this.groupBoxSubtitleInfo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonUpload;
        private System.Windows.Forms.TextBox textBoxSubtitleFileName;
        private System.Windows.Forms.LinkLabel linkLabelUploadManually;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelSubtitleFileName;
        private System.Windows.Forms.Label labelMovieFileName;
        private System.Windows.Forms.TextBox textBoxMovieFileName;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.GroupBox groupBoxLogin;
        private System.Windows.Forms.GroupBox groupBoxSubtitleInfo;
        private System.Windows.Forms.LinkLabel linkLabelSearchImdb;
        private System.Windows.Forms.TextBox textBoxComment;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxReleaseName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxImdbId;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button buttonOpenVideo;
        private System.Windows.Forms.OpenFileDialog openFileDialogVideo;
        private System.Windows.Forms.Button buttonSearchIMDb;
        private System.Windows.Forms.LinkLabel linkLabelRegister;
        private System.Windows.Forms.CheckBox checkBoxTextForHI;
        private System.Windows.Forms.CheckBox checkBoxHD;
        private System.Windows.Forms.ComboBox comboBoxEncoding;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBoxFrameRate;
        private System.Windows.Forms.Label labelFrameRate;
    }
}