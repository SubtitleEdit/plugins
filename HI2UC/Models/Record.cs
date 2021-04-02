namespace Nikse.SubtitleEdit.PluginLogic.Strategies
{
    public class Record
    {
        public string Comment { get; set; }
        public string Before { get; set; }
        public string After { get; set; }
        public Paragraph Paragraph { get; set; }
        public override string ToString() => $"#{Paragraph.Number}: {Before} => {After}";

        public void Apply() => Paragraph.Text = After;
    }
}