using System.Diagnostics;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.PluginLogic;

namespace Commas;

public partial class Main : Form
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
                    var output = await lmStudioClient.SendAsync(paragraph.Text).ConfigureAwait(false);

                    if (!output.Equals(paragraph.Text, StringComparison.Ordinal))
                    {
                        progress.Report((new ListViewItem(new[] { paragraph.Text, output }), index));
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