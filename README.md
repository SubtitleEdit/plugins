# Subtitle Edit 5 Plugins

Plugins for [Subtitle Edit 5](https://github.com/SubtitleEdit/subtitleedit) - they run on Windows,
Linux and macOS (x64 + arm64). The plugins live in [`se5/`](se5) and are listed in the online
index [`se5-plugins.json`](se5-plugins.json).

## Using plugins

1. Turn on the **Plugins** menu: **Options → Settings → Appearance → Show Plugins menu**.
2. Choose **Plugins → Manage plugins... → Get plugins online...** to install or update
   plugins from [`se5-plugins.json`](se5-plugins.json).
3. Run a plugin from the **Plugins** menu. Its changes are applied as one undo step.

## Available plugins

| Plugin | Description |
|---|---|
| [ARTE check](se5/ArteCheck) | Checks and fixes EBU STL teletext subtitles for ARTE delivery. |
| [American to British](se5/AmericanToBritish) | Converts American English spellings to British English. |
| [British to American](se5/BritishToAmerican) | Converts British English spellings to American English. |
| [Haxor](se5/Haxor) | Translates text to "haxor" - the minimal reference plugin. |
| [Italic each line](se5/ItalicEachLine) | Switches italic tags between one tag per subtitle and a tag on each line. |
| [Persian Subtitle Fixes](se5/PersianErrors) | Fixes common errors in Persian (Farsi) subtitles - 1,500+ rules in 12 groups. |
| [Remove Unicode characters](se5/RemoveUnicodeCharacters) | Finds non-ANSI characters and lets you remove or replace each one. |
| [Split dialogs](se5/SplitDialogs) | Splits dialog lines ("- Hi!" / "- Hello.") into one subtitle per speaker. |
| [Transliterate](se5/Transliterate) | Converts between scripts: Serbian, Montenegrin, Macedonian, Uzbek Cyrillic ↔ Latin; Russian, Ukrainian, Belarusian, Bulgarian, Kazakh, Greek, Korean → Latin. |
| [Typewriter effect](se5/TypewriterEffect) | Reveals the text character by character in short timed parts. |
| [Word censor](se5/WordCensor) | Censors offensive words (grawlix, custom text, or a random replacement). |

See [`se5-plugins.json`](se5-plugins.json) for current versions and downloads.

## How a plugin works

An SE5 plugin is a normal executable, so it can be written in any language and may show its own
window (the plugins here use .NET 8 + [Avalonia](https://avaloniaui.net/)):

1. Subtitle Edit writes a `request.json` (subtitle as SubRip + native format, selected lines,
   video file, settings, ...) and starts the plugin with the request file path as the first argument.
2. The plugin does its work and writes a `response.json` to the `responseFilePath` given in the request.
3. The plugin exits with code `0`. On `ok` Subtitle Edit replaces the subtitle (with undo),
   on `cancelled` nothing happens, on `error` the message is shown.

The full specification is in
[docs/plugin.md](https://github.com/SubtitleEdit/subtitleedit/blob/main/docs/plugin.md).
[`se5/Haxor`](se5/Haxor) is the smallest complete example, and [`se5/Plugin-Shared`](se5/Plugin-Shared)
contains the shared contract DTOs, SubRip parser and Avalonia theming used by the other plugins.

## Plugin folder layout

Every plugin is a folder with a `plugin.json` manifest:

```
Plugins/
  MyPlugin/
    plugin.json
    MyPlugin.exe   (Windows) / MyPlugin (Linux, macOS)
    ...
```

```json
{
  "apiVersion": 1,
  "name": "My plugin",
  "description": "What it does.",
  "version": "1.0.0",
  "author": "Your name",
  "url": "https://github.com/you/my-plugin",
  "minSeVersion": "5.0.0",
  "executables": {
    "windows": "MyPlugin.exe",
    "linux": "MyPlugin",
    "macos": "MyPlugin"
  }
}
```

Instead of `executables` you can use `"runtime": "dotnet"` + `"entry": "MyPlugin.dll"`
(launched as `dotnet MyPlugin.dll <requestFile>`, needs the .NET runtime installed) - handy
while developing. Optional fields: `menu`, `shortcut` and `icon`.

## Publishing a plugin

1. Build self-contained for each platform you support, e.g.
   `dotnet publish -c Release -r linux-x64 --self-contained`
   (`win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64`).
2. Zip each build so the zip has a single top-level folder containing `plugin.json`.
3. Attach the zips to a GitHub release.
4. Open a pull request adding an entry to [`se5-plugins.json`](se5-plugins.json) with a
   `downloads` map from platform key to zip URL. A plugin with no zip for the user's
   platform is listed but cannot be installed there.

The plugins in this repository are built and released by the workflows in
[`.github/workflows`](.github/workflows).
