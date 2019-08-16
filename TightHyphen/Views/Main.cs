using Nikse.SubtitleEdit.PluginLogic.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic.Views
{
    public partial class Main : Form
    {
        private readonly Subtitle _subtitle;

        public Main(Subtitle subtitle)
        {
            InitializeComponent();
            _subtitle = subtitle;

            buttonOK.Click += (s, e) =>
            {
                // apply fixes
                Invoke((p, inp) =>
                {
                    p.Text = inp;
                });

                Result = _subtitle.ToText();
                DialogResult = DialogResult.OK;
            };

            buttonCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
            };

            1.ToString("", null);
            // generate preview
            Invoke((p, inp) =>
            {
                listViewPreview.Items.Add(new ListViewItem($"{p.Number:000}")
                {
                    SubItems = { UIUtils.GetListViewTextFromString(p.Text), UIUtils.GetListViewTextFromString(inp) }
                });
            });

            // handles columns adjust
            Resize += (s, e) =>
            {
                int width = listViewPreview.Width - listViewPreview.Columns[0].Width;
                int halfWidth = width / 2;

                listViewPreview.BeginUpdate();

                int count = listViewPreview.Columns.Count;
                for (int i = 1; i < count; i++)
                {
                    ColumnHeader ch = listViewPreview.Columns[i];
                    ch.Width = halfWidth;
                }

                listViewPreview.EndUpdate();
            };

            linkLabelDonate.Click += delegate
            {
                Process.Start(StringUtils.DonateUrl);
            };
        }

        private void Invoke(Action<Paragraph, string> action)
        {
            foreach (Paragraph pgh in _subtitle.Paragraphs)
            {
                string cleanText = HtmlUtils.RemoveTags(pgh.Text, true);
                string text = RemoveSpaceAfterHyphen(pgh.Text);

                if (text.Length != pgh.Text.Length)
                {
                    action(pgh, text);
                }
            }
            labelStatus.Text = $"Total: {listViewPreview.Items.Count}/{_subtitle.Paragraphs.Count}";
        }

        private string RemoveSpaceAfterHyphen(string text)
        {
            string[] lines = text.SplitToLines();
            int len = lines.Length;
            for (int i = 0; i < len; i++)
            {
                string line = lines[i];

                if (!ShouldRemoveHyphen(line))
                {
                    continue;
                }

                int idx = line.IndexOf("- ", StringComparison.Ordinal);
                line = $"{line.Substring(0, idx)}-{line.Substring(idx + 1).TrimStart()}";
                lines[i] = line;
            }

            return string.Join(Environment.NewLine, lines);
        }

        private bool ShouldRemoveHyphen(string input)
        {
            input = HtmlUtils.RemoveTags(input, true);
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            int idx = input.IndexOf("- ", StringComparison.Ordinal);

            // must be at the beginning
            return idx == 0;
        }

        public string Result { get; set; }
    }
}
