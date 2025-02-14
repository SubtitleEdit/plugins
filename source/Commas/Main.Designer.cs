using System.ComponentModel;
using Nikse.SubtitleEdit.PluginLogic.Controls;

namespace Commas;

partial class Main
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
        this.listView1 = new DoubleBufferedListView();
        this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
        this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
        this.buttonOkay = new System.Windows.Forms.Button();
        this.labelEndPoint = new System.Windows.Forms.Label();
        this.textBoxEndPoint = new System.Windows.Forms.TextBox();
        this.buttonFixComma = new System.Windows.Forms.Button();
        this.progressBar1 = new System.Windows.Forms.ProgressBar();
        this.textBoxPrompt = new System.Windows.Forms.TextBox();
        this.SuspendLayout();
        // 
        // listView1
        // 
        this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
        this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { this.columnHeader1, this.columnHeader2 });
        this.listView1.FullRowSelect = true;
        this.listView1.GridLines = true;
        this.listView1.HideSelection = false;
        this.listView1.Location = new System.Drawing.Point(12, 63);
        this.listView1.Name = "listView1";
        this.listView1.Size = new System.Drawing.Size(776, 298);
        this.listView1.TabIndex = 0;
        this.listView1.UseCompatibleStateImageBehavior = false;
        this.listView1.View = System.Windows.Forms.View.Details;
        // 
        // columnHeader1
        // 
        this.columnHeader1.Text = "Before";
        this.columnHeader1.Width = 377;
        // 
        // columnHeader2
        // 
        this.columnHeader2.Text = "After";
        this.columnHeader2.Width = 376;
        // 
        // buttonOkay
        // 
        this.buttonOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.buttonOkay.Location = new System.Drawing.Point(713, 415);
        this.buttonOkay.Name = "buttonOkay";
        this.buttonOkay.Size = new System.Drawing.Size(75, 23);
        this.buttonOkay.TabIndex = 1;
        this.buttonOkay.Text = "OK";
        this.buttonOkay.UseVisualStyleBackColor = true;
        // 
        // label1
        // 
        this.labelEndPoint.AutoSize = true;
        this.labelEndPoint.Location = new System.Drawing.Point(9, 21);
        this.labelEndPoint.Name = "label1";
        this.labelEndPoint.Size = new System.Drawing.Size(52, 13);
        this.labelEndPoint.TabIndex = 2;
        this.labelEndPoint.Text = "Endpoint:";
        // 
        // textBoxEndPoint
        // 
        this.textBoxEndPoint.Location = new System.Drawing.Point(12, 37);
        this.textBoxEndPoint.Name = "textBoxEndPoint";
        this.textBoxEndPoint.Size = new System.Drawing.Size(320, 20);
        this.textBoxEndPoint.TabIndex = 3;
        this.textBoxEndPoint.Text = "http://127.0.0.1:1234";
        // 
        // buttonFixComma
        // 
        this.buttonFixComma.Location = new System.Drawing.Point(595, 415);
        this.buttonFixComma.Name = "buttonFixComma";
        this.buttonFixComma.Size = new System.Drawing.Size(112, 23);
        this.buttonFixComma.TabIndex = 4;
        this.buttonFixComma.Text = "Fix commas ✨";
        this.buttonFixComma.UseVisualStyleBackColor = true;
        this.buttonFixComma.Click += new System.EventHandler(this.buttonFixComma_Click);
        // 
        // progressBar1
        // 
        this.progressBar1.Location = new System.Drawing.Point(615, 34);
        this.progressBar1.Name = "progressBar1";
        this.progressBar1.Size = new System.Drawing.Size(173, 23);
        this.progressBar1.TabIndex = 5;
        // 
        // textBoxPrompt
        // 
        this.textBoxPrompt.Location = new System.Drawing.Point(12, 384);
        this.textBoxPrompt.Multiline = true;
        this.textBoxPrompt.Name = "textBoxPrompt";
        this.textBoxPrompt.Size = new System.Drawing.Size(198, 54);
        this.textBoxPrompt.TabIndex = 6;
        this.textBoxPrompt.Text = "Fix commas.\r\nDo not explain.\r\nDo not censor.";
        // 
        // Main
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Controls.Add(this.textBoxPrompt);
        this.Controls.Add(this.progressBar1);
        this.Controls.Add(this.buttonFixComma);
        this.Controls.Add(this.textBoxEndPoint);
        this.Controls.Add(this.labelEndPoint);
        this.Controls.Add(this.buttonOkay);
        this.Controls.Add(this.listView1);
        this.Name = "Main";
        this.Text = "Main";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.TextBox textBoxPrompt;

    #endregion

    private DoubleBufferedListView listView1;
    private Button buttonOkay;
    private Label labelEndPoint;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private System.Windows.Forms.TextBox textBoxEndPoint;
    private System.Windows.Forms.Button buttonFixComma;
    private System.Windows.Forms.ProgressBar progressBar1;
    private TextBox textBox2;
    private Label label2;
}