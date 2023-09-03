using System;
using System.Collections.Generic;
using System.Linq;
// using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.PluginLogic.Models;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class RemoveLineBreak
    {
        // private readonly Regex _regexNarrator = new Regex(":\\B", RegexOptions.Compiled);
        private readonly UnBreakConfigs _configs;
        // private readonly char[] _moodChars = { '(', '[' };

        public RemoveLineBreak(UnBreakConfigs configs)
        {
            _configs = configs;
        }

        public ICollection<RemoveLineBreakResult> Remove(IList<Paragraph> paragraphs)
        {
            var result = new List<RemoveLineBreakResult>();
            foreach (var paragraph in paragraphs.ToSmartParagraphs())
            {
                if (!paragraph.IsUnbreakable(_configs.MaxLineLength))
                {
                    continue;
                }
                
                var text = Unbreak(paragraph);
                result.Add(new RemoveLineBreakResult()
                {
                    Paragraph = paragraph.Paragraph,
                    AfterText = text
                });
            }

            return result;
        }

        private string Unbreak(SmartParagraph smartParagraph)
        {
            // var noTagText = HtmlUtils.RemoveTags(smartParagraph, true);
            // smartParagraph = smartParagraph.FixExtraSpaces().Trim();
            var lines = smartParagraph.Lines.ToArray();

            // dialog
            if (_configs.SkipDialogs && lines.Any(line => line.IsDialog))
            {
                return smartParagraph.Text;
            }

            // mood
            if (_configs.SkipMoods && lines.Any(line => line.HasMood))
            {
                return smartParagraph.Text;
            }

            // narrator
            // if (_configs.SkipNarrator && _regexNarrator.IsMatch(noTagText))
            if (_configs.SkipNarrator && lines.Any(line => line.HasNarrator))
            {
                return smartParagraph.Text;
            }

            return StringUtils.UnbreakLine(smartParagraph.Paragraph.Text);
        }
    }

    public class Line
    {
        private readonly string _line;

        public bool IsDialog { get; private set; }
        public bool HasMood { get; private set; }
        public bool IsClosed { get; private set; }
        public bool HasNarrator { get; private set; }

        public Line(string line)
        {
            _line = line;
            Evaluate(line);
        }

        private void Evaluate(string line)
        {
            // clean the line
            var reader = new LineReader(line);
            var buffer = new char[line.Length];
            var writeTrack = 0;
            const char Eof = '\0';
            var ch = reader.Read();
            while (ch != Eof)
            {
                buffer[writeTrack++] = ch;
                ch = reader.Read();
            }

            var noTagLine = new string(buffer, 0, writeTrack);

            HasMood = CheckMood(noTagLine);
            IsDialog = CheckDialog(noTagLine);
            IsClosed = CheckClosing(noTagLine);
            HasNarrator = CheckNarrator(noTagLine);
        }

        private static bool CheckClosing(string line) => line.Length > 0 && line[line.Length - 1].IsClosed();
        private static bool CheckDialog(string line) => line.Length > 0 && line.StartsWith('-');
        private static bool CheckMood(string line)
        {
            // todo: this need improvement
            return line.Contains('(') || line.Contains('[');
        }
        private static bool CheckNarrator(string line)
        {
            var colonIndex = line.IndexOf(':');
            
            // doesn't have colon or colon is last char
            if (colonIndex < 0 || colonIndex + 1 == line.Length)
            {
                return false;
            }

            var colonAdjacent = line[colonIndex + 1];
            if (char.IsDigit(colonAdjacent))
            {
                return false;
            }

            var narratorCandidate = line.Substring(0, colonIndex);
            
            // ! ? , shouldn't exists in narrator 
            return !"?.,".Any(ch => narratorCandidate.Contains(ch));
        }

        public string Content => _line;

        public override string ToString() => Content;

        private struct LineReader
        {
            private readonly string _line;
            private int _position;

            public LineReader(string line)
            {
                _line = line;
                _position = 0;
            }

            public char Read()
            {
                if (_position < _line.Length)
                {
                    if (!_line[_position].IsTagStart())
                    {
                        return _line[_position++];
                    }

                    var closingPair = _line[_position].ClosingPair();
                    _position = Math.Max(_line.IndexOf(closingPair) + 1, _position);
                }

                return '\0';
            }

            public int GetCurrentPosition() => _position;

            // public char GetCurrent()
            // {
            //     if (_position < 0) return '\0';
            //     return _line[_position++];
            // }
        }
    }

    public class SmartParagraph
    {
        public Paragraph Paragraph { get; }

        // private int _currentIndex = 0;
        private Line[] _lines;

        public IEnumerable<Line> Lines => _lines;

        public bool IsMultiLined => _lines.Length > 1;
        
        public string Text
        {
            set => _lines = ReadLines(value).ToArray();
            get => string.Join(Environment.NewLine, _lines.Select(line => line));
        }

        public SmartParagraph(Paragraph paragraph)
        {
            Paragraph = paragraph;
            _lines = ReadLines(paragraph.Text).ToArray();
        }
        
        private IEnumerable<Line> ReadLines(string text) => text.SplitToLines().Select(line => new Line(line));
    }

    public static class ParagraphExtensions
    {
        public static bool IsUnbreakable(this SmartParagraph paragraph, int maxSingleLineLength)
        {
            if (!paragraph.IsMultiLined)
            {
                return false;
            }

            if (paragraph.Lines.Any(line => line.IsDialog))
            {
                return false;
            }

            if (paragraph.Lines.Any(line => line.Content.Length >= maxSingleLineLength))
            {
                return false;
            }

            return true;
        }

        public static IEnumerable<SmartParagraph> ToSmartParagraphs(this IEnumerable<Paragraph> paragraphs) =>
            paragraphs.Select(p => new SmartParagraph(p));
    }

    public static class CharExtensions
    {
        public static bool IsTagStart(this char ch) => ch == '<' || ch == '{';
        public static char ClosingPair(this char ch) => ch == '<' ? '>' : '}';
        public static bool IsClosed(this char ch) => ch == '.' || ch == '?' || ch == '!' || ch == ']' || ch == ')';
    }
}
