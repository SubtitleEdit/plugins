using System.ComponentModel;

namespace Nikse.SubtitleEdit.PluginLogic;

partial class ReportForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

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
        this.buttonSubmit = new System.Windows.Forms.Button();
        this.textBox1 = new System.Windows.Forms.TextBox();
        this.SuspendLayout();
        // 
        // buttonSubmit
        // 
        this.buttonSubmit.Location = new System.Drawing.Point(324, 173);
        this.buttonSubmit.Name = "buttonSubmit";
        this.buttonSubmit.Size = new System.Drawing.Size(75, 23);
        this.buttonSubmit.TabIndex = 0;
        this.buttonSubmit.Text = "Submit";
        this.buttonSubmit.UseVisualStyleBackColor = true;
        // 
        // textBox1
        // 
        this.textBox1.Location = new System.Drawing.Point(12, 12);
        this.textBox1.Multiline = true;
        this.textBox1.Name = "textBox1";
        this.textBox1.Size = new System.Drawing.Size(387, 150);
        this.textBox1.TabIndex = 1;
        // 
        // ReportForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(411, 208);
        this.Controls.Add(this.textBox1);
        this.Controls.Add(this.buttonSubmit);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        this.Name = "ReportForm";
        this.Text = "Submit for analysis";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.Button buttonSubmit;
    private System.Windows.Forms.TextBox textBox1;

    #endregion
}