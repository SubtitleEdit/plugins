using System;

namespace Nikse.SubtitleEdit.PluginLogic;

public static class StringExtensions
{
    public static bool HasColor(this string input)
    {
        return input?.IndexOf("<font color=", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}