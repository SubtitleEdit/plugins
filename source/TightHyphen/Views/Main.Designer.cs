namespace Nikse.SubtitleEdit.PluginLogic.Views
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
            this.listViewPreview = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.linkLabelDonate = new System.Windows.Forms.LinkLabel();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.labelExample = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // listViewPreview
            // 
            this.listViewPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewPreview.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader1,
            this.columnHeader2});
            this.listViewPreview.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewPreview.FullRowSelect = true;
            this.listViewPreview.GridLines = true;
            this.listViewPreview.HideSelection = false;
            this.listViewPreview.Location = new System.Drawing.Point(24, 40);
            this.listViewPreview.Name = "listViewPreview";
            this.listViewPreview.Size = new System.Drawing.Size(752, 369);
            this.listViewPreview.TabIndex = 0;
            this.listViewPreview.UseCompatibleStateImageBehavior = false;
            this.listViewPreview.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Original Text";
            this.columnHeader1.Width = 346;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Changed Text";
            this.columnHeader2.Width = 398;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(620, 415);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(701, 415);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(21, 425);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(37, 13);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "Status";
            // 
            // linkLabelDonate
            // 
            this.linkLabelDonate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelDonate.AutoSize = true;
            this.linkLabelDonate.Location = new System.Drawing.Point(676, 24);
            this.linkLabelDonate.Name = "linkLabelDonate";
            this.linkLabelDonate.Size = new System.Drawing.Size(100, 13);
            this.linkLabelDonate.TabIndex = 4;
            this.linkLabelDonate.TabStop = true;
            this.linkLabelDonate.Text = "Donate with PayPal";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "#";
            // 
            // labelExample
            // 
            this.labelExample.AutoSize = true;
            this.labelExample.Location = new System.Drawing.Point(24, 23);
            this.labelExample.Name = "labelExample";
            this.labelExample.Size = new System.Drawing.Size(121, 13);
            this.labelExample.TabIndex = 5;
            this.labelExample.Text = "e.g: - Foobar => -Foobar";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.labelExample);
            this.Controls.Add(this.linkLabelDonate);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.listViewPreview);
            this.Name = "Main";
            this.Text = "Tight Hyphens";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewPreview;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.LinkLabel linkLabelDonate;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Label labelExample;
    }
}