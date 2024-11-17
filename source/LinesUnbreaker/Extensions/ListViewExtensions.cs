using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic.Extensions;

public static class ListViewExtensions
{
    public static void CheckAll(this ListView listView)
    {
        listView.BeginUpdate();
        foreach (ListViewItem listViewItem in listView.Items)
        {
            listViewItem.Checked = true;
        }

        listView.EndUpdate();
    }

    public static void UncheckAll(this ListView listView)
    {
        listView.BeginUpdate();
        foreach (ListViewItem listViewItem in listView.Items)
        {
            listViewItem.Checked = false;
        }

        listView.EndUpdate();
    }

    public static void InvertCheck(this ListView listView)
    {
        listView.BeginUpdate();
        foreach (ListViewItem listViewItem in listView.Items)
        {
            listViewItem.Checked = !listViewItem.Checked;
        }

        listView.EndUpdate();
    }
}