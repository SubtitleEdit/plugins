using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic.UnbreakLine
{
    public struct SmartParagraph
    {
        public Paragraph Paragraph { get; }

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
        // ReSharper disable once IdentifierTypo
        public static bool IsUnbreakable(in this SmartParagraph paragraph, int unbreakShorterThisValue)
        {
            if (!paragraph.IsMultiLined)
            {
                return false;
            }

            return paragraph.Lines.Sum(line => line.Content.Length) < unbreakShorterThisValue;
        }

        public static IEnumerable<SmartParagraph> ToSmartParagraphs(this IEnumerable<Paragraph> paragraphs) =>
            paragraphs.Select(p => new SmartParagraph(p));
    }
}