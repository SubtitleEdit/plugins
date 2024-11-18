using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class Main : Form
    {
        private readonly Subtitle _subtitle;
        private readonly WordsHandler _wordHandler;
        private readonly WordsHandlerConfigs _configs;

        public Main(Subtitle sub, string name, string description)
        {
            InitializeComponent();

            _subtitle = sub;
            _configs = new WordsHandlerConfigs();
            _wordHandler = new WordsHandler(_configs);
        }

        private void buttonCleanup_Click(object sender, EventArgs e)
        {
            ReloadConfiguration();
            _wordHandler.CleanUpSubtitle(_subtitle);
            FixedSubtitle = _subtitle.ToText();
            DialogResult = DialogResult.OK;
        }

        private void ReloadConfiguration()
        {
            _configs.ColorRed = checkBoxColor.Checked;
        }

        public string FixedSubtitle { get; private set; }
    }
}
