using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic.Extensions;

public static class ListViewItemCollectionExtensions
{
    public static IEnumerable<ListViewItem> CheckItems(this ListView.ListViewItemCollection listViewItemCollection)
    {
        return listViewItemCollection.OfType<ListViewItem>()
            .Where(item => item.Checked);
    }
}