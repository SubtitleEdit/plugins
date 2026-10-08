namespace SubtitleEdit.Plugins.PersianErrors;

/// <summary>
/// UI metadata for a rule group: icon, color, the Persian title from the original plugin's help
/// images, a one-line description and an example input (its output is computed by the rules).
/// </summary>
public sealed record GroupInfo(string Name, string PersianName, string Description, string Icon, string Color, string Example)
{
    public static readonly IReadOnlyList<GroupInfo> All = new[]
    {
        new GroupInfo("Fix Unicode Control Char", "نویسه\u200Cهای کنترلی یونیکد",
            "Tidies right-to-left embedding marks (U+202B) for subtitles that use them: removes doubled marks and stray spaces after them.",
            "mdi-format-pilcrow-arrow-left", "#7C5CFF", "\u202B\u202B سلام"),
        new GroupInfo("Change Arabic Chars to Persian", "تبدیل حروف عربی به فارسی",
            "Replaces Arabic letters with their Persian forms, such as ي → ی and ك → ک.",
            "mdi-abjad-arabic", "#0EA5A4", "املاك شدني"),
        new GroupInfo("Remove Unneeded Spaces", "حذف خط فاصله نادرست",
            "Removes double spaces and spaces before punctuation or at line edges.",
            "mdi-arrow-collapse-horizontal", "#3B82F6", "گفت :  باشه"),
        new GroupInfo("Add Missing Spaces", "نبود خط فاصله",
            "Adds the missing space after punctuation such as the Persian comma.",
            "mdi-arrow-expand-horizontal", "#06B6D4", "سلام،عزیزم"),
        new GroupInfo("Fix Dialog Hyphen", "دیالوگ\u200Cها",
            "Puts dialog dashes where right-to-left players expect them.",
            "mdi-forum-outline", "#F59E0B", "-سلام، خوبی؟\n-ممنون"),
        new GroupInfo("Fix Wrong Chars", "کاراکترهای اشتباه",
            "Fixes wrong punctuation: doubled commas, Latin commas and question marks.",
            "mdi-spellcheck", "#EF4444", "سلام,, خوبی?"),
        new GroupInfo("Fix Misplaced Chars", "مکان اشتباه",
            "Moves end punctuation to the start of the text, for players that do not handle right-to-left text.",
            "mdi-swap-horizontal", "#EC4899", "سلام."),
        new GroupInfo("Fix Abbreviations", "کلمات اختصاری",
            "Writes abbreviations with dots, such as اف بی آی → اف.بی.آی.",
            "mdi-format-letter-matches", "#8B5CF6", "اف بی آی"),
        new GroupInfo("Space to Invisible Space", "خط فاصله نامرئی",
            "Uses the zero-width non-joiner (نیم\u200Cفاصله) in prefixes and suffixes, such as می\u200Cشود and خانه\u200Cها.",
            "mdi-format-letter-spacing", "#10B981", "میشود خانه ها"),
        new GroupInfo("OCR", "جاگذاری کلمات",
            "Fixes common OCR misreads and misspelled words.",
            "mdi-text-recognition", "#F97316", "سوسابقه"),
        new GroupInfo("Remove Leading Dots", "حذف سه نقطه از ابتدای جمله",
            "Removes the three dots (...) that mark a continued sentence.",
            "mdi-format-letter-starts-with", "#64748B", "در ادامه..."),
        new GroupInfo("Remove Dot from the End of Line", "حذف نقطه از انتهای جمله",
            "Removes the period that ends a line.",
            "mdi-format-letter-ends-with", "#A16207", ".می\u200Cشود"),
    };

    private static readonly string[] FallbackColors = { "#7C5CFF", "#0EA5A4", "#3B82F6", "#F59E0B", "#EC4899", "#10B981" };

    public static GroupInfo For(string name, int index) =>
        All.FirstOrDefault(g => g.Name == name)
        ?? new GroupInfo(name, string.Empty, string.Empty, "mdi-auto-fix", FallbackColors[index % FallbackColors.Length], string.Empty);
}
