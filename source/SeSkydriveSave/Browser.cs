using System;
using System.Windows.Forms;

namespace SeSkydriveSave
{
    public partial class Browser : Form
    {
        public string Code { get; private set; }
        public DateTime Expires { get; private set; }

        public Browser(string url)
        {
            InitializeComponent();
            webBrowser1.Url = new Uri(url);
            webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            string url = webBrowser1.Url.AbsoluteUri;
            int startOfAccesstoken = url.IndexOf("code=");
            if (startOfAccesstoken > 0)
            {
                url = url.Substring(startOfAccesstoken + 5);
                int endOfAccessToken = url.IndexOf("&");
                if (endOfAccessToken > 0)
                    url = url.Substring(0, endOfAccessToken);
                Code = url;
                DialogResult = DialogResult.OK;
            }
        }
    }
}
