# Split dialogs (Subtitle Edit 5 plugin)

Splits dialog lines into one subtitle per speaker
([subtitleedit#14917](https://github.com/SubtitleEdit/subtitleedit/issues/14917)):

```
00:00:01,000 --> 00:00:04,000        00:00:01,000 --> 00:00:01,738
- Andro!                       →     Andro!
- Ben, I've been oozed.
                                     00:00:01,762 --> 00:00:04,000
                                     Ben, I've been oozed.
```

Built on Avalonia and the shared [`Plugin-Shared`](../Plugin-Shared/) library.

## What counts as a dialog

The same rule as Subtitle Edit's own dialog helper: a line starting with a dash
(`-`, `‐`, `–` or `—`) starts a new speaker when the line before it ends a
sentence (`.`, `!`, `?`, `…`, `♪`, `)`, `]`, an interruption like `foobar--`,
...). So all of these split:

```
- Andro!                 Andro!                   - How are you?
- Ben, I've been oozed.  - Ben, I've been oozed.  - I'm fine, but I would
                                                  have been better with candy.
```

Unlike Subtitle Edit's check it is not limited to two or three lines - a
subtitle with three speakers becomes three subtitles. A double dash (`--and
then`) is a continuation, not a speaker.

Uncheck *Only split where the line before the dash ends a sentence* for
languages without sentence-ending periods, or subtitles that leave them out.

## Splitting

- The dash is removed from every part.
- The time is shared out by text length, as Subtitle Edit's *Split* does: equal
  halves when the parts are about as long, otherwise weighted, with no part
  getting less than a quarter (for two parts) of the time.
- *Gap between lines (ms)* is left between the parts (default 24, Subtitle
  Edit's default minimum gap).
- Formatting carries over: `<i>- Hi!` / `- Hello.</i>` becomes `<i>Hi!</i>` and
  `<i>Hello.</i>`, and ASSA override tags such as `{\an8}` are repeated on each
  part.
- Advanced Sub Station Alpha and Sub Station Alpha files are edited in their own
  format, so styles, actors, layers, margins and comments are kept. Other
  formats are edited as SubRip, like the other SE5 plugins.

## UI

A preview with one row per dialog line: checkbox, line number, the line before
(with its start time) and the new lines after (with their start times).
*Select all* / *Select none* toggle every row; *Apply* splits the checked lines
and registers a "Split dialogs" undo entry. The two options are remembered
between runs.

## Build

See `.github/workflows/split-dialogs.yml`.
