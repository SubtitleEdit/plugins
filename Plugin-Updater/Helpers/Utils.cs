using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Plugin_Updater.Helpers
{
    public static class Utils
    {
        public static string GetMetaFile()
        {
            //AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(path + @"\..\..\..\..\");
            path = Directory.GetCurrentDirectory();
            path = Path.Combine(path, "Plugins4.xml");
            Debug.Assert(File.Exists(path), "Meta file not found");
            return path;
        }

        public static bool Validate(PluginInfo pluginInfo)
        {
            return true;
        }
    }
}
