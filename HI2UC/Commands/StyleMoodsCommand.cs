using System;
using System.Collections.Generic;
using Nikse.SubtitleEdit.PluginLogic.Strategies;

namespace Nikse.SubtitleEdit.PluginLogic.Commands
{
    public class StyleMoodsCommand : ICommand
    {
        private static readonly char[] Symbols = {'.', '!', '?', ')', ']'};
        private static readonly char[] HiChars = {'(', '['};

        private IStrategy Strategy { get; }

        public StyleMoodsCommand(IStrategy strategy)
        {
            Strategy = strategy;
        }

        public void Convert(IList<Paragraph> paragraphs, IController controller)
        {
            foreach (var paragraph in paragraphs)
            {
                string text = paragraph.Text;

                // doesn't have balanced brackets. O(2n)
                if (!(HasBalancedParentheses(text) && HasBalancedBrackets(text)))
                {
                    controller.AddResult(text, text, "Line contains unbalanced []/()", paragraph);
                }

                string output = MoodsToUppercase(text);

                if (!output.Equals(paragraph.Text, StringComparison.Ordinal))
                {
                    controller.AddResult(paragraph.Text, output, "Moods", paragraph);
                }
            }
        }

        public string MoodsToUppercase(string text)
        {
            // <font color="#ff00ff">(SIGHS)</font>
            // Remove invalid tags.
            text = text.Replace("()", string.Empty);
            text = text.Replace("()", string.Empty);
            text = text.Replace("( )", string.Empty);
            text = text.Replace("[ ]", string.Empty);

            if (!IsQualifedMoods(text))
            {
                return text;
            }

            int idx = text.IndexOfAny(HiChars);
            char openChar = text[idx];
            char closeChar = openChar == '(' ? ')' : ']';
            do
            {
                int endIdx = text.IndexOf(closeChar, idx + 1); // ] or )
                // There most be at lease one chars inside brackets.
                if (endIdx < idx + 2)
                {
                    break;
                }

                string moodText = text.Substring(idx, endIdx - idx + 1);
                text = text.Remove(idx, moodText.Length);
                if (string.IsNullOrWhiteSpace(HtmlUtils.RemoveTags(moodText, true)))
                {
                    idx = text.IndexOf(openChar, idx); // if invalid take out the tag!
                }
                else
                {
                    string textBetween = moodText.Substring(1, moodText.Length - 2);
                    text = text.Insert(idx, $"{moodText[0]}{Strategy.Execute(textBetween)}{moodText[moodText.Length-1]}");
                    idx = text.IndexOf(openChar, endIdx + 1); // ( or [
                }
            } while (idx >= 0);

            return text;
        }


        public string ConvertMoods(string text)
        {
            int j = 0;
            for (int i = text.Length - 1; i > 0; i--)
            {
                char ch = text[i];
                if (ch == ')')
                {
                    j = i;
                }
                else if (ch == '(' && j > i)
                {
                    string textInRange = Strategy.Execute(text.Substring(i + 1, j - i - 1));
                    text = text.Remove(i, j + 1 - i).Insert(i, textInRange);
                    j = -1;
                }
            }

            return text;
        }

        public bool HasBalancedParentheses(string input)
        {
            int count = 0;
            for (int i = input.Length - 1; i >= 0; i--)
            {
                char ch = input[i];
                if (ch == ')')
                {
                    count++;
                }
                else if (ch == '(')
                {
                    count--;
                }

                // even if you check to the end there won't be enough to balance
                if (i - count < 0)
                {
                    Console.WriteLine("too much close");
                    return false;
                }
            }

            if (count > 0)
            {
                Console.WriteLine("too much close");
            }
            else if (count < 0)
            {
                Console.WriteLine("too much open");
            }

            return count == 0;
        }

        public bool HasBalancedBrackets(string input)
        {
            int count = 0;
            for (int i = input.Length - 1; i >= 0; i--)
            {
                char ch = input[i];
                if (ch == ']')
                {
                    count++;
                }
                else if (ch == '[')
                {
                    count--;
                }

                // even if you check to the end there won't be enough to balance
                if (i - count < 0)
                {
                    Console.WriteLine("too much close");
                    return false;
                }
            }

            if (count > 0)
            {
                Console.WriteLine("too much close");
            }
            else if (count < 0)
            {
                Console.WriteLine("too much open");
            }

            return count == 0;
        }


        private static bool IsQualifedMoods(string text)
        {
            if (text == null)
            {
                return false;
            }

            int idx = text.IndexOfAny(HiChars);
            return (idx >= 0 && idx + 1 < text.Length);
        }
    }
}