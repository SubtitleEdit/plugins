namespace Nikse.SubtitleEdit.PluginLogic.Forms
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
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("ListViewGroup", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("ivandro ismael");
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("ismael");
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.linkLabelOpenDicFolder = new System.Windows.Forms.LinkLabel();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBoxDescription = new System.Windows.Forms.TextBox();
            this.textBoxNativeName = new System.Windows.Forms.TextBox();
            this.textBoxEnglishName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonAddDictionary = new System.Windows.Forms.Button();
            this.comboBoxDownloadLinks = new System.Windows.Forms.ComboBox();
            this.buttonAddLink = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            listViewGroup3.Header = "ListViewGroup";
            listViewGroup3.Name = "listViewGroup1";
            this.listView1.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup3});
            this.listView1.HideSelection = false;
            listViewItem5.Group = listViewGroup3;
            this.listView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem5,
            listViewItem6});
            this.listView1.Location = new System.Drawing.Point(27, 143);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(618, 247);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "English name";
            this.columnHeader1.Width = 113;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Native name";
            this.columnHeader2.Width = 158;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Download links";
            this.columnHeader3.Width = 129;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Description";
            this.columnHeader4.Width = 117;
            // 
            // linkLabelOpenDicFolder
            // 
            this.linkLabelOpenDicFolder.AutoSize = true;
            this.linkLabelOpenDicFolder.Location = new System.Drawing.Point(24, 406);
            this.linkLabelOpenDicFolder.Name = "linkLabelOpenDicFolder";
            this.linkLabelOpenDicFolder.Size = new System.Drawing.Size(110, 13);
            this.linkLabelOpenDicFolder.TabIndex = 1;
            this.linkLabelOpenDicFolder.TabStop = true;
            this.linkLabelOpenDicFolder.Text = "Open dictionary folder";
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(570, 396);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 2;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonDownload
            // 
            this.buttonDownload.Location = new System.Drawing.Point(489, 396);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(75, 23);
            this.buttonDownload.TabIndex = 3;
            this.buttonDownload.Text = "Download";
            this.buttonDownload.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonAddLink);
            this.groupBox1.Controls.Add(this.textBoxDescription);
            this.groupBox1.Controls.Add(this.textBoxNativeName);
            this.groupBox1.Controls.Add(this.comboBoxDownloadLinks);
            this.groupBox1.Controls.Add(this.textBoxEnglishName);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(27, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(618, 96);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Add/Update dictionary";
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.Location = new System.Drawing.Point(227, 46);
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.Size = new System.Drawing.Size(100, 20);
            this.textBoxDescription.TabIndex = 1;
            // 
            // textBoxNativeName
            // 
            this.textBoxNativeName.Location = new System.Drawing.Point(118, 46);
            this.textBoxNativeName.Name = "textBoxNativeName";
            this.textBoxNativeName.Size = new System.Drawing.Size(100, 20);
            this.textBoxNativeName.TabIndex = 1;
            // 
            // textBoxEnglishName
            // 
            this.textBoxEnglishName.Location = new System.Drawing.Point(9, 46);
            this.textBoxEnglishName.Name = "textBoxEnglishName";
            this.textBoxEnglishName.Size = new System.Drawing.Size(100, 20);
            this.textBoxEnglishName.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(224, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Description:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(333, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Download link:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(115, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Native name:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "English name:";
            // 
            // buttonRemove
            // 
            this.buttonRemove.Location = new System.Drawing.Point(570, 114);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(75, 23);
            this.buttonRemove.TabIndex = 5;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            // 
            // buttonAddDictionary
            // 
            this.buttonAddDictionary.Location = new System.Drawing.Point(489, 114);
            this.buttonAddDictionary.Name = "buttonAddDictionary";
            this.buttonAddDictionary.Size = new System.Drawing.Size(75, 23);
            this.buttonAddDictionary.TabIndex = 5;
            this.buttonAddDictionary.Text = "Add";
            this.buttonAddDictionary.UseVisualStyleBackColor = true;
            // 
            // comboBoxDownloadLinks
            // 
            this.comboBoxDownloadLinks.FormattingEnabled = true;
            this.comboBoxDownloadLinks.Location = new System.Drawing.Point(336, 46);
            this.comboBoxDownloadLinks.Name = "comboBoxDownloadLinks";
            this.comboBoxDownloadLinks.Size = new System.Drawing.Size(195, 21);
            this.comboBoxDownloadLinks.TabIndex = 2;
            // 
            // buttonAddLink
            // 
            this.buttonAddLink.Location = new System.Drawing.Point(537, 45);
            this.buttonAddLink.Name = "buttonAddLink";
            this.buttonAddLink.Size = new System.Drawing.Size(75, 23);
            this.buttonAddLink.TabIndex = 3;
            this.buttonAddLink.Text = "Add link";
            this.buttonAddLink.UseVisualStyleBackColor = true;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(672, 431);
            this.Controls.Add(this.buttonAddDictionary);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonDownload);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.linkLabelOpenDicFolder);
            this.Controls.Add(this.listView1);
            this.Name = "Main";
            this.Text = "Main";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.LinkLabel linkLabelOpenDicFolder;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonAddDictionary;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.TextBox textBoxNativeName;
        private System.Windows.Forms.TextBox textBoxEnglishName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxDownloadLinks;
        private System.Windows.Forms.Button buttonAddLink;
    }
}