namespace Nikse.SubtitleEdit.PluginLogic
{
    public class MoodsArtist : Artist
    {
        private readonly Palette _palette;

        public MoodsArtist(Palette palette)
        {
            _palette = palette;
        }

        public override void Paint(Subtitle subtitle)
        {
            // <font color="#008040">(REWINDS)</font> DOC: Was he slow?
            var paragraphs = subtitle.Paragraphs;
            for (int i = paragraphs.Count - 1; i >= 0; i--)
            {
                Paragraph p = paragraphs[i];

                string text = ProcessText(p.Text, '(');
                text = ProcessText(text, '[');

                if (text.Length != p.Text.Length)
                {
                    p.Text = text;
                }
            }
        }

        private string ProcessText(string text, char openBracket)
        {
            char closeBracket = openBracket == '(' ? ')' : ']';

            int idx = text.IndexOf(openBracket);
            while (idx >= 0)
            {
                var endIdx = text.IndexOf(closeBracket, idx + 1);

                // mood not closed properly
                if (endIdx < idx + 1)
                {
                    break;
                }

                // get mood from text
                string mood = text.Substring(idx, endIdx - idx + 1);

                // colored mood
                string coloredMood = ApplyColor(_palette.Color, mood);

                text = text.Remove(idx, mood.Length);
                text = text.Insert(idx, coloredMood);

                idx = text.IndexOf(openBracket, idx + coloredMood.Length);
            }
            return text;
        }
    }
}
