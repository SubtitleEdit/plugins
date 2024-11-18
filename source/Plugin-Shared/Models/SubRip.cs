using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class SubRip
    {
        private static readonly Regex RegexTimeCodes =
            new Regex(@"^-?\d+:-?\d+:-?\d+[:,]-?\d+\s*-->\s*-?\d+:-?\d+:-?\d+[:,]-?\d+$", RegexOptions.Compiled);

        private static readonly Regex RegexTimeCodes2 = new Regex(@"^\d+:\d+:\d+,\d+\s*-->\s*\d+:\d+:\d+,\d+$",
            RegexOptions.Compiled);

        private ExpectingLine _expecting = ExpectingLine.Number;
        private Paragraph _paragraph;

        public void LoadSubtitle(Subtitle subtitle, IList<string> lines, string fileName)
        {
            _paragraph = new Paragraph();
            _expecting = ExpectingLine.Number;
            if (subtitle?.Paragraphs?.Count > 0)
            {
                subtitle.Paragraphs.Clear();
            }
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i].TrimEnd();
                line = line.Trim('\u007F'); // Strip DEL ASCII

                var next = string.Empty;
                if (i + 1 < lines.Count)
                {
                    next = lines[i + 1];
                }

                // A new line is missing between two paragraphs (buggy srt file)
                if (_expecting == ExpectingLine.Text && i + 1 < lines.Count &&
                    _paragraph != null && !string.IsNullOrEmpty(_paragraph.Text) && StringUtils.IsInteger(line) &&
                    RegexTimeCodes.IsMatch(lines[i + 1]))
                {
                    ReadLine(subtitle, string.Empty, string.Empty);
                }
                if (_expecting == ExpectingLine.Number && RegexTimeCodes.IsMatch(line))
                {
                    _expecting = ExpectingLine.TimeCodes;
                }
                ReadLine(subtitle, line, next);
            }

            if (!string.IsNullOrWhiteSpace(_paragraph.Text))
            {
                subtitle.Paragraphs.Add(_paragraph);
            }
        }

        public string ToText(Subtitle subtitle, string title)
        {
            const string writeFormat = "{0}{4}{1} --> {2}{4}{3}{4}{4}";
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendFormat(writeFormat, p.Number, p.StartTime, p.EndTime, p.Text, Environment.NewLine);
            }
            return sb.ToString().Trim();
        }

        private static bool IsText(string text) => !(string.IsNullOrWhiteSpace(text) || StringUtils.IsInteger(text) || RegexTimeCodes.IsMatch(text));

        private void ReadLine(Subtitle subtitle, string line, string next)
        {
            switch (_expecting)
            {
                case ExpectingLine.Number:
                    if (StringUtils.IsInteger(line))
                    {
                        _paragraph.Number = int.Parse(line);
                        _expecting = ExpectingLine.TimeCodes;
                    }
                    break;

                case ExpectingLine.TimeCodes:
                    if (TryReadTimeCodesLine(line, _paragraph))
                    {
                        _paragraph.Text = string.Empty;
                        _expecting = ExpectingLine.Text;
                    }
                    else if (line.Trim().Length > 0)
                    {
                        _expecting = ExpectingLine.Number; // lets go to next paragraph
                    }
                    break;

                case ExpectingLine.Text:
                    if (!string.IsNullOrWhiteSpace(line) || IsText(next))
                    {
                        if (_paragraph.Text.Length > 0)
                        {
                            _paragraph.Text += Environment.NewLine;
                        }

                        _paragraph.Text += StringUtils.RemoveBadChars(line).TrimEnd().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    }
                    else if (string.IsNullOrEmpty(line) && string.IsNullOrEmpty(_paragraph.Text))
                    {
                        _paragraph.Text = string.Empty;
                        if (!string.IsNullOrEmpty(next) && (StringUtils.IsInteger(next) || RegexTimeCodes.IsMatch(next)))
                        {
                            subtitle.Paragraphs.Add(_paragraph);
                            _paragraph = new Paragraph();
                            _expecting = ExpectingLine.Number;
                        }
                    }
                    else
                    {
                        subtitle.Paragraphs.Add(_paragraph);
                        _paragraph = new Paragraph();
                        _expecting = ExpectingLine.Number;
                    }
                    break;
            }
        }

        private static bool TryReadTimeCodesLine(string line, Paragraph paragraph)
        {
            if (!(RegexTimeCodes.IsMatch(line) || RegexTimeCodes2.IsMatch(line)))
            {
                return false;
            }
            string[] parts = line.Replace("-->", ":").Replace(" ", string.Empty).Split(':', ',');
            try
            {
                int startHours = int.Parse(parts[0]);
                int startMinutes = int.Parse(parts[1]);
                int startSeconds = int.Parse(parts[2]);
                int startMilliseconds = int.Parse(parts[3]);

                int endHours = int.Parse(parts[4]);
                int endMinutes = int.Parse(parts[5]);
                int endSeconds = int.Parse(parts[6]);
                int endMilliseconds = int.Parse(parts[7]);

                paragraph.StartTime = new TimeCode(startHours, startMinutes, startSeconds, startMilliseconds);
                paragraph.EndTime = new TimeCode(endHours, endMinutes, endSeconds, endMilliseconds);
                return true;
            }
            catch
            {
                // ignored
            }

            return false;
        }
    }
}