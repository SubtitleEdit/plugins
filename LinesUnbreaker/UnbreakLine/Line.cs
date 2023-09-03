using System;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic.UnbreakLine
{
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
            return !"?.,".Any(ch => StringExtensions.Contains(narratorCandidate, (char)ch));
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
                        return ReadAdvance();
                    }

                    var closingPair = _line[_position].ClosingPair();
                    // Note: _line.IndexOf(closingPair) + 1
                    // fail: -1 + 1 = 0
                    // found last char: index + 1 == _line.Length
                    // found: position + 1 (jump tag)
                    var nextPosition = Math.Max(_line.IndexOf(closingPair) + 1, _position);
                    if (nextPosition != _line.Length)
                    {
                        _position = nextPosition;
                    }

                    // return tag start or character after open tag close. e.g: <i>F => F
                    return ReadAdvance();
                }

                return '\0';
            }

            private char ReadAdvance() => _line[_position++];
        }
    }
}