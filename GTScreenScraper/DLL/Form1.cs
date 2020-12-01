using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WebViewTranslate.Logic;

namespace WebViewTranslate
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
            var paragraphs = new List<Paragraph>
            {
                new Paragraph("Olá.Eu sou Freddie Highmore.", 0, 0),
                new Paragraph("O episódio a seguir retrata\r\nnossa esperança para o futuro,", 0, 0),
                new Paragraph("um futuro onde ninguém\r\nterá que usar máscaras", 0, 0),
                new Paragraph(" ou tome outras medidas\r\npara se proteger do COVID.", 0, 0),
                new Paragraph("Eu sou Jordan Allen.", 0, 0),
            };


            var englishParagraphs = new List<Paragraph>
            {
                new Paragraph("How are you?\r\nNow now!", 0, 0),
                new Paragraph("I'm Fine?", 0, 0),
                new Paragraph("How nice of you to join us today" + Environment.NewLine +
                              "so we all can work on the project together", 0, 0),
                new Paragraph("Tuesday", 0, 0),
                new Paragraph("Wedensday", 0, 0),
                new Paragraph("Thursday", 0, 0),
                new Paragraph("Friday", 0, 0),
                new Paragraph("Godbye.", 0, 0),
            };

            var sub = new Subtitle();
            sub.Paragraphs.AddRange(paragraphs);

            using (var form = new MainForm(sub, "test", "descr", this))
            {
                form.ShowDialog(this);
            }
        }
    }
}
