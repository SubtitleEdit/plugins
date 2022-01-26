namespace AssaDraw
{
    sealed partial class SetColor
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
            this.listViewStyles = new System.Windows.Forms.ListView();
            this.columnHeaderStyleName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelPickLayer = new System.Windows.Forms.Label();
            this.buttonColor = new System.Windows.Forms.Button();
            this.panelColorPicker = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // listViewStyles
            // 
            this.listViewStyles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewStyles.CheckBoxes = true;
            this.listViewStyles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderStyleName});
            this.listViewStyles.FullRowSelect = true;
            this.listViewStyles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewStyles.HideSelection = false;
            this.listViewStyles.Location = new System.Drawing.Point(12, 39);
            this.listViewStyles.Name = "listViewStyles";
            this.listViewStyles.Size = new System.Drawing.Size(565, 159);
            this.listViewStyles.TabIndex = 4;
            this.listViewStyles.UseCompatibleStateImageBehavior = false;
            this.listViewStyles.View = System.Windows.Forms.View.Details;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(502, 260);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(421, 260);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 9;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelPickLayer
            // 
            this.labelPickLayer.AutoSize = true;
            this.labelPickLayer.Location = new System.Drawing.Point(12, 23);
            this.labelPickLayer.Name = "labelPickLayer";
            this.labelPickLayer.Size = new System.Drawing.Size(73, 13);
            this.labelPickLayer.TabIndex = 8;
            this.labelPickLayer.Text = "Choose layers";
            // 
            // buttonColor
            // 
            this.buttonColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonColor.Location = new System.Drawing.Point(12, 204);
            this.buttonColor.Name = "buttonColor";
            this.buttonColor.Size = new System.Drawing.Size(162, 23);
            this.buttonColor.TabIndex = 11;
            this.buttonColor.Text = "Pick color";
            this.buttonColor.UseVisualStyleBackColor = true;
            this.buttonColor.Click += new System.EventHandler(this.buttonColor_Click);
            // 
            // panelColorPicker
            // 
            this.panelColorPicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelColorPicker.Location = new System.Drawing.Point(180, 204);
            this.panelColorPicker.Name = "panelColorPicker";
            this.panelColorPicker.Size = new System.Drawing.Size(24, 21);
            this.panelColorPicker.TabIndex = 15;
            // 
            // SetColor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(589, 295);
            this.Controls.Add(this.panelColorPicker);
            this.Controls.Add(this.buttonColor);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelPickLayer);
            this.Controls.Add(this.listViewStyles);
            this.Name = "SetColor";
            this.Text = "SetColor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewStyles;
        private System.Windows.Forms.ColumnHeader columnHeaderStyleName;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelPickLayer;
        private System.Windows.Forms.Button buttonColor;
        private System.Windows.Forms.Panel panelColorPicker;
    }
}