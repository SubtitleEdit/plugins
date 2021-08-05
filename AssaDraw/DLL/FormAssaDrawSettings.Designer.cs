
namespace AssaDraw
{
    partial class FormAssaDrawSettings
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
            this.panelLineColor = new System.Windows.Forms.Panel();
            this.buttonLineColor = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.panelLineActiveColor = new System.Windows.Forms.Panel();
            this.buttonNewLineColor = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // panelLineColor
            // 
            this.panelLineColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLineColor.Location = new System.Drawing.Point(166, 15);
            this.panelLineColor.Name = "panelLineColor";
            this.panelLineColor.Size = new System.Drawing.Size(21, 20);
            this.panelLineColor.TabIndex = 3;
            this.panelLineColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelLineColor_MouseClick);
            // 
            // buttonLineColor
            // 
            this.buttonLineColor.Location = new System.Drawing.Point(12, 12);
            this.buttonLineColor.Name = "buttonLineColor";
            this.buttonLineColor.Size = new System.Drawing.Size(148, 23);
            this.buttonLineColor.TabIndex = 2;
            this.buttonLineColor.Text = "Line color";
            this.buttonLineColor.UseVisualStyleBackColor = true;
            this.buttonLineColor.Click += new System.EventHandler(this.buttonLineColor_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(291, 158);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(210, 158);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // panelLineActiveColor
            // 
            this.panelLineActiveColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLineActiveColor.Location = new System.Drawing.Point(166, 44);
            this.panelLineActiveColor.Name = "panelLineActiveColor";
            this.panelLineActiveColor.Size = new System.Drawing.Size(21, 20);
            this.panelLineActiveColor.TabIndex = 7;
            this.panelLineActiveColor.Click += new System.EventHandler(this.panelLineActiveColor_Click);
            // 
            // buttonNewLineColor
            // 
            this.buttonNewLineColor.Location = new System.Drawing.Point(12, 41);
            this.buttonNewLineColor.Name = "buttonNewLineColor";
            this.buttonNewLineColor.Size = new System.Drawing.Size(148, 23);
            this.buttonNewLineColor.TabIndex = 6;
            this.buttonNewLineColor.Text = "New line color";
            this.buttonNewLineColor.UseVisualStyleBackColor = true;
            this.buttonNewLineColor.Click += new System.EventHandler(this.buttonNewLineColor_Click);
            // 
            // FormAssaDrawSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 193);
            this.Controls.Add(this.panelLineActiveColor);
            this.Controls.Add(this.buttonNewLineColor);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.panelLineColor);
            this.Controls.Add(this.buttonLineColor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAssaDrawSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormAssaDrawSettings_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelLineColor;
        private System.Windows.Forms.Button buttonLineColor;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Panel panelLineActiveColor;
        private System.Windows.Forms.Button buttonNewLineColor;
    }
}