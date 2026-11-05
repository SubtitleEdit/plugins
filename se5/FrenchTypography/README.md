# French typography (Subtitle Edit 5 plugin)

Fixes the spacing of French punctuation, so a `?` or `»` never ends up alone at the start of a line:

| Before | After (France) |
|---|---|
| `Tu viens?` | `Tu viens ?` |
| `Attention: danger!` | `Attention : danger !` |
| `Il a dit "jamais"...` | `Il a dit « jamais »…` |

The space added is a no-break space (U+00A0), or optionally a narrow no-break space (U+202F).

The window is in French when Subtitle Edit runs in French, and in English otherwise.

Built on Avalonia and the shared [`Plugin-Shared`](../Plugin-Shared/) library.

## Rules

- **France** (also Belgium and Switzerland): a space before `;` `:` `!` `?`.
- **Canada** (OQLF): a space before `:` only. Spaces before `;` `!` `?` are removed.
- Both: a space after `«` and before `»`. An existing normal, thin or no-break space is replaced, so there is exactly one.
- `"text"` becomes `« text »` when a subtitle has its straight quotes in pairs (option).
- Exactly three dots become `…`. Four or more dots are left alone (option).
- No space is added after another mark (`?!`, `…?`), after an opening bracket or dash, or at the start of a line.
- A colon with no space on either side is left alone: `10:30`, `16:9`, `http://`.
- Tags like `<i>`, `<font color="...">` and `{\an8}` are never changed and are skipped when looking at the neighbouring text.

## UI

- Every changed line is a checkable before/after card. No-break spaces show as `·` in the preview.
  *OK* applies the checked lines as one "French typography" undo step.
- *All lines* / *Selected lines*: lines selected in Subtitle Edit are the default scope.
- Variant, space and the two options are remembered.

Works on the lines exactly as Subtitle Edit holds them, so the styles, actors and
other fields of formats like ASSA are kept. On older SE versions it falls back to SubRip.

## Batch convert

French typography supports Subtitle Edit's **Batch convert** (Tools → Batch convert → Run plugin).
*Configure* shows the options; every line of each file is then fixed without showing the window.

## Build

See `.github/workflows/french-typography.yml`.
