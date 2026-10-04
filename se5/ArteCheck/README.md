# ARTE check (Subtitle Edit 5 plugin)

Checks EBU STL teletext subtitles against ARTE's delivery rules and fixes what can be fixed safely. Things that need a person (no room for a gap, text that cannot be split, unmappable colors) are listed as **alarms**.

The rules come from the *Check and Fix ARTE Errors* tool by Triathlon-rally in
[subtitleedit#15185](https://github.com/SubtitleEdit/subtitleedit/pull/15185). Here they are packaged as a plugin.

Needs a Subtitle Edit build whose plugin contract includes `subtitle.header` and `subtitle.paragraphs`. Only those carry the EBU header and the teletext rows of a binary EBU STL file. With an older build the plugin still runs on the SubRip text, but it skips the header and row checks.

## Checks

| Check | What it does |
|---|---|
| Header / language | Sets code page 850, `STL25.01`, teletext display standard, 40 characters × 23 rows, and the profile's language code (STA 08, STF 0F, HG DEU 2D, HG FRA 2F). Only the fields that change are shown. |
| Start time code | Optional: shifts the whole file so the programme starts on a given time code (e.g. `10:00:00:00`) and updates the header's TCP. |
| Blank control subtitle | Inserts or fixes the 5-frame blank subtitle at the programme start. |
| Frame rate | *Timed for* converts 23.976/24/29.97/30 fps material to 25 fps. The programme start stays where it is; only the time after it is scaled. |
| Frame-accurate time codes | Rounds in and out times to whole 25 fps frames. |
| Display duration | Checks reading time (Subtitle Edit's minimum duration and max CPS, minus the reading tolerance), plus the maximum duration. Corrections are offered but not pre-selected. |
| Maximum two lines / characters per row | Rebalances the line break, or splits the subtitle into several inside the original time range. Each color change uses a teletext cell. |
| Teletext row | Double height: one line on row 22, two lines on row 20. Moves a whole file down one row if it sits one row too high. Rows higher up the screen are left alone. |
| Teletext colors | Normal subtitles are all yellow or uncolored, with no boxing. SDH colors map to the eight teletext colors. |
| No italics, unneeded spaces | Teletext has no italics; leading and trailing spaces use cells. |
| Minimum gaps | Shares the missing frames between the previous out time and the next in time, without going under the minimum durations. |

## UI

- **Sidebar.** Delivery profile (read from the header language), source frame rate, programme start, limits, and which checks to run. *Reset to ARTE preset* sets 37 characters, 5-frame gaps, 15 % tolerance and strict durations.
- **Findings.** Grouped by check, with corrections (green) and alarms (red). Text changes are previewed as teletext: a 40-cell row on black, in the real teletext colors. Header, row and time code changes are shown as before → after values.
- **Apply and re-check** applies the selected corrections inside the plugin and checks again, because one fix can make room for another. **Undo** steps back one pass.
- **OK** applies whatever is still selected and returns the result to Subtitle Edit as a single undo step. **Report** saves the findings as a text file.

The limits and enabled checks are remembered between runs. Durations, CPS and double height come from Subtitle Edit's settings (`request.rules`).

## Build

See `.github/workflows/arte-check.yml`. Tests are in `../ArteCheck.Tests`:

```
dotnet test se5/ArteCheck.Tests
```
