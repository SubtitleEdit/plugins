using System;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class MusicArtist : Artist
    {
        private readonly Palette _palette;

        public MusicArtist(Palette palette)
        {
            _palette = palette;
        }

        public override void Paint(Subtitle subtitle)
        {
            // TODO: Handle when text contains both mood and music
            // e.g:  ♪ Foobar (drum) ♪
            foreach (var p in subtitle.Paragraphs)
            {
                var text = p.Text;
                // skip any formatting tag e.g: html or assa
                var i = 0;
                var len = text.Length;

                while (i < len && (text[i] == '<' || text[i] == '{'))
                {
                    i = Math.Max(text.IndexOf(GetClosingPair(text[i]), i + 1) + 1, 0);
                }
                
                // note if full text is already coloured do not change the color
                // this might happen when there is both mood and music symbols in same text
                if (!IsAlreadyColoured(text) && IsMusicText(ToSkipText(text, i)))
                {
                    p.Text = ApplyColor(_palette.Color, p.Text);
                }
            }
        }

        private static bool IsAlreadyColoured(string text)
        {
            var len = text.Length;
            
            //<font color="#ffff00">
            if (len < 15) return false;

            var closeIdx = text.IndexOf('>', 1);
            if (closeIdx < 0) return false;
            var tag = text.Substring(0, closeIdx + 1);
            return tag.Contains("color=", StringComparison.OrdinalIgnoreCase);
        }

        private static char GetClosingPair(char ch) => ch == '<' ? '>' : '}';

        private static SkipText ToSkipText(string text, int index) => new SkipText(text, index);

        private static bool IsMusicText(SkipText skipText)
        {
            if (!skipText.IsReadable()) return false;
            var ch = skipText.Read();
            return ch == '♫' || ch == '♪';
        }
        
        private struct SkipText
        {
            public string Text { get; }
            public int Index { get; private set; }

            public SkipText(string text, int index)
            {
                Text = text;
                Index = index;
            }

            public bool IsReadable() => Index < Text.Length;

            public char Read()
            {
                if (Index < Text.Length)
                {
                    return Text[Index];
                }

                return '\0';
            }
            
            public char ReadAdvance()
            {
                if (Index < Text.Length)
                {
                    return Text[Index++];
                }

                return '\0';
            }
        }
    }
}