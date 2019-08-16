using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class InvokeOnLoad : Attribute
    {
        public InvokeOnLoad(bool isWorkerThread)
        {
            IsBackground = isWorkerThread;
        }

        /// <summary>
        /// True, if the plugin should be invoked on a worker thread.
        /// </summary>
        public bool IsBackground { get; set; }
    }
}
