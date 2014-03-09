using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class Haxor : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {

        string IPlugin.Name
        {
            get { return "Haxor"; }
        }

        string IPlugin.Text
        {
            get { return "Translate to haxor"; }
        }

        decimal IPlugin.Version
        {
            get { return 1.0M; }
        }

        string IPlugin.Description
        {
            get { return "Translates to haxor"; }
        }

        string IPlugin.ActionType // Can be one of these: file, tool, sync, translate, spellcheck
        {
            get { return "translate"; }
        }

        string IPlugin.Shortcut
        {
            get { return string.Empty; }
        }

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            if (!string.IsNullOrEmpty(subtitle))
            {
                string from = "abcdefghijlkmnopqrstuvwxyz";
                string to = "4b©d3fgH!jlKmñ0pqr$tuvwx¥z";
                for (int i = 0; i < subtitle.Length; i++)
                {
                    int index = from.IndexOf(subtitle[i].ToString().ToLower());
                    try
                    {
                        if (index >= 0)
                        {
                            subtitle = subtitle.Remove(i, 1).Insert(i, to[index].ToString());
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message + "  i=" + i.ToString() + "  index="+ index);
                    }
                }
            }
            return subtitle;
        }

    }
}
