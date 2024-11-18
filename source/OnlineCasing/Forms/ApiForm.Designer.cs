namespace OnlineCasing.Forms
{
    partial class ApiForm
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
            this.textBoxApiKey = new System.Windows.Forms.TextBox();
            this.buttonSet = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.linkLabelSignUP = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // textBoxApiKey
            // 
            this.textBoxApiKey.Location = new System.Drawing.Point(23, 62);
            this.textBoxApiKey.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxApiKey.Name = "textBoxApiKey";
            this.textBoxApiKey.Size = new System.Drawing.Size(480, 22);
            this.textBoxApiKey.TabIndex = 0;
            this.textBoxApiKey.UseSystemPasswordChar = true;
            // 
            // buttonSet
            // 
            this.buttonSet.Location = new System.Drawing.Point(404, 111);
            this.buttonSet.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSet.Name = "buttonSet";
            this.buttonSet.Size = new System.Drawing.Size(100, 28);
            this.buttonSet.TabIndex = 1;
            this.buttonSet.Text = "Set";
            this.buttonSet.UseVisualStyleBackColor = true;
            this.buttonSet.Click += new System.EventHandler(this.ButtonSet_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 42);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Api key:";
            // 
            // linkLabelSignUP
            // 
            this.linkLabelSignUP.AutoSize = true;
            this.linkLabelSignUP.Location = new System.Drawing.Point(20, 123);
            this.linkLabelSignUP.Name = "linkLabelSignUP";
            this.linkLabelSignUP.Size = new System.Drawing.Size(233, 16);
            this.linkLabelSignUP.TabIndex = 5;
            this.linkLabelSignUP.TabStop = true;
            this.linkLabelSignUP.Tag = "https://www.themoviedb.org/account/signup";
            this.linkLabelSignUP.Text = "Don\'t have a API key yet? (Click here!)";
            this.linkLabelSignUP.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelSignUP_LinkClicked);
            // 
            // ApiForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 154);
            this.Controls.Add(this.linkLabelSignUP);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonSet);
            this.Controls.Add(this.textBoxApiKey);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ApiForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ApiForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxApiKey;
        private System.Windows.Forms.Button buttonSet;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabelSignUP;
    }
}