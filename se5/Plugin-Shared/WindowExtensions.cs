using Avalonia.Controls;
using System;

namespace SubtitleEdit.Plugins.Shared;

public static class WindowExtensions
{
    /// <summary>
    /// Force the window to the front with focus. A plugin process has no parent
    /// window relationship to Subtitle Edit, so on macOS (and sometimes
    /// Windows/Linux) the window can open behind SE without this trick.
    /// Call from <see cref="Window.OnOpened"/>.
    /// </summary>
    public static void BringToForeground(this Window window)
    {
        try
        {
            window.Topmost = true;
            window.Activate();
        }
        finally
        {
            window.Topmost = false;
        }
    }
}
