using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BaiduTranslate.Logic;

namespace BaiduTranslate.Translator
{
    /// <summary>
    /// Baidu translate
    /// </summary>
    public class BaiduTranslator
    {
        private string _translateResult;
        private StringBuilder _log;
        private readonly string _translateUrl;
        private Formatting[] _formattings;
        private readonly WebBrowser _webBrowser;
        public bool PageLoaded { get; private set; }
        private string _sourceLanguage;
        private string _separator;
        private string _token;
        private string _gtk;
        private string _cookie;

        public BaiduTranslator(string source, string target, WebBrowser webBrowser, string separator)
        {
            if (webBrowser == null)
                return;

            _log = new StringBuilder();
            _translateUrl = $"https://fanyi.baidu.com/#{source}/{target}/";
            _separator = separator;
            _webBrowser = webBrowser;
            _webBrowser.Navigated += WebBrowser1_Navigated;
            _webBrowser.Navigate(new Uri("https://fanyi.baidu.com/#en/de/How%20are%20you%3F"));
        }

        private void WebBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            var html = _webBrowser.DocumentText;
            if (html == null || _webBrowser.Document == null)
            {
                return;
            }

            var indexOfToken = html.IndexOf("token:", StringComparison.Ordinal);
            if (indexOfToken < 0)
            {
                return;
            }
            var endIndex = html.IndexOf("'", indexOfToken + 10, StringComparison.Ordinal);
            _token = html.Substring(indexOfToken + 7, endIndex - indexOfToken - 6).Trim('\'');

            var indexOfGtk = html.IndexOf("window.gtk =", StringComparison.Ordinal);
            if (indexOfGtk < 0)
            {
                return;
            }
            endIndex = html.IndexOf("'", indexOfGtk + 14, StringComparison.Ordinal);
            _gtk = html.Substring(indexOfGtk + 13, endIndex - indexOfGtk - 12).Trim('\'');
            _cookie = _webBrowser.Document.Cookie;
            _webBrowser.Navigated += WebBrowser1_Navigated;
            PageLoaded = true;
        }

        public static List<TranslationPair> GetTranslationPairs()
        {
            return new List<TranslationPair>
            {
                new TranslationPair("Arabic", "ara"),
                new TranslationPair("Estonian", "est"),
                new TranslationPair("Bulgarian", "bul"),
                new TranslationPair("Polish", "pl"),
                new TranslationPair("Dansih", "dan"),
                new TranslationPair("German", "de"),
                new TranslationPair("Russian", "ru"),
                new TranslationPair("French", "fra"),
                new TranslationPair("Finnish", "fin"),
                new TranslationPair("Korean", "kor"),
                new TranslationPair("Dutch", "nl"),
                new TranslationPair("Czech", "cs"),
                new TranslationPair("Romanian", "rom"),
                new TranslationPair("Portuguese", "pt"),
                new TranslationPair("Japanese", "jp"),
                new TranslationPair("Swedish", "swe"),
                new TranslationPair("Slovak", "slo"),
                new TranslationPair("Thai", "th"),
                new TranslationPair("wyw", "wyw"),
                new TranslationPair("Spanish", "spa"),
                new TranslationPair("Green", "el"),
                new TranslationPair("Hungarian", "hu"),
                new TranslationPair("Chinese zh", "zh"),
                new TranslationPair("English", "en"),
                new TranslationPair("Italian", "it"),
                new TranslationPair("Vietnamese", "vie"),
                new TranslationPair("Chinese Yue", "yue"),
                new TranslationPair("Chinese cht", "cht"),
            }.OrderBy(p=>p.Name).ToList();
        }

        public string GetName()
        {
            return "Baidu translate";
        }

        public string GetUrl()
        {
            return _translateUrl;
        }

        static long BaiduHash(long r, string o)
        {
            for (var t = 0; t < o.Length - 2; t += 3)
            {
                var a = o[t + 2];
                var a2 = a >= 'a' ? (byte)a - 87 : Convert.ToInt32(a.ToString());
                if ('+' == o[t + 1])
                {
                    var x2 = (int)((uint)r >> a2);
                    a2 = x2;
                }
                else
                {
                    var x2 = (int)r << a2;
                    a2 = x2;
                }
                if ('+' == o[t])
                {
                    r = (int)(r + a2 & 4294967295);
                }
                else
                {
                    r = r ^ a2;
                }
            }
            return r;
        }

