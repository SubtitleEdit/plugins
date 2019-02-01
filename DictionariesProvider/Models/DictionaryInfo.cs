using System.Collections.Generic;

namespace Nikse.SubtitleEdit.PluginLogic.Models
{
    public class DictionaryInfo
    {
        public string EnglishName { get; set; }
        public string NativeName { get; set; }
        public string Description { get; set; }
        public List<DownloadLink> DownloadLinks { get; set; }
    }
}
