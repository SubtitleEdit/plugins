using System;

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
            foreach (var p in subtitle.Paragraphs)
            {
                var text = p.Text;
                // skip any formatting tag e.g: html or assa
                var i = 0;
                var len = text.Length;

                // todo: move these into 'SkipText' struct?
                if (i < len && text[i] == '{')
                {
                    i = Math.Max(text.IndexOf('}') + 1, 0);
                }
                while (i < len && text[i] == '<')
                {
                    i = Math.Max(text.IndexOf('>') + 1, 0);
                }

                var skipText = ToSkipText(text, i);
                if (IsMusicText(skipText))
                {
                    p.Text = ApplyColor(_palette.Color, p.Text);
                }
            }
        }

        private static SkipText ToSkipText(string text, int index) => new SkipText(text, index);

        private static bool IsMusicText(SkipText skipText)
        {
            if (skipText.IsReadable()) return false;
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