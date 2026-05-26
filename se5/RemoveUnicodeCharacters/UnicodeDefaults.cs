using System.Collections.Generic;

namespace SubtitleEdit.Plugins.RemoveUnicodeCharacters;

/// <summary>
/// Built-in suggested replacements for non-ANSI characters that have a near-lossless
/// or universally understood ASCII equivalent. Used as the seed value for the
/// "Replace with" column when the user has no persisted setting for a character.
/// Debatable cases (bullets, arrows, math, trademark, music accidentals) are
/// deliberately omitted - the UI shows them blank so the user picks per row.
/// </summary>
internal static class UnicodeDefaults
{
    public static readonly Dictionary<char, string> Map = new()
    {
        // Smart quotes -> straight ASCII quotes
        ['‘'] = "'",   // ' left single quotation mark
        ['’'] = "'",   // ' right single quotation mark
        ['‚'] = ",",   // ‚ single low-9 quotation mark
        ['‛'] = "'",   // ‛ single high-reversed-9
        ['“'] = "\"",  // " left double quotation mark
        ['”'] = "\"",  // " right double quotation mark
        ['„'] = "\"",  // „ double low-9 quotation mark
        ['‟'] = "\"",  // ‟ double high-reversed-9

        // Hyphens / dashes -> hyphen-minus
        ['‐'] = "-",   // ‐ hyphen
        ['‑'] = "-",   // ‑ non-breaking hyphen
        ['‒'] = "-",   // ‒ figure dash
        ['–'] = "-",   // – en dash
        ['—'] = "-",   // — em dash
        ['―'] = "-",   // ― horizontal bar
        ['−'] = "-",   // − minus sign

        // Ellipses / dot leaders
        ['․'] = ".",   // ․ one-dot leader
        ['‥'] = "..",  // ‥ two-dot leader
        ['…'] = "...", // … horizontal ellipsis

        // Music notes (note heads, not accidentals)
        ['♩'] = "#",   // ♩ quarter note
        ['♪'] = "#",   // ♪ eighth note
        ['♫'] = "#",   // ♫ beamed eighth notes
        ['♬'] = "#",   // ♬ beamed sixteenth notes

        // Wide / narrow space variants -> regular space
        [' '] = " ",   // en quad
        [' '] = " ",   // em quad
        [' '] = " ",   // en space
        [' '] = " ",   // em space
        [' '] = " ",   // three-per-em space
        [' '] = " ",   // four-per-em space
        [' '] = " ",   // six-per-em space
        [' '] = " ",   // figure space
        [' '] = " ",   // punctuation space
        [' '] = " ",   // thin space
        [' '] = " ",   // hair space
        [' '] = " ",   // narrow no-break space
        [' '] = " ",   // medium mathematical space
        ['　'] = " ",   // ideographic space

        // Zero-width and joiner controls -> remove
        ['​'] = "",    // zero-width space
        ['‌'] = "",    // zero-width non-joiner
        ['‍'] = "",    // zero-width joiner
        ['⁠'] = "",    // word joiner
        ['﻿'] = "",    // zero-width no-break space / BOM
    };
}
