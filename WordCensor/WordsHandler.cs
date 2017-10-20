using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    class WordsHandler
    {
        private static readonly Random _random = new Random();
        private static readonly char[] _grawlixChars = { '@', '#', '!', '?', '$', '%', '&' };
        private readonly WordsHandlerConfigs _configs;
        private static readonly Regex _regexNonWord = new Regex("\\W", RegexOptions.Compiled);

        public WordsHandler(WordsHandlerConfigs configs)
        {
            _configs = configs;
        }

        public void CleanUpSubtitle(Subtitle subtitle)
        {
            foreach (var p in subtitle.Paragraphs)
            {
                p.Text = ProcessText(p.Text);
            }
        }

        public string ProcessText(string text)
        {
            foreach (var word in WordsHelper.GetWords())
            {
                int idx = text.IndexOf(word, StringComparison.OrdinalIgnoreCase);
                while (idx >= 0)
                {
                    bool startsOkay = idx == 0 || _regexNonWord.IsMatch(text[idx - 1].ToString());
                    bool allOkay = startsOkay && (idx + word.Length == text.Length || _regexNonWord.IsMatch(text[idx + word.Length].ToString()));
                    if (allOkay)
                    {
                        string procWord = ProcessWord(word);
                        if (_configs.ColorRed)
                        {
                            procWord = WordsHelper.ColorWordRed(procWord);
                        }
                        // take out old word
                        text = text.Remove(idx, word.Length);
                        // insert new word/colored word.
                        text = text.Insert(idx, procWord);
                    }
                    // keep looking for more words in same text.
                    idx = text.IndexOf(word, idx + word.Length, StringComparison.OrdinalIgnoreCase);
                }
            }
            return text;
        }

        private static string ProcessWord(string curseWord)
        {
            if (curseWord.Length == 1)
            {
                return curseWord;
            }
            int len = (int)Math.Ceiling(curseWord.Length * .50d);
            string charsToCensor = curseWord.Substring(0, len);
            var sb = new StringBuilder(len);
            foreach (char ch in charsToCensor)
            {
                char grawlix = _grawlixChars[_random.Next(0, _grawlixChars.Length)];
                sb.Append(grawlix);
            }
            // grawlixed word
            return sb + curseWord.Substring(len);
        }
    }
}
