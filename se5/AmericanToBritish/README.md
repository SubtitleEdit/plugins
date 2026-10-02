# American to British (Subtitle Edit 5 plugin)

Converts American English spellings to British English in the subtitle, using a
bundled word list (~1850 pairs). Shows a checkable preview of every proposed
change so you can review and toggle individual conversions before applying.

## Local word list

The gear button opens the local word list, where you can add your own conversions
(words or phrases) and ignore built-in ones. It is saved as
`Plugins/AmericanToBritish.xml` in the Subtitle Edit data folder - the same file
and format as the Subtitle Edit 4 plugin, so an existing list keeps working:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Words>
  <Word us="dumb" br="stupid" />
  <Ignore us="color" />
</Words>
```

A `Word` with the same American spelling as a built-in pair replaces it; `Ignore`
turns a built-in pair off.

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
