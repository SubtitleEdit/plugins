using System.Globalization;

namespace Nikse.SubtitleEdit.PluginLogic.Strategies
{
    public static class ExtensionMethods
    {
        public static string CapitalizeFirstLetter(this string s, CultureInfo ci = null)
        {
            var si = new StringInfo(s);
            if (ci == null)
            {
                ci = CultureInfo.CurrentCulture;
            }

            if (si.LengthInTextElements > 0)
            {
                s = si.SubstringByTextElements(0, 1).ToUpper(ci);
            }

            if (si.LengthInTextElements > 1)
            {
                s += si.SubstringByTextElements(1);
            }

            return s;
        }
    }

    public class TitlecaseStrategy : IStrategy
    {
        public string Name => "Title case";

        public string Execute(string input) => input.CapitalizeFirstLetter();

        public override string ToString() => Name;
    }
}