        public string BaiduSign(string text, string gtk)
        {
            var regex = new Regex(@"[uD800-uDBFF][uDC00-uDFFF]");
            var matches = regex.Matches(text);
            if (matches.Count >= 0) //TODO...
            {
                var t = text.Length;
                if (t > 30)
                {
                    text = text.Substring(0, 10) + text.Substring(t / 2 - 5, 10) + text.Substring(text.Length - 10, 10);
                }
            }
            else
            {
                _log.AppendLine("In unsupported if-statement with text: " + text);
                //for (FIXME_VAR_TYPE e = r.split(/[uD800 - uDBFF][uDC00 - uDFFF] /), C = 0, h = e.length, f = []; h > C; C++)
                //    "" !== e[C] && f.push.apply(f, a(e[C].split(""))),
                //C !== h - 1 && f.push(o[C]);
                //FIXME_VAR_TYPE g = f.length;
                //g > 30 && (r = f.slice(0, 10).join("") + f.slice(Math.floor(g / 2) - 5, Math.floor(g / 2) + 5).join("") + f.slice(-10).join(""))
            }

            var u = gtk;
            var d = u.Split('.');
            var m = long.Parse(d[0]);
            var s = long.Parse(d[1]);
            int c = 0;
            var S = new List<int>();
            for (int v = 0; v < text.Length; v++)
            {
                var A = text[v];
                if (128 > A)
                {
                    S.Add(A);
                }
                else
                {
                    _log.AppendLine("A <= 128: " + A + Environment.NewLine + "from text: " + text);
                }
            }

            var p = m;
            var F = "+-a^+6";
            var D = "+-3^+b+-f";
            for (int b = 0; b < S.Count; b++)
            {
                p += S[b];
                p = BaiduHash(p, F);
            }
            p = BaiduHash(p, D);
            p ^= s;
            if (0 > p)
            {
                p = (2147483647 & p) + 2147483648;
            }
            p %= 1000000;
            var result = p + "." + (p ^ m);
            return result;
        }

        private string _inputText = null;
        public void Translate(string sourceLanguage, string targetLanguage, List<Paragraph> paragraphs, StringBuilder log)
        {
            for (int i = 0; i < 10; i++)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }

