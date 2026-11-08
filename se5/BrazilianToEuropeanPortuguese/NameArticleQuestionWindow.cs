using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal sealed class NameArticleQuestionWindow : Window
{
    internal string PersonalName { get; }
    internal Button MasculineButton { get; }
    internal Button FeminineButton { get; }
    internal Button NoArticleButton { get; }
    internal Button SkipButton { get; }

    internal NameArticleQuestionWindow(string name)
    {
        PersonalName = name;
        Title = "Article for a personal name";
        Width = 560;
        Height = 270;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        MasculineButton = Choice("Use o", "o");
        FeminineButton = Choice("Use a", "a");
        NoArticleButton = Choice("No article / not a person", "none");
        SkipButton = new Button { Content = "Not now", IsCancel = true };
        SkipButton.Click += (_, _) => Close(null);
        Content = new StackPanel
        {
            Margin = new Thickness(16),
            Spacing = 14,
            Children =
            {
                new TextBlock
                {
                    Text = $"Which article should be used for \"{name}\"?",
                    TextWrapping = TextWrapping.Wrap,
                    FontWeight = FontWeight.SemiBold,
                },
                new TextBlock
                {
                    Text = "A supported conversion needs a name article. Choose o or a only if this is a personal name. " +
                           "The choice goes into your editable list; Apply saves it for later files. " +
                           "Not now skips this name for the current preview.",
                    TextWrapping = TextWrapping.Wrap,
                },
                new WrapPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children = { MasculineButton, FeminineButton, NoArticleButton, SkipButton },
                },
            },
        };
    }

    private Button Choice(string label, string article)
    {
        var button = new Button { Content = label, Margin = new Thickness(0, 0, 8, 6) };
        button.Click += (_, _) => Close(article);
        return button;
    }
}
