# Word censor (Subtitle Edit 5 plugin)

Censors offensive words in the subtitle by replacing the first ~50% of each
match with random "grawlix" characters (`@ # ! ? $ % &`). Optionally wraps the
censored word in a red `<font>` tag.

Built on the shared [`Plugin-Shared`](../Plugin-Shared/) library — theme
handling, JSON contract, SRT parsing, and the Avalonia boot are inherited.
The plugin code is just the censor engine + the preview window.

## UI

A preview window similar to *American to British*:

- A list of every line that contains at least one offensive word, with the
  original line (muted) and the censored line (SemiBold) shown side-by-side.
- A checkbox per line so individual changes can be skipped.
- **Highlight censored words in red** toggles the `<font color="#ff0000">`
  wrapping — preview updates live.
- *Select all* / *Select none* buttons, a "X of Y line(s) selected" summary,
  *Cancel*, and an accent-styled *Apply* (disabled when nothing is checked).
- The "color red" preference is persisted via the plugin Settings round-trip.

## Build

See `.github/workflows/word-censor.yml`.
