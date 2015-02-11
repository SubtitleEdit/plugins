using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class MainForm : Form
    {
        public string FixedSubtitle { get; private set; }

        private Subtitle _subtitle;
        private string _fileName;
        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(Subtitle sub, string fileName, string description)
            : this()
        {
            // TODO: Complete member initialization
            this._subtitle = sub;
            this._fileName = fileName;
        }
    }
}
