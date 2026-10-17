# Lines unbreaker (Subtitle Edit 5 plugin)

Joins subtitles with two or more short lines into one line, when the result fits on one line:

| Before | After |
|---|---|
| `Where are`<br>`you going?` | `Where are you going?` |
| `<i>Wait</i>`<br>`<i>for me.</i>` | `<i>Wait for me.</i>` |

It is the SE5 port of the SE4 *LinesUnbreaker* plugin by ivandrofly.

Built on Avalonia and the shared [`Plugin-Shared`](../Plugin-Shared/) library.

## Rules

- A subtitle is joined only when the joined text is at most *N* characters long. Tags like `<i>` and `{\an8}` are not counted.
  *N* defaults to Subtitle Edit's *single line max. length*.
- Lines are joined with one space. Extra spaces are removed, and `</i> <i>` at the join becomes a single space.
- You can skip:
  - **Dialogs**: a line starts with `-`. Skipped by default.
  - **Moods**: `( )` or `[ ]` in a line.
  - **Narrators**: `John:` at the start of a line. Skipped by default. Times like `12:30` are not narrators.

## UI

- Every subtitle that would be joined is a checkable before/after card, showing the new length.
  *OK* applies the checked subtitles as one "Lines unbreaker" undo step.
- *All lines* / *Selected lines*: lines selected in Subtitle Edit are the default scope.
- The maximum length, the skip options and the scope are remembered between runs.

Works on the lines exactly as Subtitle Edit holds them, so the styles, actors and
other fields of formats like ASSA are kept. On older SE versions it falls back to SubRip.

## Batch convert

Lines unbreaker supports Subtitle Edit's **Batch convert** (Tools → Batch convert → Run plugin).

- **Configure...** opens the window without a subtitle, to set the maximum length and what to skip.
- During the batch, every short subtitle in each file is joined without showing the window.

## Changes from the SE4 plugin

- Only the checked subtitles are joined. Before, every listed subtitle was joined.
- The length check ignores tags, the same way the length is shown.
- Dialog lines in italics (`<i>- Hi!</i>`) are recognised as dialogs.

## Build

See `.github/workflows/lines-unbreaker.yml`.
