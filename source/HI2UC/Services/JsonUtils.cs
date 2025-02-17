using System.Text;

namespace Nikse.SubtitleEdit.PluginLogic.Services;

public static class JsonUtils
{
    public static string EscapeJsonString(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var stringBuilder = new StringBuilder();

        foreach (var c in input)
        {
            switch (c)
            {
                case '\"': // Double quote
                    stringBuilder.Append("\\\"");
                    break;
                case '\\': // Backslash
                    stringBuilder.Append("\\\\");
                    break;
                case '\b': // Backspace
                    stringBuilder.Append("\\b");
                    break;
                case '\f': // Form feed
                    stringBuilder.Append("\\f");
                    break;
                case '\n': // New line
                    stringBuilder.Append("\\n");
                    break;
                case '\r': // Carriage return
                    stringBuilder.Append("\\r");
                    break;
                case '\t': // Tab
                    stringBuilder.Append("\\t");
                    break;
                default:
                    // Ensure control characters (0x00-0x1F) are escaped as Unicode.
                    if (char.IsControl(c))
                    {
                        stringBuilder.AppendFormat("\\u{0:X4}", (int)c);
                    }
                    else
                    {
                        stringBuilder.Append(c);
                    }

                    break;
            }
        }

        return stringBuilder.ToString();
    }
}