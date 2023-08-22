using System.Globalization;

namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies
{
    public static class ExtensionMethods
    {
        public static string CapitalizeFirstLetter(this string s, CultureInfo ci = null)
        {
            var si = new StringInfo(s);
            if (ci == null)
            {
                ci = CultureInfo.CurrentCulture;
                // todo: use instead?
                // ci.TextInfo.ToTitleCase()
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

    public class TitleCaseConverterStrategy : IConverterStrategy
    {
        public string Name => "Title case (sentence)";

        public string Execute(string input) => input.CapitalizeFirstLetter();

        public override string ToString() => Name;
    }
}