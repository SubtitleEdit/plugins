using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AssaDraw.ColorPicker;
using AssaDraw.Logic;

namespace AssaDraw
{
    public sealed partial class SetColor : Form
    {
        public Color Color { get; set; }
        public List<int> Layers { get; set; }

        public SetColor(List<DrawShape> drawShapes, Color backColor, string title)
        {
            InitializeComponent();
            Text = title;
            FillStyles(drawShapes);
            panelColorPicker.BackColor = backColor;
        }

        private void FillStyles(List<DrawShape> drawShapes)
        {
            listViewStyles.Items.Clear();
            listViewStyles.Items.AddRange(drawShapes.OrderBy(p => p.Layer).Select(p => new ListViewItem { Text = p.Layer.ToString(), Tag = p.Layer }).ToArray());
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            Color = panelColorPicker.BackColor;
            Layers = new List<int>();
            foreach (ListViewItem item in listViewStyles.Items)
            {
                if (item.Checked)
                {
                    Layers.Add((int)item.Tag);
                }
            }
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonColor_Click(object sender, System.EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelColorPicker.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelColorPicker.BackColor = colorChooser.Color;
                }
            }
        }
    }
}
