using System;
using System.Drawing;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public abstract class Artist
    {
        private const string WriteFormat = "<font color=\"{0}\">{1}</font>";

        public abstract void Paint(Subtitle subtitle);

        protected bool ContainsColor(string text) => text.Contains("<font", StringComparison.OrdinalIgnoreCase);

        protected static string ApplyColor(Color color, string text) => string.Format(WriteFormat, Utilities.ColorToHex(color), text.Trim());

    }
}
