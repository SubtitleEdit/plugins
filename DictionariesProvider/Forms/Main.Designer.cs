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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("ListViewGroup", System.Windows.Forms.HorizontalAlignment.Left);
            this.listViewDownloadUrls = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.linkLabelOpenDicFolder = new System.Windows.Forms.LinkLabel();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.buttonAddLink = new System.Windows.Forms.Button();
            this.textBoxDescription = new System.Windows.Forms.TextBox();
            this.textBoxNativeName = new System.Windows.Forms.TextBox();
            this.comboBoxDownloadLinks = new System.Windows.Forms.ComboBox();
            this.textBoxEnglishName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonAddDictionary = new System.Windows.Forms.Button();
            this.buttonUpdateStatus = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importFormXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBoxStatus = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewDownloadUrls
            // 
            this.listViewDownloadUrls.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewDownloadUrls.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listViewDownloadUrls.FullRowSelect = true;
            this.listViewDownloadUrls.GridLines = true;
            listViewGroup1.Header = "ListViewGroup";
            listViewGroup1.Name = "listViewGroup1";
            this.listViewDownloadUrls.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1});
            this.listViewDownloadUrls.HideSelection = false;
            this.listViewDownloadUrls.Location = new System.Drawing.Point(13, 183);
            this.listViewDownloadUrls.Margin = new System.Windows.Forms.Padding(4);
            this.listViewDownloadUrls.Name = "listViewDownloadUrls";
            this.listViewDownloadUrls.Size = new System.Drawing.Size(870, 303);
            this.listViewDownloadUrls.TabIndex = 3;
            this.listViewDownloadUrls.UseCompatibleStateImageBehavior = false;
            this.listViewDownloadUrls.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Dictionary url";
            this.columnHeader1.Width = 608;
            // 
            // linkLabelOpenDicFolder
            // 
            this.linkLabelOpenDicFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelOpenDicFolder.AutoSize = true;
            this.linkLabelOpenDicFolder.Location = new System.Drawing.Point(10, 500);
            this.linkLabelOpenDicFolder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabelOpenDicFolder.Name = "linkLabelOpenDicFolder";
            this.linkLabelOpenDicFolder.Size = new System.Drawing.Size(139, 16);
            this.linkLabelOpenDicFolder.TabIndex = 4;
            this.linkLabelOpenDicFolder.TabStop = true;
            this.linkLabelOpenDicFolder.Text = "Open dictionary folder";
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(783, 494);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(100, 28);
            this.buttonOk.TabIndex = 6;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // buttonDownload
            // 
            this.buttonDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownload.Location = new System.Drawing.Point(629, 494);
            this.buttonDownload.Margin = new System.Windows.Forms.Padding(4);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(146, 28);
            this.buttonDownload.TabIndex = 5;
            this.buttonDownload.Text = "Download selected";
            this.buttonDownload.UseVisualStyleBackColor = true;
            this.buttonDownload.Click += new System.EventHandler(this.ButtonDownload_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.linkLabel1);
            this.groupBox1.Controls.Add(this.buttonAddLink);
            this.groupBox1.Controls.Add(this.textBoxDescription);
            this.groupBox1.Controls.Add(this.textBoxNativeName);
            this.groupBox1.Controls.Add(this.comboBoxDownloadLinks);
            this.groupBox1.Controls.Add(this.textBoxEnglishName);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(13, 23);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(870, 118);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Add / Update dictionary";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel1.Location = new System.Drawing.Point(444, 83);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(252, 26);
            this.linkLabel1.TabIndex = 9;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Several url can be added by clicking the \"Add Link\"\r\nbutton.";
            // 
            // buttonAddLink
            // 
            this.buttonAddLink.Location = new System.Drawing.Point(762, 52);
            this.buttonAddLink.Margin = new System.Windows.Forms.Padding(4);
            this.buttonAddLink.Name = "buttonAddLink";
            this.buttonAddLink.Size = new System.Drawing.Size(100, 28);
            this.buttonAddLink.TabIndex = 8;
            this.buttonAddLink.Text = "Add link";
            this.buttonAddLink.UseVisualStyleBackColor = true;
            this.buttonAddLink.Click += new System.EventHandler(this.ButtonAddLink_Click);
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.Location = new System.Drawing.Point(303, 57);
            this.textBoxDescription.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.Size = new System.Drawing.Size(132, 22);
            this.textBoxDescription.TabIndex = 5;
            // 
            // textBoxNativeName
            // 
            this.textBoxNativeName.Location = new System.Drawing.Point(157, 57);
            this.textBoxNativeName.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxNativeName.Name = "textBoxNativeName";
            this.textBoxNativeName.Size = new System.Drawing.Size(132, 22);
            this.textBoxNativeName.TabIndex = 3;
            // 
            // comboBoxDownloadLinks
            // 
            this.comboBoxDownloadLinks.FormattingEnabled = true;
            this.comboBoxDownloadLinks.Location = new System.Drawing.Point(447, 55);
            this.comboBoxDownloadLinks.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxDownloadLinks.Name = "comboBoxDownloadLinks";
            this.comboBoxDownloadLinks.Size = new System.Drawing.Size(307, 24);
            this.comboBoxDownloadLinks.TabIndex = 7;
            // 
            // textBoxEnglishName
            // 
            this.textBoxEnglishName.Location = new System.Drawing.Point(12, 57);
            this.textBoxEnglishName.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxEnglishName.Name = "textBoxEnglishName";
            this.textBoxEnglishName.Size = new System.Drawing.Size(132, 22);
            this.textBoxEnglishName.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(299, 37);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 16);
            this.label4.TabIndex = 4;
            this.label4.Text = "Description:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(153, 37);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Native name:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 37);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "English name:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(444, 37);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "Download link:";
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemove.Location = new System.Drawing.Point(783, 147);
            this.buttonRemove.Margin = new System.Windows.Forms.Padding(4);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(100, 28);
            this.buttonRemove.TabIndex = 2;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            // 
            // buttonAddDictionary
            // 
            this.buttonAddDictionary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddDictionary.Location = new System.Drawing.Point(675, 147);
            this.buttonAddDictionary.Margin = new System.Windows.Forms.Padding(4);
            this.buttonAddDictionary.Name = "buttonAddDictionary";
            this.buttonAddDictionary.Size = new System.Drawing.Size(100, 28);
            this.buttonAddDictionary.TabIndex = 1;
            this.buttonAddDictionary.Text = "Add";
            this.buttonAddDictionary.UseVisualStyleBackColor = true;
            this.buttonAddDictionary.Click += new System.EventHandler(this.ButtonAddDictionary_Click);
            // 
            // buttonUpdateStatus
            // 
            this.buttonUpdateStatus.Location = new System.Drawing.Point(13, 148);
            this.buttonUpdateStatus.Name = "buttonUpdateStatus";
            this.buttonUpdateStatus.Size = new System.Drawing.Size(100, 28);
            this.buttonUpdateStatus.TabIndex = 7;
            this.buttonUpdateStatus.Text = "Update status";
            this.buttonUpdateStatus.UseVisualStyleBackColor = true;
            this.buttonUpdateStatus.Click += new System.EventHandler(this.ButtonUpdateStatus_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(896, 24);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importFormXMLToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // importFormXMLToolStripMenuItem
            // 
            this.importFormXMLToolStripMenuItem.Name = "importFormXMLToolStripMenuItem";
            this.importFormXMLToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.importFormXMLToolStripMenuItem.Text = "Import form XML";
            this.importFormXMLToolStripMenuItem.Click += new System.EventHandler(this.ImportFormXMLToolStripMenuItem_Click);
            // 
            // checkBoxStatus
            // 
            this.checkBoxStatus.AutoSize = true;
            this.checkBoxStatus.Location = new System.Drawing.Point(133, 156);
            this.checkBoxStatus.Name = "checkBoxStatus";
            this.checkBoxStatus.Size = new System.Drawing.Size(64, 20);
            this.checkBoxStatus.TabIndex = 9;
            this.checkBoxStatus.Text = "Status";
            this.checkBoxStatus.UseVisualStyleBackColor = true;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(896, 530);
            this.Controls.Add(this.checkBoxStatus);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.buttonUpdateStatus);
            this.Controls.Add(this.buttonAddDictionary);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonDownload);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.linkLabelOpenDicFolder);
            this.Controls.Add(this.listViewDownloadUrls);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Main";
            this.Text = "Main";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewDownloadUrls;
        private System.Windows.Forms.LinkLabel linkLabelOpenDicFolder;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.ColumnHeader columnHeader1;
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
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Button buttonUpdateStatus;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importFormXMLToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxStatus;
    }
}