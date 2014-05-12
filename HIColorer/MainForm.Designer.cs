namespace Nikse.SubtitleEdit.PluginLogic
{
    partial class MainForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labelMoodsColor = new System.Windows.Forms.Label();
            this.buttonMoodsColor = new System.Windows.Forms.Button();
            this.buttonNarratorColor = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.labelNarratorsColor = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxMoods = new System.Windows.Forms.CheckBox();
            this.checkBoxNarrator = new System.Windows.Forms.CheckBox();
            this.checkBoxEnabledNarrator = new System.Windows.Forms.CheckBox();
            this.checkBoxEnabledMoods = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxEnabledMoods);
            this.groupBox1.Controls.Add(this.checkBoxEnabledNarrator);
            this.groupBox1.Controls.Add(this.labelNarratorsColor);
            this.groupBox1.Controls.Add(this.buttonNarratorColor);
            this.groupBox1.Controls.Add(this.buttonMoodsColor);
            this.groupBox1.Controls.Add(this.labelMoodsColor);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(404, 165);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Color options:";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(342, 216);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(261, 216);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Location = new System.Drawing.Point(6, 19);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(75, 23);
            this.buttonRemove.TabIndex = 3;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Narrator";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Moods";
            // 
            // labelMoodsColor
            // 
            this.labelMoodsColor.BackColor = System.Drawing.SystemColors.ControlDark;
            this.labelMoodsColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelMoodsColor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.labelMoodsColor.Location = new System.Drawing.Point(138, 111);
            this.labelMoodsColor.Name = "labelMoodsColor";
            this.labelMoodsColor.Size = new System.Drawing.Size(234, 23);
            this.labelMoodsColor.TabIndex = 2;
            this.labelMoodsColor.Text = "label3";
            this.labelMoodsColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonMoodsColor
            // 
            this.buttonMoodsColor.Location = new System.Drawing.Point(29, 111);
            this.buttonMoodsColor.Name = "buttonMoodsColor";
            this.buttonMoodsColor.Size = new System.Drawing.Size(75, 23);
            this.buttonMoodsColor.TabIndex = 4;
            this.buttonMoodsColor.Text = "Choose";
            this.buttonMoodsColor.UseVisualStyleBackColor = true;
            this.buttonMoodsColor.Click += new System.EventHandler(this.buttonMoodsColor_Click);
            // 
            // buttonNarratorColor
            // 
            this.buttonNarratorColor.Location = new System.Drawing.Point(29, 47);
            this.buttonNarratorColor.Name = "buttonNarratorColor";
            this.buttonNarratorColor.Size = new System.Drawing.Size(75, 23);
            this.buttonNarratorColor.TabIndex = 5;
            this.buttonNarratorColor.Text = "Choose";
            this.buttonNarratorColor.UseVisualStyleBackColor = true;
            this.buttonNarratorColor.Click += new System.EventHandler(this.buttonNarratorColor_Click);
            // 
            // labelNarratorsColor
            // 
            this.labelNarratorsColor.BackColor = System.Drawing.SystemColors.ControlDark;
            this.labelNarratorsColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelNarratorsColor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.labelNarratorsColor.Location = new System.Drawing.Point(138, 47);
            this.labelNarratorsColor.Name = "labelNarratorsColor";
            this.labelNarratorsColor.Size = new System.Drawing.Size(234, 23);
            this.labelNarratorsColor.TabIndex = 6;
            this.labelNarratorsColor.Text = "label4";
            this.labelNarratorsColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxMoods);
            this.groupBox2.Controls.Add(this.buttonRemove);
            this.groupBox2.Controls.Add(this.checkBoxNarrator);
            this.groupBox2.Location = new System.Drawing.Point(13, 184);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(242, 55);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Strip Color";
            // 
            // checkBoxMoods
            // 
            this.checkBoxMoods.AutoSize = true;
            this.checkBoxMoods.Location = new System.Drawing.Point(96, 19);
            this.checkBoxMoods.Name = "checkBoxMoods";
            this.checkBoxMoods.Size = new System.Drawing.Size(58, 17);
            this.checkBoxMoods.TabIndex = 7;
            this.checkBoxMoods.Text = "Moods";
            this.checkBoxMoods.UseVisualStyleBackColor = true;
            // 
            // checkBoxNarrator
            // 
            this.checkBoxNarrator.AutoSize = true;
            this.checkBoxNarrator.Location = new System.Drawing.Point(160, 19);
            this.checkBoxNarrator.Name = "checkBoxNarrator";
            this.checkBoxNarrator.Size = new System.Drawing.Size(64, 17);
            this.checkBoxNarrator.TabIndex = 6;
            this.checkBoxNarrator.Text = "Narrator";
            this.checkBoxNarrator.UseVisualStyleBackColor = true;
            // 
            // checkBoxEnabledNarrator
            // 
            this.checkBoxEnabledNarrator.AutoSize = true;
            this.checkBoxEnabledNarrator.Location = new System.Drawing.Point(307, 29);
            this.checkBoxEnabledNarrator.Name = "checkBoxEnabledNarrator";
            this.checkBoxEnabledNarrator.Size = new System.Drawing.Size(65, 17);
            this.checkBoxEnabledNarrator.TabIndex = 7;
            this.checkBoxEnabledNarrator.Text = "Enabled";
            this.checkBoxEnabledNarrator.UseVisualStyleBackColor = true;
            // 
            // checkBoxEnabledMoods
            // 
            this.checkBoxEnabledMoods.AutoSize = true;
            this.checkBoxEnabledMoods.Location = new System.Drawing.Point(307, 91);
            this.checkBoxEnabledMoods.Name = "checkBoxEnabledMoods";
            this.checkBoxEnabledMoods.Size = new System.Drawing.Size(65, 17);
            this.checkBoxEnabledMoods.TabIndex = 8;
            this.checkBoxEnabledMoods.Text = "Enabled";
            this.checkBoxEnabledMoods.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(429, 250);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MainForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonNarratorColor;
        private System.Windows.Forms.Button buttonMoodsColor;
        private System.Windows.Forms.Label labelMoodsColor;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Label labelNarratorsColor;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBoxMoods;
        private System.Windows.Forms.CheckBox checkBoxNarrator;
        private System.Windows.Forms.CheckBox checkBoxEnabledMoods;
        private System.Windows.Forms.CheckBox checkBoxEnabledNarrator;
    }
}