# American to British (Subtitle Edit 5 plugin)

Converts American English spellings to British English in the subtitle, using a
bundled word list (~1000 pairs). Shows a checkable preview of every proposed
change so you can review and toggle individual conversions before applying.

First Subtitle Edit 5 plugin built on top of the shared
[`Plugin-Shared`](../Plugin-Shared/) library — Avalonia app boot, theme passing,
SRT parsing, and the JSON contract all come from there. The plugin code itself
is just the converter + the preview window.

## Build (local)

```
dotnet publish AmericanToBritish.csproj -c Release -r <rid> --self-contained -o publish
```

## CI builds

`.github/workflows/american-to-british.yml` does a matrix self-contained publish
for the six supported RIDs (`win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`,
`osx-x64`, `osx-arm64`), rewrites `plugin.json` to use the per-OS `executables`
block, and (on manual dispatch with a tag) attaches all six zips to a GitHub
release.
