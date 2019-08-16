using System;
using System.IO;
using System.Reflection;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class FileUtils
    {
        /// <summary>
        /// Returns data directory.
        /// </summary>
        public static string BaseDirectory { get; set; }

        /// <summary>
        /// Return plugins installation directory.
        /// </summary>
        public static string Plugins { get; }

        /// <summary>
        /// Returns Subtitle Edit's dictionaries directory.
        /// </summary>
        public static string Dictionaries { get; }

        /// <summary>
        /// Returns true if plugin is running on a portable version of subtitle edit.
        /// </summary>
        public static bool IsPortableMode { get; }

        static FileUtils()
        {
            var assembly = Assembly.GetExecutingAssembly();

            // exe file location (portable/installed)
            string path = new Uri(assembly.CodeBase).LocalPath;

            // the directory containing the SubtitleEdit.exe
            BaseDirectory = Path.GetDirectoryName(path);

            // for installed version there must be a file named "unins000.exe" at the same directory as SubtitleEdit.exe
            IsPortableMode = !File.Exists(Path.Combine(BaseDirectory, "unins000.exe"));

            // not portable, then look for Subtitle appdata directory in AppData
            if (!IsPortableMode)
            {
                BaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit");
            }

            Plugins = Path.Combine(BaseDirectory, nameof(Plugins));
            Dictionaries = Path.Combine(BaseDirectory, nameof(Dictionaries));

            // Assembly.CodeBase: If the assembly was loaded as a byte array, using an overload of the Load method that takes an array of bytes,
            // this property returns the location of the caller of the method, not the location of the loaded assembly.
            // Note: in portable mode, this returns SubtitleEdit.exe path
            // UriBuilder uriBuilder = new UriBuilder(assembly.CodeBase);
            // Note: getting path using Assembly.CodeBase is suitable when SE is ran/executed from a remote pc
        }
    }
}
