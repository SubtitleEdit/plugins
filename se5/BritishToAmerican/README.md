# British to American (Subtitle Edit 5 plugin)

Mirror of the [American to British](../AmericanToBritish/) plugin in the
opposite direction. Reuses the bundled word list (~1850 pairs) and the shared
`EnglishVariantConverter` from [`Plugin-Shared`](../Plugin-Shared/) with
`Direction = BrToUs`.

Word list pairs marked `only="UsToBr"` are skipped in this direction: their
British-side word is also standard American English (car, film, flat, lift,
torch...), so "converting" it would rewrite text that is already correct
American (issue [#14374](https://github.com/SubtitleEdit/subtitleedit/issues/14374)).

## Build

See `.github/workflows/british-to-american.yml`.
