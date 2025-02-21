using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.PluginLogic
{
    class WordsHandler
    {
        private static readonly Random Random = new();
        private static readonly char[] GrawlixChars = { '@', '#', '!', '?', '$', '%', '&' };
        private readonly WordsHandlerConfigs _configs;
        private static readonly Regex RegexNonWord = new("\\W", RegexOptions.Compiled);

        public WordsHandler(WordsHandlerConfigs configs)
        {
            _configs = configs;
        }

        public void CleanUpSubtitle(Subtitle subtitle)
        {
            var offensiveWordsSet = new HashSet<string>(WordsHelper.GetWords());
            foreach (var p in subtitle.Paragraphs)
            {
                p.Text = ProcessText(p.Text, offensiveWordsSet);
            }
        }

        public string ProcessText(string text, HashSet<string> ofensiveWords)
        {
            var l = 0;
            for (int r = 0; r < text.Length; r++)
            {
                if (RegexNonWord.IsMatch(text[l].ToString()) && !RegexNonWord.IsMatch(text[r].ToString()))
                {
                    l = r;
                }
                else if (!char.IsLetterOrDigit(text[r]) || RegexNonWord.IsMatch(text[r].ToString()))
                {
                    if (r - l > 1)
                    {
                        string word = text.Substring(l, r - l);
                        if (ofensiveWords.Contains(word.ToLowerInvariant()))
                        {
                            text = text.Remove(l, r - l);
                            var processWord = ProcessWord(word);
                            if (_configs.ColorRed)
                            {
                                processWord = WordsHelper.ColorWordRed(processWord);
                            }

                            text = text.Insert(l, processWord);
                            r = l + processWord.Length;
                        }
                    }

                    l = r + 1;
                }
            }
            
            // foreach (var word in WordsHelper.GetWords())
            // {
            //     int idx = text.IndexOf(word, StringComparison.OrdinalIgnoreCase);
            //     while (idx >= 0)
            //     {
            //         bool startsOkay = idx == 0 || RegexNonWord.IsMatch(text[idx - 1].ToString());
            //         bool allOkay = startsOkay && (idx + word.Length == text.Length || RegexNonWord.IsMatch(text[idx + word.Length].ToString()));
            //         if (allOkay)
            //         {
            //             string procWord = ProcessWord(word);
            //             if (_configs.ColorRed)
            //             {
            //                 procWord = WordsHelper.ColorWordRed(procWord);
            //             }
            //             // take out old word
            //             text = text.Remove(idx, word.Length);
            //             // insert new word/colored word.
            //             text = text.Insert(idx, procWord);
            //         }
            //         // keep looking for more words in same text.
            //         idx = text.IndexOf(word, idx + word.Length, StringComparison.OrdinalIgnoreCase);
            //     }
            // }
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
                char grawlix = GrawlixChars[Random.Next(0, GrawlixChars.Length)];
                sb.Append(grawlix);
            }
            // grawlixed word
            return sb + curseWord.Substring(len);
        }
    }
}
