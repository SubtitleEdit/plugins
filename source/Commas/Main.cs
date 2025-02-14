using System.Diagnostics;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.PluginLogic;
using Nikse.SubtitleEdit.PluginLogic.Helpers;

namespace Commas;

internal partial class Main : Form
{
    private readonly Subtitle _subtitle;
    private volatile bool _isProcessing;

    public Main(Subtitle subtitle)
    {
        _subtitle = subtitle;
        InitializeComponent();

        buttonOkay.DialogResult = DialogResult.OK;

        progressBar1.Visible = false;

        Closing += (sender, args) => { _ = CancelAndDisposeResources(); };
        buttonOkay.Click += ButtonOkayOnClick;

        ConfigurePrompt();
    }

    private void ConfigurePrompt()
    {
        textBoxPrompt.Text = "Fix commas only, " +
                             "do not remove or add any words, " +
                             "do not change the meaning of the sentence, " +
                             "do do add or remove any other puntuation, " +
                             "do not censor, " +
                             "give only the output without comments or notes:";
    }

    private void ButtonOkayOnClick(object sender, EventArgs e)
    {
        if (_isProcessing)
        {
            return;
        }

        const int afterIndex = 1;
        foreach (ListViewItem listView1Item in listView1.Items)
        {
            var paragraph = (Paragraph)listView1Item.Tag;
            paragraph.Text = listView1Item.SubItems[afterIndex].Text.ToDomainText();
        }
    }

    private bool CancelAndDisposeResources()
    {
        if (_isProcessing)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _isProcessing = false;
            return true;
        }

        return false;
    }

    private CancellationTokenSource _cancellationTokenSource;

    private async void buttonFixComma_Click(object sender, EventArgs e)
    {
        if (CancelAndDisposeResources())
        {
            progressBar1.Visible = false;
            return;
        }

        progressBar1.Visible = true;

        using var lmStudioClient = new LmStudioClient(textBoxEndPoint.Text, textBoxPrompt.Text);

        // listView1.BeginUpdate();
        _cancellationTokenSource = new CancellationTokenSource();

        var previousPercentage = 0;
        IProgress<(ListViewItem, int)> progress = new Progress<(ListViewItem listViewItem, int index)>(item =>
        {
            listView1.Items.Add(item.listViewItem);

            var percentage = (item.index * 100 / _subtitle.Paragraphs.Count);
            // refresh every 5%
            if (percentage != previousPercentage && percentage % 5 == 0)
            {
                listView1.Refresh();
                previousPercentage = percentage;
                progressBar1.Value = percentage;
            }
        });

        try
        {
            await Task.Run(async () =>
            {
                _isProcessing = true;
                for (var index = 0; index < _subtitle.Paragraphs.Count; index++)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }

                    var paragraph = _subtitle.Paragraphs[index];

                    var lines = paragraph.Text.SplitToLines();

                    // try sending each line individually to avoid messing up with the line that
                    // doesn't require fixing which tends to happen when using less capable models
                    for (var i = 0; i < lines.Count; i++)
                    {
                        string line = lines[i];

                        // remove formatting before sending as some models also remove formattings unintentionally
                        var st = new StrippableText(line);
                        st.StrippedText = await lmStudioClient.SendAsync(st.StrippedText).ConfigureAwait(false);
                        lines[i] = st.MergedString;
                    }

                    string result = string.Join(Environment.NewLine, lines);

                    if (!result.Equals(paragraph.Text, StringComparison.Ordinal))
                    {
                        progress.Report((new ListViewItem(new[] { paragraph.Text, result })
                        {
                            Tag = paragraph,
                        }, index));
                    }
                }
            }, _cancellationTokenSource.Token).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
        // listView1.EndUpdate();
    }
}