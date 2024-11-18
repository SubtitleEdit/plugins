
namespace AssaDraw
{
    partial class FormAssaDrawHelp
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAssaDrawHelp));
            this.buttonOK = new System.Windows.Forms.Button();
            this.richTextBoxHelpInfo = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(356, 492);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // richTextBoxHelpInfo
            // 
            this.richTextBoxHelpInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxHelpInfo.BackColor = System.Drawing.SystemColors.Control;
            this.richTextBoxHelpInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxHelpInfo.Location = new System.Drawing.Point(12, 12);
            this.richTextBoxHelpInfo.Name = "richTextBoxHelpInfo";
            this.richTextBoxHelpInfo.Size = new System.Drawing.Size(419, 474);
            this.richTextBoxHelpInfo.TabIndex = 6;
            this.richTextBoxHelpInfo.Text = resources.GetString("richTextBoxHelpInfo.Text");
            // 
            // FormAssaDrawHelp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 527);
            this.Controls.Add(this.richTextBoxHelpInfo);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAssaDrawHelp";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Help";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormAssaDrawHelp_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.RichTextBox richTextBoxHelpInfo;
    }
}