using System;
using System.Linq;
using System.Reflection;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class AssemblyUtils
    {
        public static TAttrib GetCustomAttribute<TAttrib>(Assembly assembly) where TAttrib : Attribute
        {
            // get metadata from assembly
            TAttrib attribute = assembly.GetCustomAttributes(typeof(TAttrib), false).OfType<TAttrib>().FirstOrDefault();
            return attribute;
        }
    }
}
