using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SubtitleEdit
{
    internal class GlyphReader
    {
        private readonly HashSet<char> _glyphs;
        internal GlyphReader()
        {
            _glyphs = new HashSet<char>();
            var resourceName = "SubtitleEdit.NGL2-Final-External-Use-v2.txt";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return;
                }
                using (var reader = new StreamReader(stream))
                {
                    _glyphs.Add(' ');
                    _glyphs.Add('\r');
                    _glyphs.Add('\n');
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Trim().Length == 1 && !_glyphs.Contains(line.Trim()[0]))
                        {
                            _glyphs.Add(line.Trim()[0]);
                        }
                    }
                }
            }
        }

        internal bool ContainsIllegalGlyphs(string s, out string illegalGlyphs)
        {
            var sb = new StringBuilder();
            foreach (var ch in s)
            {
                if (!_glyphs.Contains(ch))
                    sb.Append(ch);
            }
            illegalGlyphs = sb.ToString();
            return sb.Length > 0;
        }

        public string CleanText(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (_glyphs.Contains(ch))
                    sb.Append(ch);
            }
            return sb.ToString();
        }

        //internal void ConvertFromUnicodeTextFile()
        //{
        //    _glyphs = new HashSet<char>();
        //    var resourceName = "SubtitleEdit.NGL2-Final-External-Use-v2.txt";
        //    var sb2 = new StringBuilder();
        //    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        //    {
        //        if (stream == null)
        //        {
        //            return;
        //        }
        //        using (var reader = new StreamReader(stream))
        //        {
        //            _glyphs.Add('"');
        //            _glyphs.Add(',');
        //            _glyphs.Add(' ');
        //            string line;
        //            while ((line = reader.ReadLine()) != null)
        //            {
        //                var arr = line.Split('\t');
        //                if (arr.Length > 1 && arr[1].Trim().Length == 1 && !_glyphs.Contains(arr[1].Trim()[0]))
        //                {
        //                    _glyphs.Add(arr[1].Trim()[0]);
        //                }
        //            }
        //        }
        //    }
        //    foreach (var glyph in _glyphs)
        //    {
        //        sb2.AppendLine(glyph.ToString());
        //    }
        //    System.IO.File.WriteAllText(@"e:\DATA\NGL2-Final-External-Use-v2.txt", sb2.ToString());
        //}

    }

}
