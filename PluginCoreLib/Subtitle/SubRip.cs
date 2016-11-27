namespace Nikse.SubtitleEdit.PluginLogic.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using PluginCoreLib.Subtitle;
    using PluginCoreLib.Utils;

    /// <summary>
    ///  A simplified SubRip parser will suffice, because the input text is provided by Subtitle Edit.
    /// </summary>
    public class SubRip
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"\A(-?[0-9]+):(-?[0-9]+):(-?[0-9]+)[:,](-?[0-9]+)\s*-->\s*(-?[0-9]+):(-?[0-9]+):(-?[0-9]+)[:,](-?[0-9]+)(?=\z|\s)", RegexOptions.Compiled);

        private int _errorCount;

        public string Name => "SubRip";

        public string Extension => ".srt";

        public int Errors => _errorCount;

        public void LoadSubtitle(Subtitle subtitle, IList<string> lines, string fileName)
        {
            subtitle.FileName = fileName;
            _errorCount = 0;
            subtitle.Clear();

            var expecting = LineType.Number;
            var p = new Paragraph();

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                switch (expecting)
                {
                    case LineType.Number:
                        if (line.Length > 0)
                        {
                            try
                            {
                                p.Number = int.Parse(line, NumberStyles.None, CultureInfo.InvariantCulture);
                            }
                            catch // Keep looking for paragraph number if fails parsing line...
                            {
                                _errorCount++;
                            }
                        }
                        else
                        {
                            _errorCount++;
                        }
                        break;

                    case LineType.TimeCodes:
                        Match match = null;
                        if (line.Length > 19)
                        {
                            match = RegexTimeCodes.Match(line);
                        }
                        if (match?.Success == true)
                        {
                            // Parse start timestamp.
                            int startHours = int.Parse(match.Groups[1].Value, NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);
                            int startMinutes = int.Parse(match.Groups[2].Value, NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);
                            int startSeconds = int.Parse(match.Groups[3].Value, NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);
                            int startMilliseconds = int.Parse(match.Groups[4].Value, NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);


                            // Parse end timestamp.
                            int endHours = int.Parse(match.Groups[5].Value, NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);
                            int endMinutes = int.Parse(match.Groups[6].Value, NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);
                            int endSeconds = int.Parse(match.Groups[7].Value, NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);
                            int endMilliseconds = int.Parse(match.Groups[8].Value, NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);

                            p.StartTime = new TimeCode(startHours, startMilliseconds, startSeconds, startMilliseconds);
                            p.EndTime = new TimeCode(endHours, endMilliseconds, endSeconds, endMilliseconds);
                            expecting = LineType.Text;
                        }
                        else
                        {
                            // Start over.
                            _errorCount++;
                            p = new Paragraph();
                            expecting = LineType.Number;
                        }
                        break;

                    case LineType.Text:
                        if (line.Length > 0)
                        {
                            p.Text += p.Text.Length > 0 ? Environment.NewLine + line : line;
                        }
                        else
                        {
                            subtitle.Add(p);
                            p = new Paragraph();
                            expecting = LineType.Number;
                        }
                        break;
                }
            }
        }

        public string ToText(Subtitle subtitle, string title)
        {
            // Paragraph write format.
            const string writeFormat = "{1}{0}{2} --> {3}{0}{4}{0}{0}";
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, writeFormat, Environment.NewLine, p.Number, p.StartTime, p.EndTime, p.Text);
            }
            return sb.ToString();
        }

    }
}
