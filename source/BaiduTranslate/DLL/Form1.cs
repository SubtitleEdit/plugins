using System;
using System.Windows.Forms;
using BaiduTranslate.Logic;

namespace BaiduTranslate
{
    /// <summary>
    /// https://blogs.windows.com/msedgedev/2018/05/09/modern-webview-winforms-wpf-apps/
    /// </summary>
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var form = new MainForm(new Subtitle { Paragraphs =
            {
                new Paragraph("<i>Previously on</i>\r\nRiverdale...",0,0),
                new Paragraph("I'm Fine?",0,0),
                new Paragraph("How nice of you to join us today"  + Environment.NewLine + 
                              "so we all can work on the project together",0,0),
                new Paragraph("Tuesday",0,0),
                new Paragraph("Wedensday",0,0),
                new Paragraph("Thursday",0,0),
                new Paragraph("Friday",0,0),
                new Paragraph("Godbye.",0,0),
            }
            }, "test", "descr", this))
            {
                form.ShowDialog(this);
            }
        }
    }
}
