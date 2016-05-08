using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic.Logic
{
    internal class SubRip
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"\A(-?[0-9]+):(-?[0-9]+):(-?[0-9]+)[:,](-?[0-9]+)\s*-->\s*(-?[0-9]+):(-?[0-9]+):(-?[0-9]+)[:,](-?[0-9]+)(?=\z|\s)", RegexOptions.Compiled);
        private int _errorCount;

        private enum LineType
        {
            Number,
            TimeCodes,
            Text
        }

        public int Errors
        {
            get { return _errorCount; }
        }

        public string Extension
        {
            get { return ".srt"; }
        }

        public string Name
        {
            get { return "SubRip"; }
        }

        public string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{1}{0}{2} --> {3}{0}{4}{0}{0}";
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, paragraphWriteFormat, Environment.NewLine, p.Number, p.StartTime, p.EndTime, p.Text);
            }
            return sb.ToString();
        }

        // A simplified SubRip parser will suffice, because the input text is provided by Subtitle Edit.
        private Paragraph _paragraph;
        private LineType _expecting;
        private string _currentLine;

        public void LoadSubtitle(Subtitle subtitle, IList<string> lines, string fileName)
        {
            _paragraph = new Paragraph();
            _expecting = LineType.Number;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                _currentLine = lines[i];
                var line = _currentLine.Trim();

                if (!TryParseLine(subtitle, line))
                    _errorCount++;
            }
            if (_expecting == LineType.Text)
                subtitle.Paragraphs.Add(_paragraph);
            else if (_expecting != LineType.Number)
                _errorCount++;

            subtitle.FileName = fileName;
        }

        private bool TryParseLine(Subtitle subtitle, string line)
        {
            int number;
            Match timecodes;
            bool success = true;

            if (_currentLine.Length == 0)
            {
                // Number    : ok, ignore surplus empty line
                // TimeCodes : error, skip up to next number
                // Text      : ok, separator between paragraphs
                if (_expecting == LineType.Text)
                {
                    subtitle.Paragraphs.Add(_paragraph);
                    _paragraph = new Paragraph();
                }
                else if (_expecting == LineType.TimeCodes)
                {
                    success = false;
                }
                _expecting = LineType.Number;
            }
            else if (int.TryParse(line, NumberStyles.None, CultureInfo.InvariantCulture, out number))
            {
                // Number    : ok, as expected
                // TimeCodes : error, discard previous number
                // Text      : ok, text is a number
                if (_expecting == LineType.TimeCodes)
                {
                    success = false;
                    _expecting = LineType.Number;
                }
                if (_expecting == LineType.Number)
                {
                    _paragraph.Number = number;
                    _expecting = LineType.TimeCodes;
                }
                else // (_expecting == LineType.Text)
                {
                    AddCurrentLineToParagraphText();
                }
            }
            else if ((timecodes = RegexTimeCodes.Match(line)).Success)
            {
                // Number    : error, presume missing number
                // TimeCodes : ok, as expected
                // Text      : odd, but not prohibited
                if (_expecting == LineType.Number)
                {
                    success = false;
                    _expecting = LineType.TimeCodes;
                }
                if (_expecting == LineType.TimeCodes)
                {
                    try
                    {
                        var hours = int.Parse(timecodes.Groups[1].Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                        var minutes = int.Parse(timecodes.Groups[2].Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                        var seconds = int.Parse(timecodes.Groups[3].Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                        var milliseconds = int.Parse(timecodes.Groups[4].Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                        _paragraph.StartTime = new TimeCode(hours, minutes, seconds, milliseconds);
                    }
                    catch
                    {
                        success = false; // oops, overflow!
                        _paragraph.StartTime = new TimeCode(99, 59, 59, 999);
                    }
                    try
                    {
                        var hours = int.Parse(timecodes.Groups[5].Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                        var minutes = int.Parse(timecodes.Groups[6].Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                        var seconds = int.Parse(timecodes.Groups[7].Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                        var milliseconds = int.Parse(timecodes.Groups[8].Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                        _paragraph.EndTime = new TimeCode(hours, minutes, seconds, milliseconds);
                    }
                    catch
                    {
                        success = false; // oops, overflow!
                        _paragraph.EndTime = new TimeCode(99, 59, 59, 999);
                    }
                    _paragraph.Text = string.Empty;
                    _expecting = LineType.Text;
                }
                else // (_expecting == LineType.Text)
                {
                    AddCurrentLineToParagraphText();
                }
            }
            else
            {
                // Number    : error, skip up to next number
                // TimeCodes : error, skip up to next number
                // Text      : ok, as expected
                if (_expecting != LineType.Text)
                {
                    success = false;
                    _paragraph = new Paragraph();
                    _expecting = LineType.Number;
                }
                else // (_expecting == LineType.Text)
                {
                    AddCurrentLineToParagraphText();
                }
            }
            return success;
        }

        private void AddCurrentLineToParagraphText()
        {
            if (_paragraph.Text.Length > 0)
                _paragraph.Text += Environment.NewLine;
            _paragraph.Text += _currentLine;
        }

    }
}
