namespace SubtitleEdit.Plugins.FrenchTypography.Logic;

/// <summary>
/// UI texts in English or French. Subtitle Edit sends its UI language as the name of its
/// translation file ("French", "English", ...); anything that is not French gets English.
/// </summary>
public sealed class Strings
{
    // French puts a no-break space before : ! ? - also in this plugin's own texts.
    private const string S = " ";

    public static Strings For(string? uiLanguage) => IsFrench(uiLanguage) ? French : English;

    public static bool IsFrench(string? uiLanguage)
    {
        var language = (uiLanguage ?? string.Empty).Trim().ToLowerInvariant();
        return language == "fr" || language.StartsWith("fr-", StringComparison.Ordinal) || language.StartsWith("fr_", StringComparison.Ordinal)
            || language.StartsWith("french", StringComparison.Ordinal) || language.StartsWith("fran", StringComparison.Ordinal);
    }

    public required string Title { get; init; }
    public required string SettingsForBatch { get; init; }
    public required string Untitled { get; init; }
    public required string LinesFormat { get; init; }
    public required string SelectedLine { get; init; }
    public required string SelectedLinesFormat { get; init; }
    public required string AllLines { get; init; }
    public required string ScopeTip { get; init; }
    public required string LinesToChangeFormat { get; init; }
    public required string NothingToChange { get; init; }
    public required string CleanText { get; init; }
    public required string UsedForBatch { get; init; }
    public required string LinesCheckedFormat { get; init; }
    public required string SelectedOfFormat { get; init; }
    public required string NothingToChangeMessage { get; init; }
    public required string LineChangedMessage { get; init; }
    public required string LinesChangedMessageFormat { get; init; }
    public required string Failed { get; init; }
    public required string Cancel { get; init; }
    public required string Ok { get; init; }
    public required string SelectAll { get; init; }
    public required string SelectAllTip { get; init; }
    public required string SelectNone { get; init; }
    public required string SelectNoneTip { get; init; }
    public required string Invert { get; init; }
    public required string InvertTip { get; init; }
    public required string ChangeLineTip { get; init; }
    public required string AboutHeading { get; init; }
    public required string AboutText { get; init; }
    public required string Legend { get; init; }
    public required string Variant { get; init; }
    public required string France { get; init; }
    public required string FranceTip { get; init; }
    public required string Canada { get; init; }
    public required string CanadaTip { get; init; }
    public required string Space { get; init; }
    public required string NoBreakSpace { get; init; }
    public required string NoBreakSpaceTip { get; init; }
    public required string NarrowSpace { get; init; }
    public required string NarrowSpaceTip { get; init; }
    public required string Quotes { get; init; }
    public required string QuotesTip { get; init; }
    public required string Ellipsis { get; init; }
    public required string EllipsisTip { get; init; }
    public required string ConfigureHeading { get; init; }
    public required string ConfigureText { get; init; }

    public static readonly Strings English = new()
    {
        Title = "French typography",
        SettingsForBatch = "Settings for Batch convert",
        Untitled = "Untitled",
        LinesFormat = "{0:N0} lines",
        SelectedLine = "Selected line (1)",
        SelectedLinesFormat = "Selected lines ({0:N0})",
        AllLines = "All lines",
        ScopeTip = "Which lines to check",
        LinesToChangeFormat = "{0:N0} lines to change",
        NothingToChange = "Nothing to change",
        CleanText = "The punctuation already follows French typography.",
        UsedForBatch = "Used for every file in Batch convert",
        LinesCheckedFormat = "{0:N0} lines checked",
        SelectedOfFormat = "{0:N0} of {1:N0} lines selected",
        NothingToChangeMessage = "French typography: nothing to change.",
        LineChangedMessage = "French typography: 1 line changed.",
        LinesChangedMessageFormat = "French typography: {0:N0} lines changed.",
        Failed = "French typography failed: ",
        Cancel = "Cancel",
        Ok = "OK",
        SelectAll = "Select all",
        SelectAllTip = "Select every line",
        SelectNone = "None",
        SelectNoneTip = "Clear the selection",
        Invert = "Invert",
        InvertTip = "Invert the selection",
        ChangeLineTip = "Change this line",
        AboutHeading = "French punctuation spacing",
        AboutText = "Adds a no-break space before ; : ! ? and inside « », so the mark never starts a new line. Times like 10:30, web addresses and tags are left alone.",
        Legend = "· marks a no-break space in the preview",
        Variant = "Variant",
        France = "France",
        FranceTip = "France, Belgium and Switzerland: a space before ; : ! ?",
        Canada = "Canada",
        CanadaTip = "Canada (OQLF): a space before : only, none before ; ! ?",
        Space = "Space",
        NoBreakSpace = "No-break",
        NoBreakSpaceTip = "U+00A0 - shown by every font and player",
        NarrowSpace = "Narrow no-break",
        NarrowSpaceTip = "U+202F - the thin space of print typography. Not every font or player shows it.",
        Quotes = "\"text\" → « text »",
        QuotesTip = "Turn straight quotes into guillemets (only when a line has them in pairs)",
        Ellipsis = "... → …",
        EllipsisTip = "Turn three dots into one ellipsis character",
        ConfigureHeading = "Batch convert settings",
        ConfigureText = "Choose the options above and click OK. Batch convert then applies them to every file without showing this window.",
    };

