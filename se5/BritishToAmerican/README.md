# British to American (Subtitle Edit 5 plugin)

Mirror of the [American to British](../AmericanToBritish/) plugin in the
opposite direction. Reuses the bundled word list (~1850 pairs) and the shared
`EnglishVariantConverter` from [`Plugin-Shared`](../Plugin-Shared/) with
`Direction = BrToUs`.

Word list pairs marked `only="UsToBr"` are skipped in this direction: their
British-side word is also standard American English (car, film, flat, lift,
torch...), so "converting" it would rewrite text that is already correct
American (issue [#14374](https://github.com/SubtitleEdit/subtitleedit/issues/14374)).

## Batch convert

British to American supports Subtitle Edit's **Batch convert** (Tools → Batch convert → Run plugin; needs a Subtitle Edit version with plugin batch support).

- **Configure...** opens the window without a subtitle. There is nothing to set, so it only explains what the batch does; OK is always enabled.
- During the batch every line is converted without showing the window - the same changes the preview checks by default. Files with nothing to convert are left as they are.

## Build

See `.github/workflows/british-to-american.yml`.
