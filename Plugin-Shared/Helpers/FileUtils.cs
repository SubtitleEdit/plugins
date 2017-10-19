using System;
using System.IO;
using System.Reflection;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class FileUtils
    {
        /// <summary>
        /// Return plugins installation directory.
        /// </summary>
        public static string PluginDirectory { get; }

        /// <summary>
        /// Returns true if plugin is running on a portable version of subtitle edit.
        /// </summary>
        public static bool IsPortableMode { get; }

        static FileUtils()
        {
            // look for portable version's plugin directory first
            string path = Assembly.GetExecutingAssembly().CodeBase;
            if (path.StartsWith(@"file:\", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(6);
            }
            path = Path.GetDirectoryName(path);
            // app-data-roaming
            if (!Directory.Exists(path))
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit");
                // portable mode?
            }
            PluginDirectory = Path.Combine(path, "Plugins");

            // TODO: Check if plugin is running on a portable version of subtitle edit
            // and upate *IsPortableMode
        }

        public static string GetConfigFile(string fileName) => Path.Combine(PluginDirectory, fileName);
    }
}
