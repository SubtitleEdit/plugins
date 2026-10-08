# Subtitle Edit Plugins

Plugins for [Subtitle Edit](https://github.com/SubtitleEdit/subtitleedit).
This repository holds two kinds of plugins:

| | Subtitle Edit 5 | Subtitle Edit 4 |
|---|---|---|
| Folder | [`se5/`](se5) | [`source/`](source) |
| Plugin index | [`se5-plugins.json`](se5-plugins.json) | [`Plugins4.xml`](Plugins4.xml) |
| Platforms | Windows, Linux, macOS (x64 + arm64) | Windows only |
| Technology | Standalone executable, JSON request/response | In-process WinForms DLL (.NET Framework) |

SE4 plugins do **not** run in Subtitle Edit 5 - they have to be ported.


## Subtitle Edit 5 plugins

### Using plugins

1. Turn on the **Plugins** menu: **Options → Settings → Appearance → Show Plugins menu**.
2. Choose **Plugins → Manage plugins... → Get plugins online...** to install or update
   plugins from [`se5-plugins.json`](se5-plugins.json).
3. Run a plugin from the **Plugins** menu. Its changes are applied as one undo step.

### Available plugins

| Plugin | Description |
|---|---|
| [ARTE check](se5/ArteCheck) | Checks and fixes EBU STL teletext subtitles for ARTE delivery. |
| [American to British](se5/AmericanToBritish) | Converts American English spellings to British English. |
| [British to American](se5/BritishToAmerican) | Converts British English spellings to American English. |
| [Haxor](se5/Haxor) | Translates text to "haxor" - the minimal reference plugin. |
| [Persian Subtitle Fixes](se5/PersianErrors) | Fixes common errors in Persian (Farsi) subtitles - 1,500+ rules in 12 groups. |
| [Remove Unicode characters](se5/RemoveUnicodeCharacters) | Finds non-ANSI characters and lets you remove or replace each one. |
| [Split dialogs](se5/SplitDialogs) | Splits dialog lines ("- Hi!" / "- Hello.") into one subtitle per speaker. |
| [Typewriter effect](se5/TypewriterEffect) | Reveals the text character by character in short timed parts. |
| [Word censor](se5/WordCensor) | Censors offensive words (grawlix, custom text, or a random replacement). |

See [`se5-plugins.json`](se5-plugins.json) for current versions and downloads.

### How an SE5 plugin works

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

### Plugin folder layout

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

### Publishing a plugin

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

### Porting an SE4 plugin

Many SE4 plugins are already covered by built-in SE5 features (auto-translate engines, Word spell
check, dialog style, Merge two subtitles, ASSA draw/effects, ...). If yours is not, port it by
moving the logic from the `IPlugin.DoAction` method into an executable that reads `request.json`
and writes `response.json`. Contributions are welcome!


## Subtitle Edit 4 plugins

You can write your own plugins for Subtitle Edit in any .NET language.


### What can be done with plugins

At the moment plugins can be made in these menus: File, Tools, Sync, Translate, Spell check.

A new subtitle format can also be added via a plugin.

If you want to extend Subtitle Edit somewhere else, please open an issue.


### Compiling

Please compile the plugins for the "Any CPU" platform.

Framework version: If possible compile with .NET 4.0 (can be used with 4+).


### Requirements

Use `Nikse.SubtitleEdit.PluginLogic` as Entry point namespace, take a look [here](https://github.com/SubtitleEdit/plugins/blob/main/source/Haxor/DLL/Plugin.cs).

The DLL filename must be `<classname>.dll` - e.g. `SyncViaOtherSubtitle.dll`.

### Build status (SE4)
[![Build .NET Framework Project](https://github.com/SubtitleEdit/plugins/actions/workflows/main.yml/badge.svg)](https://github.com/SubtitleEdit/plugins/actions/workflows/main.yml)
