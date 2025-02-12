using System.Diagnostics;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.PluginLogic;

namespace Commas;

public partial class Main : Form
{
    private readonly Subtitle _subtitle;

    public Main(Subtitle subtitle)
    {
        _subtitle = subtitle;
        InitializeComponent();

        buttonOkay.DialogResult = DialogResult.OK;
    }

    private CancellationTokenSource _cancellationTokenSource;

    private async void buttonFixComma_Click(object sender, EventArgs e)
    {
        using var lmStudioClient = LmStudioClient.Create(textBoxEndPoint.Text, textBoxPrompt.Text);

        // listView1.BeginUpdate();
        _cancellationTokenSource = new CancellationTokenSource();

        IProgress<ListViewItem> progress = new Progress<ListViewItem>(listViewItem =>
        {
            listView1.Items.Add(listViewItem);
            listView1.Refresh();
        });
        await Task.Run(async () =>
        {
            foreach (var paragraph in _subtitle.Paragraphs)
            {
                var output = await lmStudioClient.SendAsync(paragraph.Text).ConfigureAwait(false);

                if (!output.Equals(paragraph.Text, StringComparison.Ordinal))
                {
                    progress.Report(new ListViewItem(paragraph.Text)
                    {
                        SubItems = { output, }
                    });
                }
            }
        }, _cancellationTokenSource.Token);
        // listView1.EndUpdate();
    }
}