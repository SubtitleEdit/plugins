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

## Batch convert

Remove Unicode characters supports Subtitle Edit's **Batch convert** (Tools → Batch
convert → Run plugin; needs a Subtitle Edit version with plugin batch support).

The window decides per character, which a batch run cannot ask, so a batch run only
touches the characters in a **batch list**; every other character is left alone.

- **Configure...** opens the window without a subtitle and shows the batch list.
  Add characters by pasting them or typing codes such as `U+200B`, set the
  replacement (blank = remove), and uncheck rows to drop them. *Also remove every
  other non-ANSI character* makes the batch remove everything above code point
  255 that is not in the list (the SE4-style "remove all").
- Until the batch list has been configured it holds the defaults (`♪`, `♫` → `#`)
  plus every non-blank replacement you typed in the window. Blank window entries
  are not used, because they are also saved for rows you left unchecked.
- During the batch every line of each file is processed without showing the
  window; files with nothing to change are left as they are. A normal run keeps
  the batch list but does not use it.

## Build

See `.github/workflows/remove-unicode-characters.yml`.
