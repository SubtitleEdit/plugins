using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.PluginLogic
{
    interface IConfigurable
    {
        void LoadConfigs();

        void SaveConfigs();
    }
}
