namespace Nikse.SubtitleEdit.PluginLogic
{
    public class UnBreakConfigs : Configuration<UnBreakConfigs>
    {
        public bool SkipNarrator { get; set; }

        public bool SkipMoods { get; set; }

        public bool SkipDialogs { get; set; }

        public int MaxLineLength { get; set; }

        public UnBreakConfigs(string configFile) : base(configFile)
        {
            SkipDialogs = true;
            SkipNarrator = true;
            SkipMoods = false;
        }

    }
}
