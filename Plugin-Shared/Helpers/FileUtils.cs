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
        public static string DataDirectory { get; set; }

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
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Assembly.CodeBase: If the assembly was loaded as a byte array, using an overload of the Load method that takes an array of bytes,
            // this property returns the location of the caller of the method, not the location of the loaded assembly.
            // NOTE: in portable mode, this returns SubtitleEdit.exe path
            // UriBuilder uriBuilder = new UriBuilder(assembly.CodeBase);

            // removed file://
            //string path = Uri.UnescapeDataString(uriBuilder.Path);
            //path = Path.GetDirectoryName(path);

            // subtitile-edit executable file path
            string path = new Uri(assembly.CodeBase).LocalPath; // path\SubtitleEdit.exe

            // the directory where the datas are located
            DataDirectory = Path.GetDirectoryName(path);
            Dictionary = Path.Combine(DataDirectory, "Dictionaries");
            Plugins = Path.Combine(DataDirectory, "Plugins");
            if (!Directory.Exists(Plugins))
            {
                DataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit");
                Dictionary = Path.Combine(DataDirectory, "Dictionaries");
                Plugins = Path.Combine(DataDirectory, "Plugins");
                IsPortableMode = false;
            }
            else
            {
                IsPortableMode = true;
            }
        }

        public static string GetConfigFile(string fileName) => Path.Combine(Plugins, fileName);
    }
}
