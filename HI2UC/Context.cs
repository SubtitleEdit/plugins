using Nikse.SubtitleEdit.PluginLogic.Strategies;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    /*
    public class Context
    {
        private IStrategy _strategy;

        public HiConfigs Config { get; private set; }
        private static readonly char[] Symbols = {'.', '!', '?', ')', ']'};
        private static readonly char[] LineCloseChars = {'!', '?', '¿', '¡'};

        private static readonly Regex RegexExtraSpaces = new(@"(?<=[\(\[]) +| +(?=[\)\]])", RegexOptions.Compiled);
        private static readonly Regex RegexFirstChar = new(@"\b\w", RegexOptions.Compiled);
        private readonly Lazy<StringBuilder> _lazySb = new();
        private StringBuilder _sb;

        public Context(HiConfigs config)
        {
            Config = config;
        }

        



        private string Customize(string capturedText)
        {
            var st = new StripableText(capturedText);
            string strippedText = st.StrippedText;
            switch (Config.Style)
            {
                case HiStyle.UpperLowerCase:
                    if (_sb == null)
                    {
                        _sb = _lazySb.Value;
                    }
                    else
                    {
                        _sb.Clear();
                    }

                    var isUpperTime = true;
                    // TODO: Use StringInfo to fix issue with unicode chars?
                    foreach (var myChar in strippedText)
                    {
                        if (!char.IsLetter(myChar))
                        {
                            _sb.Append(myChar);
                        }
                        else
                        {
                            _sb.Append(isUpperTime ? char.ToUpper(myChar) : char.ToLower(myChar));
                            isUpperTime = !isUpperTime;
                        }
                    }

                    strippedText = _sb.ToString();
                    break;
                case HiStyle.TitleCase:
                    // "foobar foobar" to (Foobar Foobar)
                    strippedText = RegexFirstChar.Replace(strippedText.ToLower(),
                        x => x.Value.ToUpper(CultureInfo.CurrentCulture));
                    break;
                case HiStyle.UpperCase:
                    strippedText = strippedText.ToUpper(CultureInfo.CurrentCulture);
                    break;
                case HiStyle.LowerCase:
                    strippedText = strippedText.ToLower(CultureInfo.CurrentCulture);
                    break;
            }

            return st.CombineWithPrePost(strippedText);
        }

        

    }*/
}