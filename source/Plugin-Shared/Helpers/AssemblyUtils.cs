using System;
using System.IO;
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

        public static Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // NOTE: this was written to avoid loading a assembly that is already loading into
            // appdomain, but that wasn't the case because When Probing AppDomain get checked
            // the issue was because SubtitleEdit.exe force loading assembly without
            // (unloading pre - loaded assembly) or checking if the assembly
            // was already loaded into domain
#if DEBUG
            var domainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(asm => asm.FullName.Equals(args.Name, StringComparison.OrdinalIgnoreCase));

            if (domainAssembly != null)
            {
                return domainAssembly;
            } 
#endif
            //new AppDomainManager()
            //AppDomain.CreateDomain()
            var pluginFile = Path.Combine(FileUtils.Plugins, $"{args.Name.Split(',')[0]}.dll");
            return Assembly.Load(File.ReadAllBytes(pluginFile));
        }
    }
}
