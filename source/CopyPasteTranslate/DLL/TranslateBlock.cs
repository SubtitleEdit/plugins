using System;
using System.Windows.Forms;
using WebViewTranslate.Translator;

namespace CopyPasteTranslate
{
    public partial class TranslateBlock : Form
    {
        private CopyBlock _sourceBlock;
        public string TargetText { get; set; }

        public TranslateBlock(CopyBlock source, string title, bool autoCopy)
        {
            InitializeComponent();
            _sourceBlock = source;
            Text = title;
            if (autoCopy)
            {
                buttonCopySourceTextToClipboard_Click(null, null);
                buttonCopySourceTextToClipboard.Text = "Copy source text clipboard again";
                buttonCopySourceTextToClipboard.Font = new System.Drawing.Font(Font.FontFamily.Name, buttonCopySourceTextToClipboard.Font.Size, System.Drawing.FontStyle.Regular);

            }
            else
            {
                buttonCopySourceTextToClipboard.Text = "Copy source text clipboard";
                buttonCopySourceTextToClipboard.Font = new System.Drawing.Font(Font.FontFamily.Name, buttonCopySourceTextToClipboard.Font.Size, System.Drawing.FontStyle.Bold);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonGetTargetGet_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                var x = Clipboard.GetData(DataFormats.UnicodeText);
                var text = x.ToString();
                if (text.Trim() == _sourceBlock.TargetText.Trim())
                {
                    MessageBox.Show("Clipboard contains source text!" + Environment.NewLine + 
                        Environment.NewLine +
                        "Go to translator and translate, the copy result to clipboard and click this button again.");
                    return;
                }
                TargetText = text;
            }
            DialogResult = DialogResult.OK;
        }

        private void buttonCopySourceTextToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(_sourceBlock.TargetText);
            buttonCopySourceTextToClipboard.Font = new System.Drawing.Font(Font.FontFamily.Name, buttonCopySourceTextToClipboard.Font.Size, System.Drawing.FontStyle.Regular);
        }

        private void TranslateBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                buttonGetTargetGet_Click(sender, e);
            }
        }
    }
}
