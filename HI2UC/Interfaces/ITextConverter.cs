using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.PluginLogic
{
    interface ITextConverter
    {
        string ToText(Subtitle subtitle, string title);
    }
}
