using System;
using System.IO;
using System.Reflection;
using System.Text;

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

        public static string AutoDetectGoogleLanguage(string text, int bestCount)
        {
            return Core.LanguageAutoDetect.AutoDetectGoogleLanguage(text, bestCount);
        }

        public static string AutoDetectGoogleLanguage(string text)
        {
            var languageId = AutoDetectGoogleLanguage(text, 10);
            return string.IsNullOrEmpty(languageId) ? "en" : languageId;
        }

        public static string GetSettingsFileName()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\", StringComparison.Ordinal))
            {
                path = path.Remove(0, 6);
            }

            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
            {
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            }

            return Path.Combine(path, "OpenSubtitles.xml");
        }

        public static string EncodeTo64(string toEncode)
        {
            var toEncodeAsBytes = Encoding.Unicode.GetBytes(toEncode);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        public static string DecodeFrom64(string encodedData)
        {
            var encodedDataAsBytes = Convert.FromBase64String(encodedData);
            return Encoding.Unicode.GetString(encodedDataAsBytes);
        }
    }
}