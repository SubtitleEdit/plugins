using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic.Services;

namespace Nikse.SubtitleEdit.PluginLogic;

public partial class ReportForm : Form
{
    private readonly ReportService _reportService;

    public string ReportMessage { get; set; }

    public ReportForm(ReportService reportService)
    {
        InitializeComponent();
        StartPosition = FormStartPosition.CenterParent;

        _reportService = reportService;
        buttonSubmit.DialogResult = DialogResult.OK;

        Closing += (sender, args) => ReportMessage = textBox1.Text;
    }
}