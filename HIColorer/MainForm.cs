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
    public partial class MainForm : Form
    {
        public string FixedSubtitle { get; private set; }

        private Color _narratorColor = Color.Empty;
        private Color _moodsColor = Color.Empty;
        private Subtitle _subtitle;

        public MainForm()
        {
            InitializeComponent();
        }

        internal MainForm(Subtitle sub, string name)
            : this()
        {
            this._subtitle = sub;
        }

        private void buttonNarratorColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _narratorColor = colorDialog1.Color;
                this.labelNarratorsColor.BackColor = _narratorColor;
            }
        }

        private void buttonMoodsColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _moodsColor = colorDialog1.Color;
                this.labelMoodsColor.BackColor = _moodsColor;
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (!checkBoxEnabledMoods.Checked && !checkBoxEnabledNarrator.Checked)
                DialogResult = System.Windows.Forms.DialogResult.Cancel;

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void SetColor()
        {
            if (_subtitle == null || _subtitle.Paragraphs.Count == 0)
                return;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                string text = p.Text;
                string old = text;


            }
        }

        private void RemoveColor(Paragraph p)
        {

        }
    }
}
