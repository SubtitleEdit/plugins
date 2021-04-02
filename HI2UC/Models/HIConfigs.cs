namespace Nikse.SubtitleEdit.PluginLogic
{
    public class HiConfigs : Configuration<HiConfigs>
    {
        public HiConfigs(string configFile) : base(configFile)
        {
            Style = HiStyle.UpperCase;
            MoodsToUppercase = true;
            NarratorToUppercase = true;
            SingleLineNarrator = true;
        }

        public HiStyle Style { get; set; }
        public bool MoodsToUppercase { get; set; }
        public bool NarratorToUppercase { get; set; }
        public bool SingleLineNarrator { get; set; }
        public bool RemoveExtraSpaces { get; set; }

    }
}
