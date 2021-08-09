using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SubtitleEdit.Logic
{
    public class Subtitle
    {
        public const int MaxFileSize = 1024 * 1024 * 20; // 20 MB

        public List<Paragraph> Paragraphs { get; private set; }

        public string Header { get; set; } = string.Empty;
        public string Footer { get; set; } = string.Empty;

        public string FileName { get; set; }

        public static int MaximumHistoryItems => 100;


        public Subtitle()
        {
            Paragraphs = new List<Paragraph>();
            FileName = "Untitled";
        }

        /// <summary>
        /// Copy constructor (without history).
        /// </summary>
        /// <param name="subtitle">Subtitle to copy</param>
        /// <param name="generateNewId">Generate new ID (guid) for paragraphs</param>
        public Subtitle(Subtitle subtitle, bool generateNewId = true)
        {
            if (subtitle == null)
            {
                FileName = "Untitled";
                Paragraphs = new List<Paragraph>();
                return;
            }

            Paragraphs = new List<Paragraph>(subtitle.Paragraphs.Count);
            foreach (var p in subtitle.Paragraphs)
            {
                Paragraphs.Add(new Paragraph(p, generateNewId));
            }

            Header = subtitle.Header;
            Footer = subtitle.Footer;
            FileName = subtitle.FileName;
        }

        public Subtitle(List<Paragraph> paragraphs) : this()
        {
            Paragraphs = paragraphs;
        }

        /// <summary>
        /// Get the paragraph of index, null if out of bounds
        /// </summary>
        /// <param name="index">Index of wanted paragraph</param>
        /// <returns>Paragraph, null if index is index is out of bounds</returns>
        public Paragraph GetParagraphOrDefault(int index)
        {
            if (Paragraphs == null || Paragraphs.Count <= index || index < 0)
            {
                return null;
            }

            return Paragraphs[index];
        }

        public Paragraph GetParagraphOrDefaultById(string id)
        {
            return Paragraphs.Find(p => p.Id == id);
        }

        public SubtitleFormat ReloadLoadSubtitle(List<string> lines, string fileName, SubtitleFormat format)
        {
            Paragraphs.Clear();
            if (format != null && format.IsMine(lines, fileName))
            {
                format.LoadSubtitle(this, lines, fileName);
                return format;
            }
            return null;
        }

        /// <summary>
        /// Load a subtitle from a file.
        /// Check "OriginalFormat" to see what subtitle format was used.
        /// Check "OriginalEncoding" to see what text encoding was used.
        /// </summary>
        /// <param name="fileName">File name of subtitle to load.</param>
        /// <returns>Loaded subtitle, null if file is not known subtitle format.</returns>
        public static Subtitle Parse(string fileName)
        {
            var subtitle = new Subtitle();
            var format = subtitle.LoadSubtitle(fileName, out var encodingUsed, null);
            if (format == null)
            {
                return null;
            }

            return subtitle;
        }

        public SubtitleFormat LoadSubtitle(string fileName, out Encoding encoding, Encoding useThisEncoding)
        {
            return LoadSubtitle(fileName, out encoding, useThisEncoding, false);
        }

        public SubtitleFormat LoadSubtitle(string fileName, out Encoding encoding, Encoding useThisEncoding, bool batchMode, double? sourceFrameRate = null, bool loadSubtitle = true)
        {
            FileName = fileName;
            Paragraphs = new List<Paragraph>();
            List<string> lines;
            try
            {
                lines = ReadLinesFromFile(fileName, useThisEncoding, out encoding);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                encoding = Encoding.UTF8;
                return null;
            }

            if (useThisEncoding == null)
            {
                return LoadSubtitle(fileName, out encoding, Encoding.Unicode);
            }

            return null;
        }

        private static List<string> ReadLinesFromFile(string fileName, Encoding useThisEncoding, out Encoding encoding)
        {
            StreamReader sr;
            if (useThisEncoding != null)
            {
                sr = new StreamReader(fileName, useThisEncoding);
            }
            else
            {
                try
                {
                    sr = new StreamReader(fileName, true);
                }
                catch
                {
                    var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    sr = new StreamReader(fs);
                }
            }

            encoding = sr.CurrentEncoding;
            var lines = sr.ReadToEnd().SplitToLines();
            sr.Close();
            return lines;
        }


        /// <summary>
        /// Creates subtitle as text in its native format.
        /// </summary>
        /// <param name="format">Format to output</param>
        /// <returns>Native format as text string</returns>
        public string ToText(SubtitleFormat format)
        {
            return format.ToText(this, Path.GetFileNameWithoutExtension(FileName));
        }

        public void AddTimeToAllParagraphs(TimeSpan time)
        {
            double milliseconds = time.TotalMilliseconds;
            foreach (var p in Paragraphs)
            {
                p.StartTime.TotalMilliseconds += milliseconds;
                p.EndTime.TotalMilliseconds += milliseconds;
            }
        }

        public void Renumber(int startNumber = 1)
        {
            var number = startNumber;
            int l = Paragraphs.Count + number;
            while (number < l)
            {
                var p = Paragraphs[number - startNumber];
                p.Number = number++;
            }
        }

        public int GetIndex(Paragraph p)
        {
            if (p == null)
            {
                return -1;
            }

            int index = Paragraphs.IndexOf(p);
            if (index >= 0)
            {
                return index;
            }

            for (int i = 0; i < Paragraphs.Count; i++)
            {
                if (p.Id == Paragraphs[i].Id)
                {
                    return i;
                }

                if (i < Paragraphs.Count - 1 && p.Id == Paragraphs[i + 1].Id)
                {
                    return i + 1;
                }

                if (Math.Abs(p.StartTime.TotalMilliseconds - Paragraphs[i].StartTime.TotalMilliseconds) < 0.1 &&
                    Math.Abs(p.EndTime.TotalMilliseconds - Paragraphs[i].EndTime.TotalMilliseconds) < 0.1)
                {
                    return i;
                }

                if (p.Number == Paragraphs[i].Number && (Math.Abs(p.StartTime.TotalMilliseconds - Paragraphs[i].StartTime.TotalMilliseconds) < 0.1 ||
                    Math.Abs(p.EndTime.TotalMilliseconds - Paragraphs[i].EndTime.TotalMilliseconds) < 0.1))
                {
                    return i;
                }

                if (p.Text == Paragraphs[i].Text && (Math.Abs(p.StartTime.TotalMilliseconds - Paragraphs[i].StartTime.TotalMilliseconds) < 0.1 ||
                    Math.Abs(p.EndTime.TotalMilliseconds - Paragraphs[i].EndTime.TotalMilliseconds) < 0.1))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get paragraph index by time in seconds
        /// </summary>
        public int GetIndex(double seconds)
        {
            var totalMilliseconds = seconds * TimeCode.BaseUnit;
            for (int i = 0; i < Paragraphs.Count; i++)
            {
                var p = Paragraphs[i];
                if (totalMilliseconds >= p.StartTime.TotalMilliseconds && totalMilliseconds <= p.EndTime.TotalMilliseconds)
                {
                    return i;
                }
            }
            return -1;
        }

        public Paragraph GetFirstAlike(Paragraph p)
        {
            foreach (var item in Paragraphs)
            {
                if (Math.Abs(p.StartTime.TotalMilliseconds - item.StartTime.TotalMilliseconds) < 0.1 &&
                    Math.Abs(p.EndTime.TotalMilliseconds - item.EndTime.TotalMilliseconds) < 0.1 &&
                    p.Text == item.Text)
                {
                    return item;
                }
            }
            return null;
        }

        public Paragraph GetNearestAlike(Paragraph p)
        {
            foreach (var item in Paragraphs)
            {
                if (Math.Abs(p.StartTime.TotalMilliseconds - item.StartTime.TotalMilliseconds) < 0.1 &&
                    Math.Abs(p.EndTime.TotalMilliseconds - item.EndTime.TotalMilliseconds) < 0.1 &&
                    p.Text == item.Text)
                {
                    return item;
                }
            }

            foreach (var item in Paragraphs)
            {
                if (Math.Abs(p.StartTime.TotalMilliseconds - item.StartTime.TotalMilliseconds) < 0.1 &&
                    Math.Abs(p.EndTime.TotalMilliseconds - item.EndTime.TotalMilliseconds) < 0.1)
                {
                    return item;
                }
            }

            return Paragraphs.OrderBy(s => Math.Abs(s.StartTime.TotalMilliseconds - p.StartTime.TotalMilliseconds)).FirstOrDefault();
        }

        public int RemoveEmptyLines()
        {
            int count = Paragraphs.Count;
            if (count <= 0)
            {
                return 0;
            }

            int firstNumber = Paragraphs[0].Number;
            for (int i = Paragraphs.Count - 1; i >= 0; i--)
            {
                var p = Paragraphs[i];
                if (p.Text.IsOnlyControlCharactersOrWhiteSpace())
                {
                    Paragraphs.RemoveAt(i);
                }
            }
            if (count != Paragraphs.Count)
            {
                Renumber(firstNumber);
            }
            return count - Paragraphs.Count;
        }

        /// <summary>
        /// Removes paragraphs by a list of indices
        /// </summary>
        /// <param name="indices">Indices of paragraphs/lines to delete</param>
        /// <returns>Number of lines deleted</returns>
        public int RemoveParagraphsByIndices(IEnumerable<int> indices)
        {
            int count = 0;
            foreach (var index in indices.OrderByDescending(p => p))
            {
                if (index >= 0 && index < Paragraphs.Count)
                {
                    Paragraphs.RemoveAt(index);
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Removes paragraphs by a list of IDs
        /// </summary>
        /// <param name="ids">IDs of paragraphs/lines to delete</param>
        /// <returns>Number of lines deleted</returns>
        public int RemoveParagraphsByIds(IEnumerable<string> ids)
        {
            int beforeCount = Paragraphs.Count;
            Paragraphs = Paragraphs.Where(p => !ids.Contains(p.Id)).ToList();
            return beforeCount - Paragraphs.Count;
        }

        public int InsertParagraphInCorrectTimeOrder(Paragraph newParagraph)
        {
            for (int i = 0; i < Paragraphs.Count; i++)
            {
                var p = Paragraphs[i];
                if (newParagraph.StartTime.TotalMilliseconds < p.StartTime.TotalMilliseconds)
                {
                    Paragraphs.Insert(i, newParagraph);
                    return i;
                }
            }
            Paragraphs.Add(newParagraph);
            return Paragraphs.Count - 1;
        }

        public Paragraph GetFirstParagraphOrDefaultByTime(double milliseconds)
        {
            foreach (var p in Paragraphs)
            {
                if (p.StartTime.TotalMilliseconds < milliseconds && milliseconds < p.EndTime.TotalMilliseconds)
                {
                    return p;
                }
            }
            return null;
        }

        /// <summary>
        /// Fast hash code for subtitle - includes pre (encoding atm) + header + number + start + end + text + style + actor + extra.
        /// </summary>
        /// <returns>Hash value that can be used for quick compare</returns>
        public int GetFastHashCode(string pre)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                if (pre != null)
                {
                    hash = hash * 23 + pre.GetHashCode();
                }
                if (Header != null)
                {
                    hash = hash * 23 + Header.Trim().GetHashCode();
                }
                if (Footer != null)
                {
                    hash = hash * 23 + Footer.Trim().GetHashCode();
                }
                var max = Paragraphs.Count;
                for (int i = 0; i < max; i++)
                {
                    var p = Paragraphs[i];
                    hash = hash * 23 + p.Number.GetHashCode();
                    hash = hash * 23 + p.StartTime.TotalMilliseconds.GetHashCode();
                    hash = hash * 23 + p.EndTime.TotalMilliseconds.GetHashCode();
                    hash = hash * 23 + p.Text.GetHashCode();
                    if (p.Style != null)
                    {
                        hash = hash * 23 + p.Style.GetHashCode();
                    }
                    if (p.Extra != null)
                    {
                        hash = hash * 23 + p.Extra.GetHashCode();
                    }
                    if (p.Actor != null)
                    {
                        hash = hash * 23 + p.Actor.GetHashCode();
                    }
                }

                return hash;
            }
        }

        /// <summary>
        /// Fast hash code for subtitle text.
        /// </summary>
        /// <returns>Hash value that can be used for quick text compare</returns>
        public int GetFastHashCodeTextOnly()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                var max = Paragraphs.Count;
                for (int i = 0; i < max; i++)
                {
                    var p = Paragraphs[i];
                    hash = hash * 23 + p.Text.GetHashCode();
                }

                return hash;
            }
        }

        /// <summary>
        /// Concatenates all Paragraphs Text property, using the default NewLine string between each Text.
        /// </summary>
        /// <returns>Concatenated Text property of all Paragraphs.</returns>
        public string GetAllTexts()
        {
            int max = Paragraphs.Count;
            const int averageLength = 40;
            var sb = new StringBuilder(max * averageLength);
            for (var index = 0; index < max; index++)
            {
                sb.AppendLine(Paragraphs[index].Text);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Concatenates all Paragraphs Text property, using the default NewLine string between each Text.
        /// </summary>
        /// <returns>Concatenated Text property of all Paragraphs.</returns>
        public string GetAllTexts(int stopAfterBytes)
        {
            int max = Paragraphs.Count;
            const int averageLength = 40;
            var sb = new StringBuilder(max * averageLength);
            for (var index = 0; index < max; index++)
            {
                sb.AppendLine(Paragraphs[index].Text);
                if (sb.Length > stopAfterBytes)
                {
                    return sb.ToString();
                }
            }
            return sb.ToString();
        }
    }
}
