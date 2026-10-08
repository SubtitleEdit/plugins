# Persian Subtitle Fixes (Subtitle Edit 5 plugin)

Fixes common errors in Persian (Farsi) subtitles - اصلاح خطاهای رایج زیرنویس فارسی.

This is the Subtitle Edit 5 version of the *Persian Subtitle Fixes* plugin by
[Mohammad Sasan (msasanmh)](https://github.com/msasanmh/PersianSubtitleFixes)
([`source/PersianErrors`](../../source/PersianErrors) for Subtitle Edit 3/4). The 1,500+
find-and-replace rules in [`multiple_replace.xml`](multiple_replace.xml) are his, unchanged.
The plugin runs them the same way and wraps them in a new Avalonia UI built on the shared
[`Plugin-Shared`](../Plugin-Shared/) library.

## Fix groups

| Group | | Example |
|---|---|---|
| Fix Unicode Control Char | نویسه‌های کنترلی یونیکد | Removes doubled right-to-left embedding marks (U+202B) and the spaces after them |
| Change Arabic Chars to Persian | تبدیل حروف عربی به فارسی | `املاك شدني` → `املاک شدنی` |
| Remove Unneeded Spaces | حذف خط فاصله نادرست | `گفت :  باشه` → `گفت: باشه` |
| Add Missing Spaces | نبود خط فاصله | `سلام،عزیزم` → `سلام، عزیزم` |
| Fix Dialog Hyphen | دیالوگ‌ها | Moves dialog dashes to where right-to-left players expect them |
| Fix Wrong Chars | کاراکترهای اشتباه | `سلام,, خوبی?` → `سلام، خوبی؟` |
| Fix Misplaced Chars | مکان اشتباه | Moves end punctuation to the start of the text |
| Fix Abbreviations | کلمات اختصاری | `اف بی آی` → `اف.بی.آی` |
| Space to Invisible Space | خط فاصله نامرئی | `میشود خانه ها` → `می‌شود خانه‌ها` (zero-width non-joiner) |
| OCR | جاگذاری کلمات | `سوسابقه` → `سوءسابقه` |
| Remove Leading Dots | حذف سه نقطه از ابتدای جمله | `در ادامه...` → `در ادامه` |
| Remove Dot from the End of Line | حذف نقطه از انتهای جمله | `.می‌شود` → `می‌شود` |

The groups run in this order, so a later group sees the output of an earlier one.

## UI

- **Sidebar.** Turn each group on or off by clicking its row. Hover over a row to see a
  description and a live example. The rules themselves produce the example's result. The
  badge shows how many lines the group changes. *Check* limits the run to the lines selected
  in Subtitle Edit.
- **Fixes.** Each changed line is a card: line number, time code, the groups that changed it,
  and the text before and after, right to left, with the changed characters highlighted.
  Filter by group with the chips, or search for text or a line number. Uncheck the fixes you
  do not want.
- **Show hidden characters** draws the zero-width non-joiner as `·`, direction marks as small
  `RLE`/`RLM`/... labels, and a changed line break as `↵`. Without it, a fix that only adds a
  نیم‌فاصله would look like no change at all.
- **Apply and re-check** applies the selected fixes inside the plugin and checks again.
  **Undo** steps back one pass. **OK** applies what is still selected and returns the
  subtitle.

The enabled groups, the display option and the scope are remembered between runs.

## Differences from the Subtitle Edit 3/4 plugin

- Every subtitle format is kept. The plugin changes only the text of each line (`subtitle.paragraphs`),
  so styles, actors and positions survive. Before, the subtitle came back as SubRip.
- Line breaks keep their style. The rules expect Windows line breaks (`\r\n`), so text is
  converted for the rules and converted back afterwards. The rules work the same on macOS and
  Linux.
- Plain (non-regex) rules no longer get their quotes escaped. In the old plugin the two
  `"` + ZWNJ rules in *Space to Invisible Space* could never match.
- Fixes are shown with highlighted differences and the groups that made them, instead of a
  plain before/after list.

## Building

```bash
dotnet test se5/PersianErrors.Tests/PersianErrors.Tests.csproj
dotnet run --project se5/PersianErrors -- request.json
```

The workflow [`persian-errors.yml`](../../.github/workflows/persian-errors.yml) builds
self-contained zips for Windows, Linux and macOS (x64 and arm64). Run it manually with
*release* to publish `se5-persian-errors-vX.Y`.
