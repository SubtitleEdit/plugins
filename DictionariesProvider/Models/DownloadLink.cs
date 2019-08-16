using System;

namespace Nikse.SubtitleEdit.PluginLogic.Models
{
    public class DownloadLink
    {
        // On / down
        public bool Status { get; set; }
        public Uri Url { get; set; }

        public override string ToString() => Url.ToString();
    }
}
