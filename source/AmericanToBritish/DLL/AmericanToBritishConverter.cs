using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class AmericanToBritishConverter
    {
        private IList<Regex> _regexList = new List<Regex>();
        private IList<string> _replaceList = new List<string>();
        private bool _extendsBuiltInWordList;

        public bool ExtendsBuiltInWordList
        {
            get
            {
                return _extendsBuiltInWordList;
            }
        }

        public string Convert(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            for (int index = 0; index < _regexList.Count; index++)
            {
                var regex = _regexList[index];
                if (regex.IsMatch(text))
                {
                    text = regex.Replace(text, _replaceList[index]);
                }
            }
            return RevertFontColorFix(text);
        }

        public bool LoadBuiltInWords()
        {
            _regexList.Clear();
            _replaceList.Clear();

            return AddBuiltInWords();
        }

        public bool AddBuiltInWords()
        {
            bool success = false;
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Nikse.SubtitleEdit.PluginLogic.WordList.xml"))
            {
                success = AddWordsToLists(stream);
            }
            return success;
        }

        public bool LoadLocalWords(string path)
        {
            bool success = false;
            if (File.Exists(path))
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var oldRegexList = _regexList;
                    var oldReplaceList = _replaceList;
                    _regexList = new List<Regex>();
                    _replaceList = new List<string>();
                    success = AddWordsToLists(stream);
                    if (!success)
                    {
                        _regexList = oldRegexList;
                        _replaceList = oldReplaceList;
                    }
                }
            }
            return success;
        }

        private bool AddWordsToLists(Stream stream)
        {
            XDocument xDoc;
            try
            {
                stream.Position = 0;
                xDoc = XDocument.Load(stream);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                return false;
            }
            if (xDoc?.Root.Name == "Words")
            {
                foreach (XElement xe in xDoc.Root.Elements("Word"))
                {
                    if (xe.Attribute("us")?.Value.Length > 1 && xe.Attribute("br")?.Value.Length > 1)
                    {
                        string american = xe.Attribute("us").Value;
                        string british = xe.Attribute("br").Value;

                        _regexList.Add(new Regex("\\b" + american + "\\b", RegexOptions.ExplicitCapture));
                        _replaceList.Add(british);

                        _regexList.Add(new Regex("\\b" + american.ToUpperInvariant() + "\\b", RegexOptions.ExplicitCapture));
                        _replaceList.Add(british.ToUpperInvariant());

                        _regexList.Add(new Regex("\\b" + char.ToUpperInvariant(american[0]) + american.Substring(1) + "\\b", RegexOptions.ExplicitCapture));
                        _replaceList.Add(char.ToUpperInvariant(british[0]) + british.Substring(1));
                    }
                }
                bool.TryParse(xDoc.Root.Attribute("ExtendsBuiltInWordList")?.Value, out _extendsBuiltInWordList);
            }
            return true;
        }

        private string RevertFontColorFix(string s)
        {
            var tagIndex = s.IndexOf("<font", StringComparison.OrdinalIgnoreCase);
            while (tagIndex >= 0)
            {
                var tagEndIndex = s.IndexOf('>', tagIndex + 5);
                if (tagEndIndex < 0)
                    break;
                var tag = s.Substring(tagIndex, tagEndIndex - tagIndex);
                var colourIndex = tag.IndexOf("colour", StringComparison.OrdinalIgnoreCase);
                while (colourIndex >= 0)
                {
                    tag = tag.Remove(colourIndex + 4, 1); // colour => color
                    colourIndex = tag.IndexOf("colour", colourIndex + 5, StringComparison.OrdinalIgnoreCase);
                }
                s = s.Remove(tagIndex, tagEndIndex - tagIndex).Insert(tagIndex, tag);
                tagIndex = s.IndexOf("<font", tagIndex + tag.Length + 1, StringComparison.OrdinalIgnoreCase);
            }
            return s;
        }

    }
}
