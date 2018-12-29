using System;
using System.Linq;
using System.Reflection;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class AssemblyUtils
    {
        public static TAttrib GetCustomAttribute<TAttrib>(Assembly assembly) where TAttrib : Attribute
        {
            return assembly.GetCustomAttributes(typeof(TAttrib), false).Cast<TAttrib>().FirstOrDefault();
            // get metadata from assembly
            //return assembly.GetCustomAttributes(typeof(TAttrib), false).OfType<TAttrib>().FirstOrDefault();
        }
    }
}