            _translateResult = null;
            _log = log;
            var input = new StringBuilder();
            _formattings = new Formatting[paragraphs.Count];
            for (var index = 0; index < paragraphs.Count; index++)
            {
                var p = paragraphs[index];
                var f = new Formatting();
                _formattings[index] = f;
                if (input.Length > 0)
                {
                    input.Append(Environment.NewLine + _separator + Environment.NewLine);
                }

                var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text, sourceLanguage), sourceLanguage);
                while (text.Contains(Environment.NewLine + Environment.NewLine))
                {
                    text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                }

                text = text.Replace(Environment.NewLine, "\n");
                if (string.IsNullOrWhiteSpace(text))
                {
                    text = "_";
                }

                input.Append(text);
            }

            _sourceLanguage = sourceLanguage;
            _inputText = input.ToString().Trim();
        }


        public List<string> GetTranslationResult(string targetLanguage, List<Paragraph> paragraphs)
        {
            try
            {
                using (var client = new HttpClient(new HttpClientHandler { UseCookies = false }))
                {
                    var signHash = BaiduSign(_inputText, _gtk);
                    var dict = new Dictionary<string, string>
                    {
                        {"from", _sourceLanguage},
                        {"to", targetLanguage},
                        {"query", _inputText},
                        {"simple_means_flag", "3"},
                        {"sign", signHash},
                        {"token", _token}
                    };
                    var req = new HttpRequestMessage(HttpMethod.Post, "https://fanyi.baidu.com/v2transapi")
                    {
                        Content = new FormUrlEncodedContent(dict)
                    };
                    req.Headers.TryAddWithoutValidation("Cookie", _cookie);
                    var response = client.SendAsync(req).Result;
                    var translateResult = response.Content.ReadAsStringAsync().Result;
                    _translateResult = translateResult;
                }

                var sb = new StringBuilder();
                var parser = new JsonParser();
                var x = (Dictionary<string, object>)parser.Parse(_translateResult);
                foreach (var elementUnknown in x.Values)
                {
                    if (elementUnknown is Dictionary<string, object>)
                    {
                        var element = (Dictionary<string, object>)elementUnknown;
                        foreach (var element2 in element)
                        {
                            if (element2.Key == "data")
                            {
                                if (element2.Value is List<object> objects)
                                {
                                    foreach (var dataItem in objects)
                                    {
                                        if (dataItem is Dictionary<string, object> y)
                                        {
                                            foreach (var dataItemElement in y)
                                            {
                                                if (dataItemElement.Key == "dst")
                                                {
                                                    sb.AppendLine(dataItemElement.Value.ToString());
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }

                var list = MakeList(sb.ToString(), targetLanguage, _formattings);
                if (list.Count > paragraphs.Count)
                {
                    return list.Where(p => !string.IsNullOrEmpty(p)).ToList();
                }
                if (list.Count < paragraphs.Count)
                {
                    var splitList = SplitMergedLines(list, paragraphs);
                    if (splitList.Count == paragraphs.Count)
                    {
                        return splitList;
                    }
                }

                return list;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private List<string> MakeList(string input, string targetLanguage, Formatting[] formattings)
        {
            var res = input.Replace("<br/>", Environment.NewLine);
            res = res.Replace("<br />", Environment.NewLine);
            res = res.Replace("<br>", Environment.NewLine);
            var lines = new List<string>();

            var sb = new StringBuilder();
            foreach (var l in res.SplitToLines())
            {
                if (l.Trim() == _separator)
                {
                    if (sb.Length > 0)
                    {
                        lines.Add(sb.ToString().Trim());
                    }

                    sb.Clear();
                }
                else
                {
                    sb.AppendLine(l);
                }
            }
            if (sb.Length > 0)
            {
                lines.Add(sb.ToString().Trim());
            }

            var resultList = new List<string>();
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index].Trim();
                var s = WebUtility.HtmlDecode(line).Replace("+", " ").Replace("  ", " ").Replace(" , ", ", ");
                s = s.Replace("<I>", "<i>");
                s = s.Replace("<I >", "<i>");
                s = s.Replace("< I >", "<i>");
                s = s.Replace("< i >", "<i>");
                s = s.Replace("</I>", "</i>");
                s = s.Replace("</I >", "</i>");
                s = s.Replace("</ I>", "</i>");
                s = s.Replace("</ i>", "</i>");
                s = s.Replace("< / i>", "</i>");
                s = string.Join(Environment.NewLine, s.SplitToLines());
                s = TranslationHelper.PostTranslate(s, targetLanguage);
                s = s.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
                s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
                s = s.Replace(" " + Environment.NewLine, Environment.NewLine);
                s = s.Replace(" " + Environment.NewLine, Environment.NewLine).Trim();
                if (formattings.Length > index)
                {
                    s = formattings[index].ReAddFormatting(s);
                }

                resultList.Add(s.Replace("  ", " "));
            }
            return resultList;
        }

        private static List<string> SplitMergedLines(List<string> input, List<Paragraph> paragraphs)
        {
            var hits = 0;
            var results = new List<string>();
            for (var index = 0; index < input.Count; index++)
            {
                var line = input[index];
                var text = paragraphs[index].Text;
                var badPoints = 0;
                if (text.StartsWith("[") && !line.StartsWith("["))
                {
                    badPoints++;
                }

                if (text.StartsWith("-") && !line.StartsWith("-"))
                {
                    badPoints++;
                }

                if (text.Length > 0 && char.IsUpper(text[0]) && line.Length > 0 && !char.IsUpper(line[0]))
                {
                    badPoints++;
                }

                if (text.EndsWith(".") && !line.EndsWith("."))
                {
                    badPoints++;
                }

                if (text.EndsWith("!") && !line.EndsWith("!"))
                {
                    badPoints++;
                }

                if (text.EndsWith("?") && !line.EndsWith("?"))
                {
                    badPoints++;
                }

                if (text.EndsWith(",") && !line.EndsWith(","))
                {
                    badPoints++;
                }

                if (text.EndsWith(":") && !line.EndsWith(":"))
                {
                    badPoints++;
                }

                var added = false;
                if (badPoints > 0 && hits + input.Count < paragraphs.Count)
                {
                    var percent = line.Length * 100.0 / text.Length;
                    if (percent > 150)
                    {
                        var temp = Utilities.AutoBreakLine(line).SplitToLines();
                        if (temp.Length == 2)
                        {
                            hits++;
                            results.Add(temp[0]);
                            results.Add(temp[1]);
                            added = true;
                        }
                    }
                }
                if (!added)
                {
                    results.Add(line);
                }
            }

            if (results.Count == paragraphs.Count)
            {
                return results;
            }

            return input;
        }
    }
}
