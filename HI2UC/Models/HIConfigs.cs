namespace Nikse.SubtitleEdit.PluginLogic
{
    public class HIConfigs : Configuration<HIConfigs>
    {
        public HIConfigs(string configFile) : base(configFile)
        {
            Style = HIStyle.UpperCase;
            MoodsToUppercase = true;
            NarratorToUppercase = true;
            SingleLineNarrator = true;
        }

        public HIStyle Style { get; set; }
        public bool MoodsToUppercase { get; set; }
        public bool NarratorToUppercase { get; set; }
        public bool SingleLineNarrator { get; set; }
        public bool RemoveExtraSpaces { get; set; }

    }
}
