using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal sealed class GrammarRulesWindow : Window
{
    private readonly Dictionary<string, CheckBox> _groups = new(StringComparer.Ordinal);
    private readonly Dictionary<string, CheckBox> _rules = new(StringComparer.Ordinal);
    private readonly TextBox _extra;
    internal string DisabledRules => GrammarRuleSelection.Validate(string.Join(",",
        _rules.Where(r => r.Value.IsChecked != true).Select(r => r.Key).Append(_extra.Text ?? "")));
    internal string DisabledGroups => string.Join(",", _groups.Where(g => g.Value.IsChecked != true).Select(g => g.Key).Order(StringComparer.Ordinal));

    internal GrammarRulesWindow(ConversionOptions options, IEnumerable<string> observed)
    {
        Title = "LanguageTool rules";
        Width = 780;
        Height = 650;
        MinWidth = 540;
        MinHeight = 400;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var scope = new NameScope();
        NameScope.SetNameScope(this, scope);
        _extra = new TextBox { Name = "ExtraGrammarDisabledRules", AcceptsReturn = true, MinHeight = 65, Watermark = "RULE_ID, ANOTHER_RULE_ID" };
        scope.Register(_extra.Name, _extra);
        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(new TextBlock
        {
            Text = "Choose which LanguageTool alerts and corrections to use. Unchecked groups override individual rules. " +
                   "These choices are saved for future files and Batch convert. They do not disable the plugin's own conversion rules.",
            TextWrapping = TextWrapping.Wrap,
        });
        foreach (var group in GrammarRuleSelection.Groups)
        {
            var box = new CheckBox
            {
                Name = "GrammarGroup_" + group.Id, Content = group.Label,
                IsChecked = !GrammarRuleSelection.Ids(options.GrammarDisabledGroups).Contains(group.Id, StringComparer.Ordinal),
            };
            _groups.Add(group.Id, box);
            scope.Register(box.Name, box);
            content.Children.Add(box);
        }
        var grammarOnly = new Button { Name = "GrammarOnlyPreset", Content = "Grammar only (fewer alerts)" };
        scope.Register(grammarOnly.Name, grammarOnly);
        grammarOnly.Click += (_, _) =>
        {
            foreach (var group in _groups) group.Value.IsChecked = group.Key == "grammar";
        };
        var all = new Button { Name = "AllGrammarRules", Content = "Enable all" };
        scope.Register(all.Name, all);
        all.Click += (_, _) =>
        {
            foreach (var box in _groups.Values.Concat(_rules.Values)) box.IsChecked = true;
            _extra.Text = "";
        };
        content.Children.Add(new WrapPanel { Children = { grammarOnly, all } });
        content.Children.Add(new TextBlock
        {
            Text = "Individual rules: current findings, saved exclusions and common subtitle alerts",
            FontWeight = FontWeight.SemiBold, TextWrapping = TextWrapping.Wrap,
        });
        var common = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["MORFOLOGIK_RULE_PT_PT"] = "Spelling / unknown names",
            ["DASH_ENUMERATION_SPACE_RULE"] = "Space after dialogue dash",
            ["PT_WORDINESS_REPLACE_POR_CAUSA_DA"] = "Style: por causa da",
        };
        var disabled = GrammarRuleSelection.Ids(options.GrammarDisabledRules);
        foreach (var id in observed.Concat(common.Keys).Concat(disabled).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            var box = new CheckBox
            {
                Name = "GrammarRule_" + id,
                Content = new TextBlock
                {
                    Text = common.TryGetValue(id, out var label) ? $"{label} [{id}]" : id,
                    TextWrapping = TextWrapping.Wrap,
                },
                IsChecked = !disabled.Contains(id, StringComparer.Ordinal),
            };
            _rules.Add(id, box);
            scope.Register(box.Name, box);
            content.Children.Add(box);
        }
        content.Children.Add(new TextBlock
        {
            Text = "Disable additional rule IDs (comma-separated or one per line). Copy an ID from LanguageTool [ID] in a finding. " +
                   "Run checks again after saving to refresh results.",
            TextWrapping = TextWrapping.Wrap,
        });
        content.Children.Add(_extra);
        var error = new TextBlock { Name = "GrammarRulesError", Foreground = Brushes.Red, TextWrapping = TextWrapping.Wrap };
        scope.Register(error.Name, error);
        var save = new Button { Name = "SaveGrammarRules", Content = "Use selected rules", IsDefault = true };
        scope.Register(save.Name, save);
        save.Click += (_, _) =>
        {
            try
            {
                _ = DisabledRules;
                Close(true);
            }
            catch (InvalidDataException ex) { error.Text = ex.Message; }
        };
        var cancel = new Button { Content = "Cancel", IsCancel = true };
        cancel.Click += (_, _) => Close(false);
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8, Margin = new Thickness(0, 10, 0, 0), Children = { cancel, save },
        };
        var panel = new DockPanel { Margin = new Thickness(16), LastChildFill = true };
        DockPanel.SetDock(buttons, Dock.Bottom);
        DockPanel.SetDock(error, Dock.Bottom);
        panel.Children.Add(buttons);
        panel.Children.Add(error);
        panel.Children.Add(new ScrollViewer { Content = content, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled });
        Content = panel;
    }
}
