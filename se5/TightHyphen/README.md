# Tight hyphen (Subtitle Edit 5 plugin)

Removes the space after a dialog dash at the start of a line:

| Before | After |
|---|---|
| `- Where are you going?`<br>`- Home.` | `-Where are you going?`<br>`-Home.` |
| `<i>- Hi!</i>` | `<i>-Hi!</i>` |

It is the SE5 port of the SE4 *TightHyphen* plugin by ivandrofly.

Built on Avalonia and the shared [`Plugin-Shared`](../Plugin-Shared/) library.

## Rules

- Only a dash that starts the visible text of a line is changed. It may come after tags like `<i>`, `<font>` or `{\an8}`.
- Every space after the dash is removed.
- A dash followed by a number (`- 5 dollars`) is left alone, so it does not read as a minus sign.

## UI

- Every changed line is a checkable before/after card. *OK* applies the checked
  lines as one "Tight hyphen" undo step.
- *All lines* / *Selected lines*: lines selected in Subtitle Edit are the default scope.

Works on the lines exactly as Subtitle Edit holds them, so the styles, actors and
other fields of formats like ASSA are kept. On older SE versions it falls back to SubRip.

## Batch convert

Tight hyphen supports Subtitle Edit's **Batch convert** (Tools → Batch convert → Run plugin).
It has no options. Every line of each file is changed without showing the window.

## Changes from the SE4 plugin

- Each line can be checked or unchecked before applying.
- `- 5` is no longer turned into `-5`.
- Line breaks are kept as they were.

## Build

See `.github/workflows/tight-hyphen.yml`.
