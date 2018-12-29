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
        /// Returns Subtitle Edit's dictionary directory.
        /// </summary>
        public static string Dictionary { get; }

        /// <summary>
        /// Returns true if plugin is running on a portable version of subtitle edit.
        /// </summary>
        public static bool IsPortableMode { get; }

        static FileUtils()
        {
            // TODO: HANDLE FOR INSTALLED VERSION
            Assembly assemblyEntry = Assembly.GetEntryAssembly();

            // Assembly.CodeBase: If the assembly was loaded as a byte array, using an overload of the Load method that takes an array of bytes,
            // this property returns the location of the caller of the method, not the location of the loaded assembly.
            // NOTE: in portable mode, this returns SubtitleEdit.exe path
            // UriBuilder uriBuilder = new UriBuilder(assembly.CodeBase);

            // removed file://
            //string path = Uri.UnescapeDataString(uriBuilder.Path);
            //path = Path.GetDirectoryName(path);

            // subtitile-edit executable file path
            string path = new Uri(assemblyEntry.CodeBase).LocalPath; // path\SubtitleEdit.exe

            // the directory where the datas are located
            BaseDirectory = Path.GetDirectoryName(path);

            string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit");

            IsPortableMode = Directory.GetFiles(BaseDirectory, "unins*.*").Length == 0;

            Plugins = Path.Combine(IsPortableMode ? BaseDirectory : appDataDir, "Plugins");
            Dictionary = Path.Combine(IsPortableMode ? BaseDirectory : appDataDir, "Dictionaries");

            // Note: getting path using Assembly.CodeBase is suitable when SE is ran/executed from a remote pc
        }

        public static string GetConfigFile(string fileName) => Path.Combine(Plugins, fileName);
    }
}
