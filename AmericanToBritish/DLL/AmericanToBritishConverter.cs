using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class AmericanToBritishConverter
    {
        // built-in words
        private readonly List<Regex> _regexListBuiltIn = new List<Regex>();
        private readonly List<string> _replaceListBuitIn = new List<string>();

        // local words
        private readonly List<Regex> _regexListLocal = new List<Regex>();
        private readonly List<string> _replaceListLocal = new List<string>();

        public AmericanToBritishConverter()
        {
            LoadBuiltInWords();
        }

        public AmericanToBritishConverter(string localListPath) : this()
        {
            LoadLocalWords(localListPath);
        }

        public string FixText(string text, ListType listType)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            switch (listType)
            {
                case ListType.BuiltIn:
                    for (int index = 0; index < _regexListBuiltIn.Count; index++)
                    {
                        var regex = _regexListBuiltIn[index];
                        if (regex.IsMatch(text))
                        {
                            text = regex.Replace(text, _replaceListBuitIn[index]);
                        }
                    }
                    break;
                case ListType.Local:
                    // Todo: make sure local-list is loaded!
                    for (int index = 0; index < _regexListLocal.Count; index++)
                    {
                        var regex = _regexListLocal[index];
                        if (regex.IsMatch(text))
                        {
                            text = regex.Replace(text, _replaceListLocal[index]);
                        }
                    }
                    break;
            }
            return FixMissChangedWord(text);
        }

        public void LoadBuiltInWords()
        {
            if (_regexListBuiltIn.Count > 0 && _replaceListBuitIn.Count > 0)
                return;
            _regexListBuiltIn.Clear();
            _replaceListBuitIn.Clear();

            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Nikse.SubtitleEdit.PluginLogic.WordList.xml"))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var xdoc = XDocument.Parse(reader.ReadToEnd());
                        foreach (XElement xElement in xdoc.Descendants("Word"))
                        {
                            string american = xElement.Attribute("us").Value;
                            string british = xElement.Attribute("br").Value;
                            if (!(string.IsNullOrWhiteSpace(american) || string.IsNullOrWhiteSpace(british)) && american != british)
                            {
                                _regexListBuiltIn.Add(new Regex("\\b" + american + "\\b", RegexOptions.Compiled));
                                _replaceListBuitIn.Add(british);

                                _regexListBuiltIn.Add(new Regex("\\b" + american.ToUpperInvariant() + "\\b", RegexOptions.Compiled));
                                _replaceListBuitIn.Add(british.ToUpperInvariant());

                                if (american.Length > 1)
                                {
                                    _regexListBuiltIn.Add(new Regex("\\b" + char.ToUpperInvariant(american[0]) + american.Substring(1) + "\\b", RegexOptions.Compiled));
                                    if (british.Length > 1)
                                        _replaceListBuitIn.Add(char.ToUpperInvariant(british[0]) + british.Substring(1));
                                    else
                                        _replaceListBuitIn.Add(british.ToUpper());
                                }
                            }
                        }
                    }
                }
            }
        }

        public void LoadLocalWords(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path) || !File.Exists(path))
                return;
            // always reload list
            var xDoc = XDocument.Load(path);
            if (xDoc?.Root.Name == "Words")
            {
                // todo: make load words generic to allow loading both built-in and local
                foreach (XElement xe in xDoc.Root.Elements("Word"))
                {
                    if (xe.Attribute("us")?.Value.Length > 1 && xe.Attribute("br")?.Value.Length > 1)
                    {
                        string american = xe.Attribute("us").Value;
                        string british = xe.Attribute("us").Value;

                        _regexListLocal.Add(new Regex("\\b" + american + "\\b", RegexOptions.Compiled));
                        _replaceListLocal.Add(british);

                        _regexListLocal.Add(new Regex("\\b" + american.ToUpperInvariant() + "\\b", RegexOptions.Compiled));
                        _replaceListLocal.Add(british.ToUpperInvariant());

                        _regexListLocal.Add(new Regex("\\b" + char.ToUpperInvariant(american[0]) + american.Substring(1) + "\\b", RegexOptions.Compiled));
                        if (british.Length > 1)
                            _replaceListLocal.Add(char.ToUpperInvariant(british[0]) + british.Substring(1));
                        else
                            _replaceListLocal.Add(british.ToUpper());
                    }
                }
            }
        }

        private string FixMissChangedWord(string s)
        {
            var idx = s.IndexOf("<font", StringComparison.OrdinalIgnoreCase);
            while (idx >= 0) // Fix colour => color
            {
                var endIdx = s.IndexOf('>', idx + 5);
                if (endIdx < 5)
                    break;
                var tag = s.Substring(idx, endIdx - idx);
                tag = tag.Replace("colour", "color");
                tag = tag.Replace("COLOUR", "COLOR");
                tag = tag.Replace("Colour", "Color");
                s = s.Remove(idx, endIdx - idx).Insert(idx, tag);
                idx = s.IndexOf("<font", endIdx + 1, StringComparison.OrdinalIgnoreCase);
            }
            return s;
        }
    }
}
