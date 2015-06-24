using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class Utilities
    {
        #region Extension Methods
        public static string[] SplitToLines(this string source)
        {
            return source.Replace(Environment.NewLine, "\n").Replace('\r', '\n').Split('\n');
        }
        #endregion

        #region Static Methods
        public static int GetNumberOfLines(string s)
        {
            if (s.Length < 1)
                return 0;
            var ln = 0;
            var idx = s.IndexOf('\n');
            while (idx >= 0)
            {
                ln++;
                idx = s.IndexOf('\n', idx + 1);
            }
            return ln + 1;
        }

        public static bool IsInteger(string s)
        {
            int i;
            return int.TryParse(s, out i);
        }

        public static string RemoveHtmlTags(string s, bool alsoSSa = false)
        {
            if (s == null || s.Length < 3)
                return s;

            if (alsoSSa)
            {
                var idx = s.IndexOf('{');
                while (idx >= 0)
                {
                    var endIdx = s.IndexOf('}', idx + 1);
                    if (endIdx < idx)
                        break;
                    s = s.Remove(idx, endIdx - idx + 1);
                    idx = s.IndexOf('{', idx);
                }
            }
            s = System.Text.RegularExpressions.Regex.Replace(s, "</?[bipu]>", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            s = RemoveParagraphTag(s);
            return RemoveHtmlFontTag(s);
        }

        internal static string RemoveHtmlFontTag(string s)
        {
            s = s.Replace("</font>", string.Empty);
            s = s.Replace("</FONT>", string.Empty);
            s = s.Replace("</Font>", string.Empty);
            var idx = s.IndexOf("<font", StringComparison.OrdinalIgnoreCase);
            while (idx >= 0)
            {
                var endIdx = s.IndexOf('>', idx + 5);
                if (endIdx < idx)
                    break;
                s = s.Remove(idx, endIdx - idx + 1);
                idx = s.IndexOf("<font", idx, StringComparison.OrdinalIgnoreCase);
            }
            return s;
        }

        internal static string RemoveParagraphTag(string s)
        {
            s = s.Replace("</p>", string.Empty);
            s = s.Replace("</P>", string.Empty);
            var idx = s.IndexOf("<p", StringComparison.OrdinalIgnoreCase);
            while (idx >= 0)
            {
                var endIdx = s.IndexOf('>', idx + 2);
                if (endIdx < idx)
                    break;
                s = s.Remove(idx, endIdx - idx + 1);
                idx = s.IndexOf("<p", idx, StringComparison.OrdinalIgnoreCase);
            }
            return s;
        }

        public static string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public static IEnumerable<string> GetMovieFileExtensions()
        {
            return new List<string> { ".avi", ".mkv", ".wmv", ".mpg", ".mpeg", ".divx", ".mp4", ".asf", ".flv", ".mov", ".m4v", ".vob", ".ogv", ".webm", ".ts", ".m2ts" };
        }

        public static string GetVideoFileFilter(bool includeAudioFiles)
        {
            var sb = new StringBuilder();
            sb.Append("Video files|");
            int i = 0;
            foreach (string extension in GetMovieFileExtensions())
            {
                if (i > 0)
                    sb.Append(";");
                sb.Append("*" + extension);
                i++;
            }
            if (includeAudioFiles)
                sb.Append("|Audio files|*.mp3;*.wav;*.wma;*.ogg;*.mpa;*.ape;*.aiff;*.flac;*.aac");
            sb.Append("|All files|*.*");
            return sb.ToString();
        }
        #endregion

    }
}