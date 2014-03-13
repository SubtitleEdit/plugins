namespace tmdb_casing
{
    partial class MovieSeacher
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
            this.buttonSeach = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBoxMovieInfo = new System.Windows.Forms.TextBox();
            this.radioButtonTMDB = new System.Windows.Forms.RadioButton();
            this.radioButtonIMDB = new System.Windows.Forms.RadioButton();
            this.radioButtonTitle = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // buttonSeach
            // 
            this.buttonSeach.Location = new System.Drawing.Point(470, 26);
            this.buttonSeach.Name = "buttonSeach";
            this.buttonSeach.Size = new System.Drawing.Size(75, 23);
            this.buttonSeach.TabIndex = 6;
            this.buttonSeach.Text = "Seach";
            this.buttonSeach.UseVisualStyleBackColor = true;
            this.buttonSeach.Click += new System.EventHandler(this.buttonSeach_Click);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.Location = new System.Drawing.Point(12, 55);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(533, 232);
            this.listView1.TabIndex = 7;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "ID";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Title";
            this.columnHeader2.Width = 384;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(389, 293);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 8;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(470, 293);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // textBoxMovieInfo
            // 
            this.textBoxMovieInfo.Location = new System.Drawing.Point(12, 29);
            this.textBoxMovieInfo.Name = "textBoxMovieInfo";
            this.textBoxMovieInfo.Size = new System.Drawing.Size(452, 20);
            this.textBoxMovieInfo.TabIndex = 10;
            // 
            // radioButtonTMDB
            // 
            this.radioButtonTMDB.AutoSize = true;
            this.radioButtonTMDB.Location = new System.Drawing.Point(13, 6);
            this.radioButtonTMDB.Name = "radioButtonTMDB";
            this.radioButtonTMDB.Size = new System.Drawing.Size(84, 17);
            this.radioButtonTMDB.TabIndex = 11;
            this.radioButtonTMDB.TabStop = true;
            this.radioButtonTMDB.Text = "TMDB Code";
            this.radioButtonTMDB.UseVisualStyleBackColor = true;
            // 
            // radioButtonIMDB
            // 
            this.radioButtonIMDB.AutoSize = true;
            this.radioButtonIMDB.Location = new System.Drawing.Point(105, 6);
            this.radioButtonIMDB.Name = "radioButtonIMDB";
            this.radioButtonIMDB.Size = new System.Drawing.Size(80, 17);
            this.radioButtonIMDB.TabIndex = 12;
            this.radioButtonIMDB.TabStop = true;
            this.radioButtonIMDB.Text = "IMDB Code";
            this.radioButtonIMDB.UseVisualStyleBackColor = true;
            // 
            // radioButtonTitle
            // 
            this.radioButtonTitle.AutoSize = true;
            this.radioButtonTitle.Location = new System.Drawing.Point(196, 6);
            this.radioButtonTitle.Name = "radioButtonTitle";
            this.radioButtonTitle.Size = new System.Drawing.Size(45, 17);
            this.radioButtonTitle.TabIndex = 13;
            this.radioButtonTitle.TabStop = true;
            this.radioButtonTitle.Text = "Title";
            this.radioButtonTitle.UseVisualStyleBackColor = true;
            // 
            // MovieSeacher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(557, 322);
            this.Controls.Add(this.radioButtonTitle);
            this.Controls.Add(this.radioButtonIMDB);
            this.Controls.Add(this.radioButtonTMDB);
            this.Controls.Add(this.textBoxMovieInfo);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.buttonSeach);
            this.Name = "MovieSeacher";
            this.Text = "MovieSeach";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonSeach;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textBoxMovieInfo;
        private System.Windows.Forms.RadioButton radioButtonTMDB;
        private System.Windows.Forms.RadioButton radioButtonIMDB;
        private System.Windows.Forms.RadioButton radioButtonTitle;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
    }
}