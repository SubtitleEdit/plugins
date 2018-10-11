using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    /// <summary>
    /// Inform Subtitle Edit that that plugin contains this attribute should be run immediately after loading.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class InvokeOnLoading : Attribute
    {
    }
}
