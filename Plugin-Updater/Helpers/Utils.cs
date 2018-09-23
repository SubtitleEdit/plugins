using System.IO;
using System.Reflection;

namespace Plugin_Updater.Helpers
{
    public static class Utils
    {
        public static string GetMetaFile()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(path + @"\..\..\..\");
            path = Directory.GetCurrentDirectory();
            path = Path.Combine(path, "Plugins4.xml");
            return path;
            //return Path.Combine(path, "\\Plugins4.xml");
        }

        public static bool Validate(PluginInfo pluginInfo)
        {
            return true;
        }
    }
}
