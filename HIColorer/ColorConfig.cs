using System.Drawing;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class ColorConfig : Configuration<ColorConfig>
    {
        /// <summary>
        /// Narrators color.
        /// </summary>
        public int Narrator { get; set; }

        /// <summary>
        /// Moods color.
        /// </summary>
        public int Moods { get; set; }

        public ColorConfig(string configFile) : base(configFile)
        {
            Narrator = Color.Blue.ToArgb();
            Moods = Color.Maroon.ToArgb();
        }
    }
}
