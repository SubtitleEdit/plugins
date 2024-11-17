
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
            this.panelBackgroundColor = new System.Windows.Forms.Panel();
            this.buttonBackgroundColor = new System.Windows.Forms.Button();
            this.panelScreenRes = new System.Windows.Forms.Panel();
            this.buttonScreenRes = new System.Windows.Forms.Button();
            this.checkBoxAutoLoadBackgroundFromSE = new System.Windows.Forms.CheckBox();
            this.checkBoxHideSettingsAndTreeView = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // panelLineColor
            // 
            this.panelLineColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLineColor.Location = new System.Drawing.Point(199, 15);
            this.panelLineColor.Name = "panelLineColor";
            this.panelLineColor.Size = new System.Drawing.Size(21, 20);
            this.panelLineColor.TabIndex = 3;
            this.panelLineColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelLineColor_MouseClick);
            // 
            // buttonLineColor
            // 
            this.buttonLineColor.Location = new System.Drawing.Point(12, 12);
            this.buttonLineColor.Name = "buttonLineColor";
            this.buttonLineColor.Size = new System.Drawing.Size(181, 23);
            this.buttonLineColor.TabIndex = 2;
            this.buttonLineColor.Text = "Shape color";
            this.buttonLineColor.UseVisualStyleBackColor = true;
            this.buttonLineColor.Click += new System.EventHandler(this.buttonLineColor_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(330, 204);
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
            this.buttonOK.Location = new System.Drawing.Point(249, 204);
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
            this.panelLineActiveColor.Location = new System.Drawing.Point(199, 44);
            this.panelLineActiveColor.Name = "panelLineActiveColor";
            this.panelLineActiveColor.Size = new System.Drawing.Size(21, 20);
            this.panelLineActiveColor.TabIndex = 7;
            this.panelLineActiveColor.Click += new System.EventHandler(this.panelLineActiveColor_Click);
            // 
            // buttonNewLineColor
            // 
            this.buttonNewLineColor.Location = new System.Drawing.Point(12, 41);
            this.buttonNewLineColor.Name = "buttonNewLineColor";
            this.buttonNewLineColor.Size = new System.Drawing.Size(181, 23);
            this.buttonNewLineColor.TabIndex = 6;
            this.buttonNewLineColor.Text = "New/Active color";
            this.buttonNewLineColor.UseVisualStyleBackColor = true;
            this.buttonNewLineColor.Click += new System.EventHandler(this.buttonNewLineColor_Click);
            // 
            // panelBackgroundColor
            // 
            this.panelBackgroundColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBackgroundColor.Location = new System.Drawing.Point(199, 73);
            this.panelBackgroundColor.Name = "panelBackgroundColor";
            this.panelBackgroundColor.Size = new System.Drawing.Size(21, 20);
            this.panelBackgroundColor.TabIndex = 9;
            this.panelBackgroundColor.Click += new System.EventHandler(this.panelBackgroundColor_Click);
            // 
            // buttonBackgroundColor
            // 
            this.buttonBackgroundColor.Location = new System.Drawing.Point(12, 70);
            this.buttonBackgroundColor.Name = "buttonBackgroundColor";
            this.buttonBackgroundColor.Size = new System.Drawing.Size(181, 23);
            this.buttonBackgroundColor.TabIndex = 8;
            this.buttonBackgroundColor.Text = "Background color";
            this.buttonBackgroundColor.UseVisualStyleBackColor = true;
            this.buttonBackgroundColor.Click += new System.EventHandler(this.buttonBackgroundColor_Click);
            // 
            // panelScreenRes
            // 
            this.panelScreenRes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelScreenRes.Location = new System.Drawing.Point(199, 102);
            this.panelScreenRes.Name = "panelScreenRes";
            this.panelScreenRes.Size = new System.Drawing.Size(21, 20);
            this.panelScreenRes.TabIndex = 11;
            // 
            // buttonScreenRes
            // 
            this.buttonScreenRes.Location = new System.Drawing.Point(12, 99);
            this.buttonScreenRes.Name = "buttonScreenRes";
            this.buttonScreenRes.Size = new System.Drawing.Size(181, 23);
            this.buttonScreenRes.TabIndex = 10;
            this.buttonScreenRes.Text = "Screen resolution";
            this.buttonScreenRes.UseVisualStyleBackColor = true;
            this.buttonScreenRes.Click += new System.EventHandler(this.buttonScreenRes_Click);
            // 
            // checkBoxAutoLoadBackgroundFromSE
            // 
            this.checkBoxAutoLoadBackgroundFromSE.AutoSize = true;
            this.checkBoxAutoLoadBackgroundFromSE.Location = new System.Drawing.Point(12, 167);
            this.checkBoxAutoLoadBackgroundFromSE.Name = "checkBoxAutoLoadBackgroundFromSE";
            this.checkBoxAutoLoadBackgroundFromSE.Size = new System.Drawing.Size(220, 17);
            this.checkBoxAutoLoadBackgroundFromSE.TabIndex = 12;
            this.checkBoxAutoLoadBackgroundFromSE.Text = "Auto load background from SE video pos";
            this.checkBoxAutoLoadBackgroundFromSE.UseVisualStyleBackColor = true;
            // 
            // checkBoxHideSettingsAndTreeView
            // 
            this.checkBoxHideSettingsAndTreeView.AutoSize = true;
            this.checkBoxHideSettingsAndTreeView.Location = new System.Drawing.Point(12, 144);
            this.checkBoxHideSettingsAndTreeView.Name = "checkBoxHideSettingsAndTreeView";
            this.checkBoxHideSettingsAndTreeView.Size = new System.Drawing.Size(151, 17);
            this.checkBoxHideSettingsAndTreeView.TabIndex = 13;
            this.checkBoxHideSettingsAndTreeView.Text = "Hide settings and treeview";
            this.checkBoxHideSettingsAndTreeView.UseVisualStyleBackColor = true;
            // 
            // FormAssaDrawSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(417, 239);
            this.Controls.Add(this.checkBoxHideSettingsAndTreeView);
            this.Controls.Add(this.checkBoxAutoLoadBackgroundFromSE);
            this.Controls.Add(this.panelScreenRes);
            this.Controls.Add(this.buttonScreenRes);
            this.Controls.Add(this.panelBackgroundColor);
            this.Controls.Add(this.buttonBackgroundColor);
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
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelLineColor;
        private System.Windows.Forms.Button buttonLineColor;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Panel panelLineActiveColor;
        private System.Windows.Forms.Button buttonNewLineColor;
        private System.Windows.Forms.Panel panelBackgroundColor;
        private System.Windows.Forms.Button buttonBackgroundColor;
        private System.Windows.Forms.Panel panelScreenRes;
        private System.Windows.Forms.Button buttonScreenRes;
        private System.Windows.Forms.CheckBox checkBoxAutoLoadBackgroundFromSE;
        private System.Windows.Forms.CheckBox checkBoxHideSettingsAndTreeView;
    }
}