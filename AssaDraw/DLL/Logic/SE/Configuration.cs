using System;
using System.Drawing;
using System.IO;
using System.Reflection;

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

        public static Color LastColorPickerColor { get; set; } = Color.Yellow;
        public static Color LastColorPickerColor1 { get; set; } = Color.Red;
        public static Color LastColorPickerColor2 { get; set; } = Color.White;
        public static Color LastColorPickerColor3 { get; set; } = Color.Black;

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

        public static string DataDirectory 
        {
            get 
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                if (path != null && path.StartsWith("file:\\", StringComparison.Ordinal))
                {
                    path = path.Remove(0, 6);
                }

                return path;
            }                
        }
    }
}
