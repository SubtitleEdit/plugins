using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic
{
    public static class Utils
    {
        #region String Extensions

        public static bool StartsWith(this string s, char c)
        {
            return s.Length > 0 && s[0] == c;
        }

        public static bool Contains(this string s, char c)
        {
            return s.Length > 0 && s.IndexOf(c) >= 0;
        }

        public static string[] SplitToLines(this string s)
        {
            return s.Replace(Environment.NewLine, "\n").Replace('\r', '\n').Split('\n');
        }

        #endregion

        private static int GetCount(string text, params string[] words)
        {
            int count = 0;
            for (int i = 0; i < words.Length; i++)
            {
                var regEx = new Regex("\\b" + words[i] + "\\b");
                count += regEx.Matches(text).Count;
            }
            return count;
        }

        private static int CountTagInText(string text, string tag)
        {
            var idx = text.IndexOf(tag, StringComparison.Ordinal);
            var count = 0;
            while (idx >= 0)
            {
                count++;
                idx = text.IndexOf(tag, idx + tag.Length, StringComparison.Ordinal);
            }
            return count;
        }

        public static string AutoDetectGoogleLanguage(string text, int bestCount)
        {
            return Nikse.SubtitleEdit.Core.LanguageAutoDetect.AutoDetectGoogleLanguage(text, bestCount);
        }

        private static int GetCountContains(string text, params string[] words)
        {
            if (words == null || words.Length == 0)
                return 0;
            var count = 0;
            for (int i = 0; i < words.Length; i++)
            {
                count += CountTagInText(text, words[i]);
            }
            return count;
        }

        public static string AutoDetectGoogleLanguage(string text)
        {
            string languageId = AutoDetectGoogleLanguage(text, 10);

            if (string.IsNullOrEmpty(languageId))
                return "en";

            return languageId;
        }

        public static string GetSettingsFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\", StringComparison.Ordinal))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "OpenSubtitles.xml");
        }

        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.Unicode.GetBytes(toEncode);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
            return Encoding.Unicode.GetString(encodedDataAsBytes);
        }
    }
}