using System;
using System.Linq;
using System.Reflection;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class AssemblyUtils
    {
        public static TAttribute GetCustomAttribute<TAttribute>(Assembly assembly) where TAttribute : Attribute
        {
            // get metadata from assembly
            TAttribute attribute = assembly.GetCustomAttributes(typeof(TAttribute), false).OfType<TAttribute>().FirstOrDefault();
            return attribute;
        }
    }
}
