using System;
using System.IO;

namespace SubtitleEdit.Logic
{
    internal static class Configuration
    {
        public static double CurrentFrameRate = 23.976;
        public static string ListViewLineSeparatorString = "<br />";
        private const int PlatformWindows = 1;
        private const int PlatformLinux = 2;
        private const int PlatformMac = 3;
        private static int _platform;

        private static int GetPlatform()
        {
            // Current versions of Mono report MacOSX platform as Unix
            return Environment.OSVersion.Platform == PlatformID.MacOSX || (Environment.OSVersion.Platform == PlatformID.Unix && Directory.Exists("/Applications") && Directory.Exists("/System") && Directory.Exists("/Users"))
                ? PlatformMac
                : Environment.OSVersion.Platform == PlatformID.Unix
                    ? PlatformLinux
                    : PlatformWindows;
        }

        public static bool IsRunningOnWindows
        {
            get
            {
                if (_platform == 0)
                {
                    _platform = GetPlatform();
                }
                return _platform == PlatformWindows;
            }
        }

        public static bool IsRunningOnLinux
        {
            get
            {
                if (_platform == 0)
                {
                    _platform = GetPlatform();
                }
                return _platform == PlatformLinux;
            }
        }
    }
}
