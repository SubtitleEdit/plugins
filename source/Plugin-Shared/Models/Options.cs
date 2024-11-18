namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class Options
    {
        static Options()
        {
            Frame = 23.976;
            UILineBreak = "<br />";
        }

        public static double Frame { get; set; }
        public static string UILineBreak { get; set; }
    }
}
