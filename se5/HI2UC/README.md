# Hearing impaired to uppercase (Subtitle Edit 5 plugin)

Changes the case of hearing-impaired text:

| Before | After |
|---|---|
| `(laughs) John: Hi.` | `(LAUGHS) JOHN: Hi.` |
| `<i>[door slams]</i>` | `<i>[DOOR SLAMS]</i>` |
| `- Mary: Wait!` | `- MARY: Wait!` |

It is the SE5 port of the SE4 *HI2UC* plugin by ivandrofly.

Built on Avalonia and the shared [`Plugin-Shared`](../Plugin-Shared/) library.

## What changes

- **Moods**: text inside `( )` and `[ ]`. Empty brackets like `()` are removed.
  Lines with unbalanced brackets are still changed, but the line is flagged so you can check it.
- **Narrators**: the speaker label before a colon, on each line. These are left alone:
  - times (`12:30`)
  - links (`http://`)
  - text with `?` or `!` before the colon
  - credits like `Corrected by:`
- Tags like `<i>`, `<font color="...">` and `{\an8}` are never changed.

Styles: *Uppercase*, *Toggle case*, *Title case*, *Sentence case* and *Lowercase*.

## UI

- Pick the style, and whether to change moods, narrators or both.
- *All lines* / *Selected lines*: lines selected in Subtitle Edit are the default scope.
- Every changed line is a checkable before/after card. *OK* applies the checked
  lines as one "Hearing impaired to uppercase" undo step.
- Style, options and scope are remembered between runs.

Works on the lines exactly as Subtitle Edit holds them, so the styles, actors and
other fields of formats like ASSA are kept. On older SE versions it falls back to SubRip.

## Batch convert

Hearing impaired to uppercase supports Subtitle Edit's **Batch convert** (Tools → Batch convert → Run plugin). This needs a Subtitle Edit version with plugin batch support.

- **Configure...** opens the window without a subtitle, to pick the style and options.
- During the batch, every line of each file is changed without showing the window. Files with nothing to change are left as they are.

## Changes from the SE4 plugin

- A line with both a mood and a narrator now gets both changes. Before, the second change overwrote the first.
- The saved style is used again on the next run.
- Lines with nothing to change no longer appear in the list as warnings.
- No usage data is sent anywhere.

## Build

See `.github/workflows/hi2uc.yml`.
