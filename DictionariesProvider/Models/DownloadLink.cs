using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.PluginLogic.Models
{
    public class DownloadLink
    {
        // On / down
        public string Status { get; set; }
        public Uri Url { get; set; }

        public override string ToString() => Url.ToString();
    }
}
