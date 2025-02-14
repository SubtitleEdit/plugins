namespace Nikse.SubtitleEdit.PluginLogic.Controls;

/// <summary>
/// A custom implementation of the ListView control with double buffering enabled.
/// </summary>
/// <remarks>
/// This class inherits from ListView and overrides its double-buffering behavior to reduce flickering during frequent updates or additions of items to the control.
/// </remarks>
public class DoubleBufferedListView : ListView
{
    public DoubleBufferedListView()
    {
        // to fix flickering when nearly constant refresh listview when adding items 
        DoubleBuffered = true;
    }
}