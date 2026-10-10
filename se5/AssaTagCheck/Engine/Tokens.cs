using System.Text;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.AssaTagCheck;

internal abstract class Token
{
    public bool Removed { get; set; }

    public abstract string Serialize();
}

internal sealed class TextToken : Token
{
    public TextToken(string text) => Text = text;

    public string Text { get; }

    public override string Serialize() => Removed ? string.Empty : Text;
}

internal sealed class NewLineToken : Token
{
    public NewLineToken(string raw) => Raw = raw;

    public string Raw { get; }

    public override string Serialize() => Removed ? string.Empty : Raw;
}

/// <summary>
/// Subtitle Edit keeps \i, \b, \u and simple font tags as HTML-like tags in memory
/// (<c>{\i1}</c> is <c>&lt;i&gt;</c>) and writes them back as override tags on save.
/// </summary>
internal sealed class HtmlToken : Token
{
    public HtmlToken(string raw, string name, bool closing)
    {
        Raw = raw;
        Name = name;
        Closing = closing;
    }

    public string Raw { get; }

    /// <summary>Lower case: i, b, u, s or font.</summary>
    public string Name { get; }

    public bool Closing { get; }

    public override string Serialize() => Removed ? string.Empty : Raw;
}

/// <summary>One override tag inside a {...} block, e.g. <c>\pos(10,20)</c>.</summary>
internal sealed class TagNode
{
    public TagNode(string name, string argument, bool known, bool parenthesisClosed)
    {
        Name = name;
        Argument = argument;
        Known = known;
        ParenthesisClosed = parenthesisClosed;
        Raw = "\\" + name + argument;
    }

    public string Name { get; }

    /// <summary>Everything after the name up to the next tag, e.g. <c>(10,20)</c> or <c>20 </c>.</summary>
    public string Argument { get; }

    public bool Known { get; }
    public bool ParenthesisClosed { get; }
    public string Raw { get; set; }
    public bool Removed { get; set; }
    public TagSpec? Spec => Known ? AssaTags.Spec(Name) : null;

    /// <summary>The argument without surrounding space, and for parenthesis tags without the parentheses.</summary>
    public string Value
    {
        get
        {
            var value = Argument.Trim();
            if (value.StartsWith('('))
            {
                var close = value.LastIndexOf(')');
                value = close > 0 ? value[1..close] : value[1..];
            }

            return value;
        }
    }
}

/// <summary>A {...} block: override tags, or a comment when it holds no backslash.</summary>
internal sealed class BlockToken : Token
{
    public BlockToken(string content, bool closed)
    {
        Content = content;
        Closed = closed;
        var first = content.IndexOf('\\');
        if (first < 0)
        {
            Leading = content;
            return;
        }

        Leading = content[..first];
        Tags = ParseTags(content, first);
    }

    public string Content { get; }
    public bool Closed { get; }

    /// <summary>Text before the first tag - usually nothing, sometimes a comment.</summary>
    public string Leading { get; }

    public List<TagNode> Tags { get; } = new();
    public bool HasTags => Tags.Count > 0;
    public bool IsComment => Tags.Count == 0;
    public IEnumerable<TagNode> LiveTags => Tags.Where(t => !t.Removed);

    public override string Serialize()
    {
        if (Removed)
        {
            return string.Empty;
        }

        if (!HasTags)
        {
            return "{" + Content + (Closed ? "}" : string.Empty);
        }

        var live = LiveTags.ToList();
        if (live.Count == 0 && string.IsNullOrWhiteSpace(Leading) && Closed)
        {
            return string.Empty; // every tag was removed - drop the block
        }

        var sb = new StringBuilder("{");
        sb.Append(Leading);
        foreach (var tag in live)
        {
            sb.Append(tag.Raw);
        }

        if (Closed)
        {
            sb.Append('}');
        }

        return sb.ToString();
    }

    private static List<TagNode> ParseTags(string content, int start)
    {
        var tags = new List<TagNode>();
        var i = start;
        while (i < content.Length)
        {
            // content[i] is a backslash
            var nameStart = i + 1;
            var name = AssaTags.MatchName(content, nameStart);
            var known = name != null;
            if (name == null)
            {
                var end = nameStart;
                while (end < content.Length && content[end] != '\\' && content[end] != '(' && !char.IsWhiteSpace(content[end]))
                {
                    end++;
                }

                name = content[nameStart..end];
            }

            var k = nameStart + name.Length;
            var parenthesisClosed = true;
            if (k < content.Length && content[k] == '(')
            {
                parenthesisClosed = false;
                var depth = 0;
                for (; k < content.Length; k++)
                {
                    if (content[k] == '(')
                    {
                        depth++;
                    }
                    else if (content[k] == ')' && --depth == 0)
                    {
                        k++;
                        parenthesisClosed = true;
                        break;
                    }
                }
            }

            while (k < content.Length && content[k] != '\\')
            {
                k++;
            }

            tags.Add(new TagNode(name, content[(nameStart + name.Length)..k], known, parenthesisClosed));
            i = k;
        }

        return tags;
    }
}

internal static class Tokenizer
{
    private static readonly Regex HtmlTagRegex = new(@"\G<(/?)(i|b|u|s|font)(\s[^<>]*)?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static List<Token> Tokenize(string text)
    {
        var tokens = new List<Token>();
        var plain = new StringBuilder();

        void FlushText()
        {
            if (plain.Length > 0)
            {
                tokens.Add(new TextToken(plain.ToString()));
                plain.Clear();
            }
        }

        var i = 0;
        while (i < text.Length)
        {
            var c = text[i];
            if (c == '\r' || c == '\n')
            {
                FlushText();
                var length = c == '\r' && i + 1 < text.Length && text[i + 1] == '\n' ? 2 : 1;
                tokens.Add(new NewLineToken(text.Substring(i, length)));
                i += length;
                continue;
            }

            if (c == '{')
            {
                FlushText();
                var close = text.IndexOf('}', i + 1);
                var nextOpen = text.IndexOf('{', i + 1);
                if (close >= 0 && (nextOpen < 0 || close < nextOpen))
                {
                    tokens.Add(new BlockToken(text[(i + 1)..close], closed: true));
                    i = close + 1;
                }
                else
                {
                    // Unclosed: runs to the next '{', line break or the end.
                    var end = i + 1;
                    while (end < text.Length && text[end] != '{' && text[end] != '\r' && text[end] != '\n')
                    {
                        end++;
                    }

                    tokens.Add(new BlockToken(text[(i + 1)..end], closed: false));
                    i = end;
                }

                continue;
            }

            if (c == '<')
            {
                var match = HtmlTagRegex.Match(text, i);
                if (match.Success)
                {
                    FlushText();
                    var name = match.Groups[2].Value.ToLowerInvariant();
                    var closing = match.Groups[1].Value == "/";
                    if (name != "font" && match.Groups[3].Success)
                    {
                        // "<i class=...>" is not something Subtitle Edit writes - leave it as text.
                        plain.Append(c);
                        i++;
                        continue;
                    }

                    tokens.Add(new HtmlToken(match.Value, name, closing));
                    i += match.Length;
                    continue;
                }
            }

            plain.Append(c);
            i++;
        }

        FlushText();
        return tokens;
    }

    public static string Serialize(IEnumerable<Token> tokens)
    {
        var sb = new StringBuilder();
        foreach (var token in tokens)
        {
            sb.Append(token.Serialize());
        }

        return sb.ToString();
    }
}
