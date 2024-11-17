﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class SubRip : SubtitleFormat
    {
        public string Errors { get; private set; }
        private StringBuilder _errors;

        private enum ExpectingLine
        {
            Number,
            TimeCodes,
            Text
        }

        private Paragraph _paragraph;
        private ExpectingLine _expecting = ExpectingLine.Number;
        private static Regex _regexTimeCodes = new Regex(@"^-?\d+:-?\d+:-?\d+[:,]-?\d+\s*-->\s*-?\d+:-?\d+:-?\d+[:,]-?\d+$", RegexOptions.Compiled);
        private static Regex _regexTimeCodes2 = new Regex(@"^\d+:\d+:\d+,\d+\s*-->\s*\d+:\d+:\d+,\d+$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".srt"; }
        }

        public override string Name
        {
            get { return "SubRip"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string writeFormat = "{0}\r\n{1} --> {2}\r\n{3}\r\n\r\n";

            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                var s = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine)
                    .Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                sb.AppendFormat(writeFormat, p.Number, p.StartTime, p.EndTime, p.Text);
            }
            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            bool doRenum = false;
            _errors = new StringBuilder();

            _paragraph = new Paragraph();
            _expecting = ExpectingLine.Number;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].TrimEnd();
                line = line.Trim('\u007F'); // 127=delete acscii

                var next = string.Empty;
                if (i + 1 < lines.Count)
                    next = lines[i + 1];

                // A new line is missing between two paragraphs (buggy srt file)
                if (_expecting == ExpectingLine.Text && i + 1 < lines.Count &&
                    _paragraph != null && !string.IsNullOrEmpty(_paragraph.Text) && Utilities.IsInteger(line) &&
                    _regexTimeCodes.IsMatch(lines[i + 1]))
                {
                    ReadLine(subtitle, string.Empty, string.Empty);
                }
                if (_expecting == ExpectingLine.Number && _regexTimeCodes.IsMatch(line))
                {
                    _expecting = ExpectingLine.TimeCodes;
                    doRenum = true;
                }

                ReadLine(subtitle, line, next);
            }
            if (_paragraph.EndTime.TotalMilliseconds > _paragraph.StartTime.TotalMilliseconds)
                subtitle.Paragraphs.Add(_paragraph);

            foreach (Paragraph p in subtitle.Paragraphs)
                p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

            if (doRenum)
                subtitle.Renumber(1);

            Errors = _errors.ToString();
        }

        private void ReadLine(Subtitle subtitle, string line, string next)
        {
            switch (_expecting)
            {
                case ExpectingLine.Number:
                    if (Utilities.IsInteger(line))
                    {
                        _paragraph.Number = int.Parse(line);
                        _expecting = ExpectingLine.TimeCodes;
                    }
                    else if (line.Trim().Length > 0)
                    {
                        _errorCount++;
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
                        _errorCount++;
                        _expecting = ExpectingLine.Number; // lets go to next paragraph
                    }
                    break;
                case ExpectingLine.Text:
                    if (line.Trim().Length > 0)
                    {
                        if (_paragraph.Text.Length > 0)
                            _paragraph.Text += Environment.NewLine;
                        _paragraph.Text += RemoveBadChars(line).TrimEnd().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    }
                    else if (IsText(next))
                    {
                        if (_paragraph.Text.Length > 0)
                            _paragraph.Text += Environment.NewLine;
                        _paragraph.Text += RemoveBadChars(line).TrimEnd().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    }
                    else if (string.IsNullOrEmpty(line) && string.IsNullOrEmpty(_paragraph.Text))
                    {
                        _paragraph.Text = string.Empty;
                        if (!string.IsNullOrEmpty(next) && (Utilities.IsInteger(next) || _regexTimeCodes.IsMatch(next)))
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

        private bool IsText(string text)
        {
            if (text.Trim().Length == 0 || Utilities.IsInteger(text) || _regexTimeCodes.IsMatch(text))
                return false;
            return true;
        }

        private string RemoveBadChars(string line)
        {
            return line.Replace('\0', ' ');
        }

        private bool TryReadTimeCodesLine(string line, Paragraph paragraph)
        {
            const string defaultDash = " --> ";
            line = line.Replace('،', ',');
            line = line.Replace('', ',');
            line = line.Replace('¡', ',');

            // Fix some badly formatted separator sequences - anything can happen if you manually edit ;)
            line = line.Replace(" -> ", defaultDash); // I've seen this
            line = line.Replace(" - > ", defaultDash);
            line = line.Replace(" ->> ", defaultDash);
            line = line.Replace(" -- > ", defaultDash);
            line = line.Replace(" - -> ", defaultDash);
            line = line.Replace(" -->> ", defaultDash);
            line = line.Replace(" ---> ", defaultDash);

            // Removed stuff after timecodes - like subtitle position
            //  - example of position info: 00:02:26,407 --> 00:02:31,356  X1:100 X2:100 Y1:100 Y2:100
            if (line.Length > 30 && line[29] == ' ')
                line = line.Substring(0, 29);

            // removes all extra spaces
            line = line.Replace(" ", string.Empty).Replace("-->", defaultDash);
            line = line.Trim();

            // Fix a few more cases of wrong time codes, seen this: 00.00.02,000 --> 00.00.04,000
            line = line.Replace('.', ':');
            if (line.Length >= 29 && ":;".Contains(line[8].ToString()))
                line = line.Substring(0, 8) + ',' + line.Substring(8 + 1);
            if (line.Length >= 29 && line.Length <= 30 && ":;".Contains(line[25].ToString()))
                line = line.Substring(0, 25) + ',' + line.Substring(25 + 1);

            if (_regexTimeCodes.IsMatch(line) || _regexTimeCodes2.IsMatch(line))
            {
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
                    return false;
                }
            }
            return false;
        }

        public override List<string> AlternateExtensions
        {
            get
            {
                return new List<string> { ".wsrt" };
            }
        }
    }
}
