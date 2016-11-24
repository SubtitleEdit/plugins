namespace PluginCoreLib.Utils
{
    using System;

    public static class HtmlUtils
    {
        private const string SsaStart = "{\\";

        public static string RemoveSsa(string input)
        {
            int idx = input.IndexOf(SsaStart, StringComparison.Ordinal);
            while (idx >= 0)
            {
                var endIdx = input.IndexOf('}', idx + 2);
                if (endIdx < idx)
                {
                    break;
                }
                input = input.Remove(idx, endIdx - idx + 1);
                idx = input.IndexOf(SsaStart, idx);
            }
            return input;
        }

        public static string RemoveHtmlTags(string input, bool alsoSsa)
        {
            if (input == null || input.Length < 3)
            {
                return input;
            }
            if (alsoSsa)
            {
                input = RemoveSsa(input);
            }
            var idx = input.IndexOf('<');
            while (idx >= 0)
            {
                var endIdx = input.IndexOf('>', idx + 1);
                if (endIdx < idx) break;
                input = input.Remove(idx, endIdx - idx + 1);
                idx = input.IndexOf('<', idx);
            }
            return input;
        }
    }
}
