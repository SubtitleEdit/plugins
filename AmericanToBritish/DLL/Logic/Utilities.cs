using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic.Logic
{
    internal static class Utilities
    {
        public static readonly char[] NewLineChars = Environment.NewLine.ToCharArray();

        #region String Extensions

        public static bool StartsWith(this string s, char c)
        {
            return s.Length > 0 && s[0] == c;
        }

        public static bool StartsWith(this StringBuilder sb, char c)
        {
            return sb.Length > 0 && sb[0] == c;
        }

        public static bool EndsWith(this string s, char c)
        {
            return s.Length > 0 && s[s.Length - 1] == c;
        }

        public static bool EndsWith(this StringBuilder sb, char c)
        {
            return sb.Length > 0 && sb[sb.Length - 1] == c;
        }

        public static bool Contains(this string source, char value)
        {
            return source.IndexOf(value) >= 0;
        }

        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source.IndexOf(value, comparisonType) >= 0;
        }

        public static string[] SplitToLines(this string s)
        {
            return s.Replace(Environment.NewLine, "\n").Replace('\r', '\n').Split('\n');
        }

        #endregion

        public static string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public static bool IsInteger(string s)
        {
            int i;
            return int.TryParse(s, out i);
        }

        public static string RemoveHtmlTags(string s, bool alsoSsa)
        {
            if (string.IsNullOrEmpty(s))
                return null;

            int idx;
            if (alsoSsa)
            {
                const string SSATAg = "{\\";
                idx = s.IndexOf(SSATAg, StringComparison.Ordinal);
                while (idx >= 0)
                {
                    var endIdx = s.IndexOf('}', idx + 2);
                    if (endIdx < idx)
                        break;
                    s = s.Remove(idx, endIdx - idx + 1);
                    idx = s.IndexOf(SSATAg, idx, StringComparison.Ordinal);
                }
            }
            if (!s.Contains("<"))
                return s;
            s = Regex.Replace(s, "</?[ibu]>", string.Empty, RegexOptions.IgnoreCase);

            s = Regex.Replace(s, "</?font>", string.Empty, RegexOptions.IgnoreCase);
            idx = s.IndexOf("<font", StringComparison.OrdinalIgnoreCase);
            while (idx >= 0)
            {
                var endIdx = s.IndexOf('>', idx + 5);
                if (endIdx < idx)
                    break;
                s = s.Remove(idx, endIdx - idx + 1);
                idx = s.IndexOf("<font", idx, StringComparison.OrdinalIgnoreCase);
            }
            return s.Trim();
        }

        public static int GetNumberOfLines(string text)
        {
            if (text == null)
                return 0;
            var lines = 1;
            var idx = text.IndexOf('\n');
            while (idx >= 0)
            {
                lines++;
                idx = text.IndexOf('\n', idx + 1);
            }
            return lines;
        }

        public static string GetWordListFileName()
        {
            /* If the assembly is loaded from a byte array, such as when using the Load(Byte[])
             method overload, the value returned is an empty string ("").*/
            //var path = Assembly.GetExecutingAssembly().Location; // this will Throw

            // If the assembly was loaded as a byte array, using an overload of the Load
            // method that takes an array of bytes, this property returns the location of
            // the caller of the method, not the location of the loaded assembly.
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase); // Note: Subtitle edit loads assembly from raw bytes
            if (path.StartsWith("file:\\", StringComparison.Ordinal))
                path = path.Remove(0, 6);

            path = Path.Combine(path, "Plugins"); // try to find portable SE Plugins folder
            if (!Directory.Exists(path))
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit", "Plugins");
            }
            return Path.Combine(path, "AmericanToBritish.xml");
        }

    }
}
