# Haxor (Subtitle Edit 5 sample plugin)

Translates subtitle text to "haxor" — lower-cases it and swaps a few letters for
look-alikes (`a→4`, `e→3`, `o→0`, `s→$`, `i→!`, `c→©`, `h→H`, `k→K`, `n→ñ`, `y→¥`).
If lines are selected in the grid only those are changed, otherwise every line is.

This is a reference plugin for the Subtitle Edit 5 plugin system. It shows the whole
contract: read the request JSON, transform the subtitle, write the response JSON.
See [docs/plugin.md](https://github.com/SubtitleEdit/subtitleedit/blob/main/docs/plugin.md)
for the full specification.

## Files

| File | Purpose |
|------|---------|
| `plugin.json` | Manifest — lets Subtitle Edit list the plugin without launching it. |
| `PluginContract.cs` | The request/response DTOs (the JSON contract). |
| `Program.cs` | Entry point: read request → transform → write response → exit 0. |
| `HaxorTranslator.cs` | The actual text transformation. |

## Build

```
dotnet publish Haxor.csproj -c Release -o publish
```

## Install for testing

Copy the build output plus `plugin.json` into a folder under Subtitle Edit's
`Plugins` directory:

```
Plugins/
  Haxor/
    plugin.json
    Haxor.dll
    Haxor.runtimeconfig.json
    ... (other files from publish/)
```

The manifest uses `"runtime": "dotnet"`, so Subtitle Edit launches it as
`dotnet Haxor.dll <requestFile>` — the .NET 8 runtime must be installed. To ship a
self-contained build instead, publish with `-r <rid> --self-contained` and replace
the `runtime`/`entry` fields in `plugin.json` with an `executables` block.

## Package for the plugin index

Zip the plugin folder (so the zip contains a single top-level `Haxor/` folder),
attach it to a GitHub release, and point `downloadUrl` in `se5-plugins.json` at it.
