# Remove Unicode characters (Subtitle Edit 5 plugin)

Detects non-ANSI characters (code point above 255) in the subtitle and lets you
remove or replace each one. Modeled after the original SE4 plugin
[`RemoveUnicodeCharacters`](../../source/RemoveUnicodeCharacters/) but rebuilt
on Avalonia and the shared [`Plugin-Shared`](../Plugin-Shared/) library.

## UI

A preview window with one row per unique character:

- **Checkbox** — include this character in the apply step.
- **Code** — Unicode hex (e.g. `U+266A`).
- **Char** — the glyph itself.
- **Count** — total occurrences in the (selected) lines.
- **Replace with** — editable text used as the replacement. Leave blank to
  simply remove the character.
- **Lines** — comma-separated list of 1-based line numbers where the character
  occurs.

Defaults: `♪` and `♫` map to `#`; everything else is blank (= remove). Custom
replacements are persisted via the SE5 plugin Settings round-trip so they
reappear on the next run.

Toolbar buttons:

- *Select all* / *Select none* toggle every row.
- *Google selected* opens a search for the selected row's hex code.
- *Cancel* exits without changes.
- *Apply* (accent) replaces every checked character with its replacement,
  reports the count to the SE status bar, and registers a "Remove Unicode
  characters" undo entry.

## Build

See `.github/workflows/remove-unicode-characters.yml`.
