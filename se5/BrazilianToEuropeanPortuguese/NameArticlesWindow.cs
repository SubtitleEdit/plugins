using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal sealed class NameArticlesWindow : Window
{
    private readonly TextBox _editor;
    private readonly TextBlock _error;
    internal string Value => _editor.Text ?? "";
    internal TextBox Editor => _editor;
    internal TextBlock Error => _error;
    internal Button SaveButton { get; }

    internal NameArticlesWindow(string value)
    {
        Title = "Personal name articles";
        Width = 620;
        Height = 480;
        MinWidth = 480;
        MinHeight = 350;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        _editor = new TextBox
        {
            Name = "NameArticlesEditor",
            Text = value,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        _error = new TextBlock { Name = "NameArticlesError", TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Red };
        var cancel = new Button { Content = "Cancel", MinWidth = 90, IsCancel = true };
        cancel.Click += (_, _) => Close(false);
        var save = new Button { Name = "SaveNameArticles", Content = "Use list", MinWidth = 90, IsDefault = true };
        SaveButton = save;
        save.Click += (_, _) =>
        {
            try
            {
                _ = new PersonalNameArticles(Value);
                Close(true);
            }
            catch (InvalidDataException ex)
            {
                _error.Text = ex.Message;
            }
        };
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Children = { cancel, save },
        };
        var panel = new DockPanel { Margin = new Thickness(16), LastChildFill = true };
        var instructions = new TextBlock
        {
            Text = "One single-word name per line: Jamie = o, Alex = a, or Maria = none.\n" +
                   "Choices override built-in names in supported rules. Remove a line to restore the default. " +
                   "The same choice applies to every file using these settings.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10),
        };
        DockPanel.SetDock(instructions, Dock.Top);
        DockPanel.SetDock(buttons, Dock.Bottom);
        DockPanel.SetDock(_error, Dock.Bottom);
        panel.Children.Add(instructions);
        panel.Children.Add(buttons);
        panel.Children.Add(_error);
        panel.Children.Add(_editor);
        Content = panel;
    }
}
