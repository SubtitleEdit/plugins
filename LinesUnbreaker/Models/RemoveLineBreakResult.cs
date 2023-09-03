namespace Nikse.SubtitleEdit.PluginLogic.Models
{
    public class RemoveLineBreakResult
    {
        public Paragraph Paragraph { get; set; }
        public string BeforeText => Paragraph.Text;
        public string AfterText { get; set; }
    }
}