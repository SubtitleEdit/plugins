namespace Nikse.SubtitleEdit.PluginLogic.Models
{
    public class Configs
    {
        public Configs(double frame = 23.976, string uILineBreak = "<br />")
        {
            Frame = frame;
            UILineBreak = uILineBreak;
        }

        public double Frame { get; set; }
        public string UILineBreak { get; set; }
    }
}
