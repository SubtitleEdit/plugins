using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.PluginLogic.Logic
{
    internal static class StringExtensions
    {

        public static string[] SplitToLines(this string source)
        {
            return source.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }
    }
}
