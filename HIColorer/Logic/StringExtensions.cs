using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.PluginLogic.Logic
{
    public static class StringExtensions
    {
        public static string[] SplitToLines(this string source)
        {
            return source.Replace("\r\n", "\n").Replace('\r', '\n').Replace('\u2028', '\n').Split('\n');
        }
    }
}
