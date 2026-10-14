# Typewriter effect (Subtitle Edit 5 plugin)

Splits each selected subtitle line into a sequence of short, timed lines that
progressively reveal the text — character by character — over the same on-screen
duration. Optionally holds the fully-typed line for an *end delay* before the
original end time.

Ported from the SE4 *Effect → Typewriter…* dialog, repackaged as a standalone
Avalonia 11 desktop app that communicates with Subtitle Edit through the
[SE5 plugin JSON contract](https://github.com/SubtitleEdit/subtitleedit/blob/main/docs/plugin.md).

## Batch convert

Typewriter effect supports Subtitle Edit's **Batch convert** (Tools → Batch convert → Run plugin; needs a Subtitle Edit version with plugin batch support).

- **Configure...** opens the window without a subtitle to set the end delay (0.5 s by default).
- During the batch every line is split with the saved end delay, without showing the window. Lines of 500 ms or less are left as they are, as in the window; files with nothing to change are left untouched.

## Files

| File | Purpose |
|------|---------|
| `plugin.json` | Manifest. Currently uses `runtime: dotnet` (the build workflow rewrites it to per-platform `executables` for release zips). |
| `PluginContract.cs` | Request/response DTOs (the JSON contract). |
| `Program.cs` | Reads the request, boots Avalonia, writes the response. |
| `App.axaml(.cs)` | Avalonia application. Picks `Dark` or `Light` theme variant based on `request.theme`. |
| `MainWindow.axaml(.cs)` | The settings dialog (end-delay numeric input + OK/Cancel). |
| `SubRipParser.cs` | Minimal SRT parser/serializer. |
| `TypewriterResponses.cs` | Builds the run / batch / configure responses. |
| `TypewriterEngine.cs` | Generates the exploded paragraphs from the original ones. |

## Build (local)

```
dotnet publish TypewriterEffect.csproj -c Release -r <rid> --self-contained -o publish
```

## CI builds

`.github/workflows/typewriter.yml` does a matrix self-contained publish for the
six supported RIDs (`win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`,
`osx-x64`, `osx-arm64`), rewrites `plugin.json` to use the per-OS `executables`
block, and (on manual dispatch with a tag) attaches all six zips to a GitHub
release. The `se5-plugins.json` index then points at the release URLs via its
per-platform `downloads` map.
