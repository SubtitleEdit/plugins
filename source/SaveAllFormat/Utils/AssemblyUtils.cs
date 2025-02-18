namespace Nikse.SubtitleEdit.PluginLogic.Utils;

using System;
using System.Linq;
using System.Reflection;

public static class AssemblyUtils
{
    private static Assembly _libseCached;

    public static Assembly GetLibse()
    {
        if (_libseCached == null)
        {
            _libseCached = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.GetName().Name.Equals("libse"));
        }

        return _libseCached;
    }
}