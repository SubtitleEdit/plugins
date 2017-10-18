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
            // app-data-roaming
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit");
            if (!Directory.Exists(path))
            {
                path = Assembly.GetExecutingAssembly().CodeBase;
                if (path.StartsWith(@"file:\", StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring(6);
                }
                path = Path.GetDirectoryName(path);

                // portable mode?
            }
            PluginDirectory = Path.Combine(path, "Plugins");

            // TODO: Check if plugin is running on a portable version of subtitle edit
            // and upate *IsPortableMode
        }
    }
}
