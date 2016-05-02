namespace Nikse.SubtitleEdit.PluginLogic
{
    public class Configuration
    {
        public static double CurrentFrameRate = 23.976;
        public static string ListViewLineSeparatorString = "<br />";

        public HIStyle Style { get; set; }
        public bool MoodsToUppercase { get; set; }
        public bool NarratorToUppercase { get; set; }
    }
}
