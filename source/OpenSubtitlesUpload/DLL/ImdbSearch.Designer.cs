namespace OpenSubtitlesUpload
{
    partial class ImdbSearch
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
            this.labelSearch = new System.Windows.Forms.Label();
            this.textBoxSearchQuery = new System.Windows.Forms.TextBox();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.listViewSearchResults = new System.Windows.Forms.ListView();
            this.columnHeaderImdbId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelSearch
            // 
            this.labelSearch.AutoSize = true;
            this.labelSearch.Location = new System.Drawing.Point(12, 25);
            this.labelSearch.Name = "labelSearch";
            this.labelSearch.Size = new System.Drawing.Size(85, 13);
            this.labelSearch.TabIndex = 0;
            this.labelSearch.Text = "Search IMDb for";
            // 
            // textBoxSearchQuery
            // 
            this.textBoxSearchQuery.Location = new System.Drawing.Point(104, 22);
            this.textBoxSearchQuery.Name = "textBoxSearchQuery";
            this.textBoxSearchQuery.Size = new System.Drawing.Size(310, 20);
            this.textBoxSearchQuery.TabIndex = 1;
            this.textBoxSearchQuery.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxSearchQuery_KeyDown);
            // 
            // buttonSearch
            // 
            this.buttonSearch.Location = new System.Drawing.Point(420, 20);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(75, 23);
            this.buttonSearch.TabIndex = 2;
            this.buttonSearch.Text = "Search";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // listViewSearchResults
            // 
            this.listViewSearchResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewSearchResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderImdbId,
            this.columnHeaderTitle});
            this.listViewSearchResults.FullRowSelect = true;
            this.listViewSearchResults.GridLines = true;
            this.listViewSearchResults.HideSelection = false;
            this.listViewSearchResults.Location = new System.Drawing.Point(15, 63);
            this.listViewSearchResults.MultiSelect = false;
            this.listViewSearchResults.Name = "listViewSearchResults";
            this.listViewSearchResults.Size = new System.Drawing.Size(480, 238);
            this.listViewSearchResults.TabIndex = 3;
            this.listViewSearchResults.UseCompatibleStateImageBehavior = false;
            this.listViewSearchResults.View = System.Windows.Forms.View.Details;
            this.listViewSearchResults.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewSearchResults_MouseDoubleClick);
            // 
            // columnHeaderImdbId
            // 
            this.columnHeaderImdbId.Text = "Imdb ID";
            this.columnHeaderImdbId.Width = 100;
            // 
            // columnHeaderTitle
            // 
            this.columnHeaderTitle.Text = "Title";
            this.columnHeaderTitle.Width = 350;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(339, 307);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(420, 307);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(13, 316);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 6;
            this.labelStatus.Text = "labelStatus";
            // 
            // ImdbSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(507, 342);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.listViewSearchResults);
            this.Controls.Add(this.buttonSearch);
            this.Controls.Add(this.labelSearch);
            this.Controls.Add(this.textBoxSearchQuery);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(523, 380);
            this.Name = "ImdbSearch";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "IMDb search";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelSearch;
        private System.Windows.Forms.TextBox textBoxSearchQuery;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.ListView listViewSearchResults;
        private System.Windows.Forms.ColumnHeader columnHeaderImdbId;
        private System.Windows.Forms.ColumnHeader columnHeaderTitle;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelStatus;
    }
}