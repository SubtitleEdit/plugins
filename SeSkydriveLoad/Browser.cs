using System;
using System.Windows.Forms;

namespace SeSkydriveLoad
{
    public partial class Browser : Form
    {
        public string Token { get; private set; }
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
            int startOfAccesstoken = url.IndexOf("access_token=");
            if (startOfAccesstoken > 0)
            {
                url = url.Substring(startOfAccesstoken + 13);
                int endOfAccessToken = url.IndexOf("&");
                if (endOfAccessToken > 0)
                    url = url.Substring(0, endOfAccessToken);
                Token = url;

                try
                {
                    url = webBrowser1.Url.AbsoluteUri;
                    startOfAccesstoken = url.IndexOf("expires_in=");
                    if (startOfAccesstoken > 0)
                    {
                        url = url.Substring(startOfAccesstoken + 11);
                        endOfAccessToken = url.IndexOf("&");
                        if (endOfAccessToken > 0)
                            url = url.Substring(0, endOfAccessToken);
                    }
                    Expires = DateTime.Now.AddSeconds(Convert.ToInt32(url));
                }
                catch
                {
                    Expires = DateTime.Now;
                }
                DialogResult = DialogResult.OK;
            }
        }

        private void Browser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode ==  Keys.Escape)
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}