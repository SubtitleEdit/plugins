using System;
using System.IO;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class FileUtils
    {
        public static string PluginDirectory { get; }

        public static bool IsPortableMode { get; }

        static FileUtils()
        {
            // TODO: initialize plugin directory
            //string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            //if (path.StartsWith(@"file:\", StringComparison.OrdinalIgnoreCase))
            //{
            //    path = path.Substring(6);
            //}

            string path = null;

            var appDataRoamingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit");
            //path = Path.Combine(path, "Plugins");
            if (Directory.Exists(appDataRoamingPath))
            {
                path = Path.Combine(appDataRoamingPath, "Plugins");
            }
            PluginDirectory = path;

            // TODO: Check if plugin is running on a portable version of subtitle edit
            // and upate *IsPortableMode
        }
    }
}
