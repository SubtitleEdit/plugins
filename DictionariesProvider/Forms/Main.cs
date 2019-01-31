using Nikse.SubtitleEdit.PluginLogic.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Nikse.SubtitleEdit.PluginLogic.Forms
{
    public partial class Main : Form
    {
        private IList<DictionaryInfo> DictionaryInfo { get; set; }

        public Main()
        {
            InitializeComponent();

            DictionaryInfo = new List<DictionaryInfo>();

            // init ui

            linkLabelOpenDicFolder.Click += delegate
            {
                Process.Start(FileUtils.Dictionary);
            };

        }
    }
}
