using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.PluginLogic.Helpers
{
    public class UIUtils
    {
        public static string GetListViewTextFromString(string s) => s.Replace(Environment.NewLine, Options.UILineBreak);

        public static string GetStringFromListViewText(string lviText) => lviText.Replace(Options.UILineBreak, Environment.NewLine);
    }
}