    public static readonly Strings French = new()
    {
        Title = "Typographie française",
        SettingsForBatch = "Réglages pour la conversion par lots",
        Untitled = "Sans titre",
        LinesFormat = "{0:N0} lignes",
        SelectedLine = "Ligne sélectionnée (1)",
        SelectedLinesFormat = "Lignes sélectionnées ({0:N0})",
        AllLines = "Toutes les lignes",
        ScopeTip = "Lignes à vérifier",
        LinesToChangeFormat = "{0:N0} lignes à modifier",
        NothingToChange = "Rien à modifier",
        CleanText = "La ponctuation respecte déjà la typographie française.",
        UsedForBatch = "Utilisé pour chaque fichier de la conversion par lots",
        LinesCheckedFormat = "{0:N0} lignes vérifiées",
        SelectedOfFormat = "{0:N0} lignes cochées sur {1:N0}",
        NothingToChangeMessage = "Typographie française" + S + ": rien à modifier.",
        LineChangedMessage = "Typographie française" + S + ": 1 ligne modifiée.",
        LinesChangedMessageFormat = "Typographie française" + S + ": {0:N0} lignes modifiées.",
        Failed = "Échec de la typographie française" + S + ": ",
        Cancel = "Annuler",
        Ok = "OK",
        SelectAll = "Tout cocher",
        SelectAllTip = "Cocher toutes les lignes",
        SelectNone = "Aucune",
        SelectNoneTip = "Tout décocher",
        Invert = "Inverser",
        InvertTip = "Inverser la sélection",
        ChangeLineTip = "Modifier cette ligne",
        AboutHeading = "Espacement de la ponctuation française",
        AboutText = "Ajoute une espace insécable avant ; : ! ? et à l'intérieur des « », pour que le signe ne passe jamais seul à la ligne. Les heures comme 10:30, les adresses web et les balises ne sont pas modifiées.",
        Legend = "· indique une espace insécable dans l'aperçu",
        Variant = "Variante",
        France = "France",
        FranceTip = "France, Belgique et Suisse" + S + ": espace avant ; : ! ?",
        Canada = "Canada",
        CanadaTip = "Canada (OQLF)" + S + ": espace avant : seulement, aucune avant ; ! ?",
        Space = "Espace",
        NoBreakSpace = "Insécable",
        NoBreakSpaceTip = "U+00A0 - affichée par toutes les polices et tous les lecteurs",
        NarrowSpace = "Fine insécable",
        NarrowSpaceTip = "U+202F - l'espace fine de l'imprimerie. Certaines polices ou certains lecteurs ne l'affichent pas.",
        Quotes = "\"texte\" → « texte »",
        QuotesTip = "Remplacer les guillemets droits par des guillemets français (seulement s'ils vont par paires)",
        Ellipsis = "... → …",
        EllipsisTip = "Remplacer trois points par le caractère points de suspension",
        ConfigureHeading = "Réglages de la conversion par lots",
        ConfigureText = "Choisissez les options ci-dessus puis cliquez sur OK. La conversion par lots les applique ensuite à chaque fichier sans afficher cette fenêtre.",
    };
}
