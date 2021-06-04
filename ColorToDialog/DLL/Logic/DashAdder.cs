using System;
using System.Drawing;
using System.Text;

namespace ColorToDialog.Logic
{
    public static class DashAdder
    {
        public static string GetFixedText(string text, string dash)
        {
            var result = new StringBuilder();
            var i = 0;
            var currentColor = Color.White;
            var noColorOn = false;
            var waitingForFirstNoColorNoWhitespaceChar = true;
            while (i < text.Length)
            {
                var s = text.Substring(i);
                Color lastColor;
                if (s.StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
                {
                    noColorOn = false;
                    lastColor = currentColor;
                    currentColor = GetColorFromFontString(s, Color.White);
                    if (i == 0)
                    {
                        lastColor = currentColor;
                    }

                    if (result.Length > 0 && (currentColor.R != lastColor.R || currentColor.G != lastColor.G || currentColor.B != lastColor.B))
                    {
                        var newText = AddStartDash(result.ToString(), dash);
                        result = new StringBuilder(newText.TrimEnd() + Environment.NewLine);
                        result.Append(dash);
                    }
                    var len = s.IndexOf('>');
                    if (len < 0)
                    {
                        return text;
                    }

                    i += len + 1;
                }
                else if (s.StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                {
                    i += "</font>".Length;
                    noColorOn = true;
                }
                else
                {
                    var ch = text[i];
                    if (noColorOn && ch.ToString().Trim().Length > 0 && waitingForFirstNoColorNoWhitespaceChar)
                    {
                        lastColor = currentColor;
                        currentColor = Color.White;
                        if (result.Length > 0 && (currentColor.R != lastColor.R || currentColor.G != lastColor.G || currentColor.B != lastColor.B))
                        {
                            var newText = AddStartDash(result.ToString(), dash);
                            result = new StringBuilder(newText.TrimEnd() + Environment.NewLine);
                            if (ch != '-')
                            {
                                result.Append(dash);
                            }
                        }
                        waitingForFirstNoColorNoWhitespaceChar = false;
                    }

                    result.Append(ch);
                    i++;
                }
            }

            var resultString = result.ToString().Trim();
            while (resultString.Contains(Environment.NewLine + Environment.NewLine))
            {
                resultString = resultString.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            }

            while (resultString.Contains(Environment.NewLine + "-  "))
            {
                resultString = resultString.Replace(Environment.NewLine + "-  ", Environment.NewLine + "- ");
            }

            return resultString;
        }

        private static string AddStartDash(string text, string dash)
        {
            var s = text.Trim();
            if (!s.Contains(Environment.NewLine))
            {
                if (s.StartsWith("-"))
                {
                    return s;
                }

                return dash + s;
            }

            var idx = s.LastIndexOf(Environment.NewLine, StringComparison.Ordinal);
            if (idx < 0)
            {
                return s;
            }

            var before = s.Substring(0, idx);
            var after = s.Substring(idx + Environment.NewLine.Length);
            if (before.TrimEnd('"').EndsWith(".") || before.TrimEnd('"').EndsWith("!") || before.TrimEnd('"').EndsWith("?"))
            {
                if (after.StartsWith("-"))
                {
                    return s;
                }

                return before + Environment.NewLine + dash + after;
            }

            if (!s.StartsWith('-'))
            {
                return dash + s;
            }

            return s;
        }

        private static Color GetColorFromFontString(string text, Color defaultColor)
        {
            var s = text.TrimEnd();
            var start = s.IndexOf("<font ", StringComparison.OrdinalIgnoreCase);
            var endFont = s.IndexOf("</font>", StringComparison.OrdinalIgnoreCase);
            if (endFont > 0)
            {
                s = s.Substring(0, endFont + "</font>".Length);
            }

            if (start < 0 || !s.EndsWith("</font>", StringComparison.OrdinalIgnoreCase))
            {
                return defaultColor;
            }

            s = s.Replace("color  =", "color=");
            s = s.Replace("color =", "color=");
            s = s.Replace("color=", "color= ");
            int end = s.IndexOf('>', start);
            if (end > 0)
            {
                var f = s.Substring(start, end - start);
                if (f.Contains(" color=", StringComparison.OrdinalIgnoreCase))
                {
                    var colorStart = f.IndexOf(" color=", StringComparison.OrdinalIgnoreCase);
                    if (s.IndexOf('"', colorStart + " color=".Length + 1) > 0)
                    {
                        end = s.IndexOf('"', colorStart + " color=".Length + 1);
                    }

                    s = s.Substring(colorStart, end - colorStart);
                    s = s.Replace(" color=", string.Empty);
                    s = s.Trim('\'').Trim('"').Trim('\'');
                    try
                    {
                        if (s.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
                        {
                            var arr = s.Remove(0, 4).TrimEnd(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            return Color.FromArgb(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
                        }

                        return ColorTranslator.FromHtml(s.Trim());
                    }
                    catch
                    {
                        return defaultColor;
                    }
                }
            }
            return defaultColor;
        }
    }
}
