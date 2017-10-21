namespace Nikse.SubtitleEdit.PluginLogic
{
    public class ComboBoxItem
    {
        public ComboBoxItem(string displayName, string example, HIStyle style)
        {
            DisplayName = displayName;
            Example = example;
            Style = style;
        }

        public string DisplayName { get; }
        public string Example { get; }
        public HIStyle Style { get; }

        public override string ToString() => DisplayName;

    }
}
