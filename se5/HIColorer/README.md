# Hearing impaired colorer (Subtitle Edit 5 plugin)

Colors hearing-impaired text with `<font color>` tags:

| Before | After |
|---|---|
| `- MAN: Baby.` | `- <font color="#ffff00">MAN</font>: Baby.` |
| `(door slams) Hi.` | `<font color="#00ffff">(door slams)</font> Hi.` |
| `♪ La la la ♪` | `<font color="#ff80ff">♪ La la la ♪</font>` |

It is the SE5 port of the SE4 *HIColorer* plugin by ivandrofly.

Built on Avalonia and the shared [`Plugin-Shared`](../Plugin-Shared/) library.

## What gets colored

- **Narrators**: the speaker label before a colon, on each line. These are not narrators:
  - times (`12:30`)
  - links
  - credits (`Subtitles by:`)
  - labels that come after a mood (`(laughs) John:`)
- **Moods**: everything in `( )` or `[ ]`, brackets included.
- **Music**: a whole subtitle that starts with ♪ or ♫.

Each has its own color and can be turned off.

On a line that gets colored, old `<font>` tags are replaced, so running the plugin
again with new colors recolors the line. Lines with nothing to color keep their tags.

**Remove colors** removes every `<font>` tag and keeps the text, italics and other tags.

## UI

- *Add colors* / *Remove colors*, plus a color picker for narrators, moods and music.
- *Colors* shows the preview as it will look; *Tags* shows the raw tags.
- Every changed line is a checkable before/after card. *OK* applies the checked
  lines as one "Hearing impaired colorer" undo step.
- *All lines* / *Selected lines*: lines selected in Subtitle Edit are the default scope.
- Colors and options are remembered between runs.

Works on the lines exactly as Subtitle Edit holds them, so the styles, actors and
other fields of formats like ASSA are kept. On older SE versions it falls back to SubRip.

## Batch convert

Hearing impaired colorer supports Subtitle Edit's **Batch convert** (Tools → Batch convert → Run plugin).

- **Configure...** opens the window without a subtitle, to pick the mode and colors.
- During the batch, every line of each file is changed without showing the window.

## Changes from the SE4 plugin

- The *Music* checkbox now turns music on and off. Before, the narrator checkbox controlled it.
- Colors are remembered. Before, they were never saved and started as black.
- Font tags are only replaced on lines that get colored. Before, they were removed from every line.
- A narrator inside `<i>...</i>` keeps valid tag nesting.
- Names like *Bobby* are no longer skipped because they contain "by".
- There is a before/after preview, and each line can be checked or unchecked.

## Build

See `.github/workflows/hi-colorer.yml`.
