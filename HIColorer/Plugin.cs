using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class HIColorer : IPlugin
    {
        string IPlugin.Name
        {
            get { throw new NotImplementedException(); }
        }

        string IPlugin.Text
        {
            get { throw new NotImplementedException(); }
        }

        decimal IPlugin.Version
        {
            get { throw new NotImplementedException(); }
        }

        string IPlugin.Description
        {
            get { throw new NotImplementedException(); }
        }

        string IPlugin.ActionType
        {
            get { throw new NotImplementedException(); }
        }

        string IPlugin.Shortcut
        {
            get { throw new NotImplementedException(); }
        }

        string IPlugin.DoAction(System.Windows.Forms.Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            throw new NotImplementedException();
        }
    }
}
