using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.PluginLogic.Models
{
    public class DictionaryInfo
    {
        public string EnglishName { get; set; }
        public string NativeName { get; set; }
        public string Description { get; set; }
        public IList<DownloadLink> DownloadLinks { get; set; }
    }
}
