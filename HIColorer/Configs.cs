using System.Drawing;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class Configs : Configuration<Configs>
    {
        /// <summary>
        /// Narrators color.
        /// </summary>
        public int Narrator { get; set; }

        /// <summary>
        /// Moods color.
        /// </summary>
        public int Moods { get; set; }

        public Configs()
        {
            Narrator = Color.Blue.ToArgb();
            Moods = Color.Maroon.ToArgb();
        }
    }
}
