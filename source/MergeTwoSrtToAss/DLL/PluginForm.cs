using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Nikse.SubtitleEdit.PluginLogic.Logic;
using Nikse.SubtitleEdit.PluginLogic.Logic.SubtitleFormats;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class PluginForm : Form
    {
        public string FixedSubtitle { get; set; }
        private Subtitle _subtitle1;
        private Subtitle _subtitle2;
        private int _selectedIndex = -1;
        private readonly Timer _previewTimer = new Timer();
        private string _header;
        private bool _isSubStationAlpha;
        private readonly object _lockObj = new object();

        private string _textOne = "File one style example!";
        private string _textTwo = "File two style example!";

        public PluginForm()
        {
            InitializeComponent();
        }

        public PluginForm(Subtitle subtitle, string name, string description, Form parentForm)
            : this()
        {
            _subtitle1 = subtitle;
            subtitleListView1.InitializeLanguage();
            subtitleListView2.InitializeLanguage();
            subtitleListView1.MultiSelect = false;
            subtitleListView2.MultiSelect = false;

            subtitleListView1.Fill(subtitle);

            _previewTimer.Interval = 200;
            _previewTimer.Tick += PreviewTimerTick;

            comboBoxFontName1.Items.Clear();
            comboBoxFontName2.Items.Clear();
            foreach (var x in FontFamily.Families)
            {
                comboBoxFontName1.Items.Add(x.Name);
                comboBoxFontName2.Items.Add(x.Name);
            }
            openFileDialogSubtitle.Filter = "SubRip/ASS/SSA files|*.srt;*.ass;*.ssa";
            SetHeader();
        }

        private void LoadSettingsIfThereIs()
        {
            var fileName = GetSettingsFileName();
            if (File.Exists(fileName))
            {
                var doc = XDocument.Parse(File.ReadAllText(fileName));
                if (doc.Root != null)
                {
                    var xElement = doc.Root.Element("FontName1");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                            comboBoxFontName1.Text = xElement.Value;
                    }
                    xElement = doc.Root.Element("FontName2");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                            comboBoxFontName2.Text = xElement.Value;
                    }

                    xElement = doc.Root.Element("FontSize1");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                            numericUpDownFontSize1.Value = int.Parse(xElement.Value);
                    }
                    xElement = doc.Root.Element("FontSize2");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                            numericUpDownFontSize2.Value = int.Parse(xElement.Value);
                    }

                    xElement = doc.Root.Element("Bold1");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                            checkBoxFontBold1.Checked = Convert.ToBoolean(xElement.Value);
                    }
                    xElement = doc.Root.Element("Italic1");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                            checkBoxFontItalic1.Checked = Convert.ToBoolean(xElement.Value);
                    }
                    xElement = doc.Root.Element("Underline1");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                            checkBoxFontUnderline1.Checked = Convert.ToBoolean(xElement.Value);
                    }

                    xElement = doc.Root.Element("Bold2");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                            checkBoxFontBold2.Checked = Convert.ToBoolean(xElement.Value);
                    }
                    xElement = doc.Root.Element("Italic2");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                            checkBoxFontItalic2.Checked = Convert.ToBoolean(xElement.Value);
                    }
                    xElement = doc.Root.Element("Underline2");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                            checkBoxFontUnderline2.Checked = Convert.ToBoolean(xElement.Value);
                    }

                    xElement = doc.Root.Element("AlignTop1");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                            radioButtonAlignTop1.Checked = Convert.ToBoolean(xElement.Value);
                        radioButtonAlignBottom1.Checked = !radioButtonAlignTop1.Checked;
                    }
                    xElement = doc.Root.Element("AlignTop2");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                            radioButtonAlignTop2.Checked = Convert.ToBoolean(xElement.Value);
                        radioButtonAlignBottom2.Checked = !radioButtonAlignTop2.Checked;
                    }

                    xElement = doc.Root.Element("PrimaryColor1");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                            panelPrimaryColor1.BackColor = ColorTranslator.FromHtml(xElement.Value);
                    }
                    xElement = doc.Root.Element("PrimaryColor2");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                            panelPrimaryColor2.BackColor = ColorTranslator.FromHtml(xElement.Value);
                    }

                    xElement = doc.Root.Element("OutlineColor1");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                            panelOutlineColor1.BackColor = ColorTranslator.FromHtml(xElement.Value);
                    }
                    xElement = doc.Root.Element("OutlineColor2");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                            panelOutlineColor2.BackColor = ColorTranslator.FromHtml(xElement.Value);
                    }

                    xElement = doc.Root.Element("OutputFormat");
                    if (xElement != null)
                    {
                        if (!string.IsNullOrEmpty(xElement.Value))
                        {
                            for (int index = 0; index < comboBoxFormat.Items.Count; index++)
                            {
                                var item = comboBoxFormat.Items[index];
                                if (item.ToString() == xElement.Value)
                                {
                                    comboBoxFormat.SelectedIndex = index;
                                    break;
                                }
                            }
                        }
                    }

                }
                comboBoxFormat_SelectedIndexChanged(null, null);
            }
        }

        private string GetSettingsFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path != null && path.StartsWith("file:\\"))
                path = path.Remove(0, 6);
            if (path != null)
                path = Path.Combine(path, "Plugins");
            if (path == null || !Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "MergeTwoSrtToAss.xml");
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (_subtitle1 == null || _subtitle1.Paragraphs.Count == 0 ||
                _subtitle2 == null || _subtitle1.Paragraphs.Count == 2)
            {
                MessageBox.Show("Please load two subtitles");
                return;
            }
            Merge();
            DialogResult = DialogResult.OK;
        }

        private void Merge()
        {
            var subtitle = new Subtitle { Header = _header };
            foreach (var paragraph in _subtitle1.Paragraphs)
            {
                paragraph.Extra = "style1";
                subtitle.InsertParagraphInCorrectTimeOrder(paragraph);
            }
            foreach (var paragraph in _subtitle2.Paragraphs)
            {
                paragraph.Extra = "style2";
                subtitle.InsertParagraphInCorrectTimeOrder(paragraph);
            }


            if (_isSubStationAlpha)
            {
                FixedSubtitle = new SubStationAlpha().ToText(subtitle, string.Empty);
            }
            else
            {
                FixedSubtitle = new AdvancedSubStationAlpha().ToText(subtitle, string.Empty);
            }
        }

        private void PluginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void subtitleListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count == 0)
                return;

            _selectedIndex = subtitleListView1.SelectedItems[0].Index;
            Paragraph p = _subtitle1.GetParagraphOrDefault(_selectedIndex);
            if (p != null && !string.IsNullOrWhiteSpace(Utilities.RemoveHtmlTags(p.Text)))
            {
                _textOne = Utilities.RemoveHtmlTags(p.Text);
                GeneratePreview();
            }
        }

        private void subtitleListView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (subtitleListView2.SelectedItems.Count == 0)
                return;

            var selectedIndex = subtitleListView2.SelectedItems[0].Index;
            Paragraph p = _subtitle2.GetParagraphOrDefault(selectedIndex);
            if (p != null && !string.IsNullOrWhiteSpace(Utilities.RemoveHtmlTags(p.Text)))
            {
                _textTwo = Utilities.RemoveHtmlTags(p.Text);
                GeneratePreview();
            }
        }

        private void PluginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                var fileName = GetSettingsFileName();
                var document = new XDocument(
                    new XDeclaration("1.0", "utf8", "yes"),
                    new XComment("This XML file defines the settings for the Subtitle Edit Merge2Srt2Ass plugin"),
                    new XComment("XML file generated by Merge2Srt2Ass plugin"),
                    new XElement("Merge2Srt2Ass",
                        new XElement("FontName1", comboBoxFontName1.Text),
                        new XElement("FontName2", comboBoxFontName2.Text),
                        new XElement("FontSize1", numericUpDownFontSize1.Value.ToString(CultureInfo.InvariantCulture)),
                        new XElement("FontSize2", numericUpDownFontSize2.Value.ToString(CultureInfo.InvariantCulture)),
                        new XElement("Bold1", checkBoxFontBold1.Checked.ToString()),
                        new XElement("Italic1", checkBoxFontItalic1.Checked.ToString()),
                        new XElement("Underline1", checkBoxFontUnderline1.Checked.ToString()),
                        new XElement("Bold2", checkBoxFontBold2.Checked.ToString()),
                        new XElement("Italic2", checkBoxFontItalic2.Checked.ToString()),
                        new XElement("Underline2", checkBoxFontUnderline2.Checked.ToString()),
                        new XElement("AlignTop1", radioButtonAlignTop1.Checked.ToString()),
                        new XElement("AlignTop2", radioButtonAlignTop2.Checked.ToString()),
                        new XElement("PrimaryColor1", ColorTranslator.ToHtml(panelPrimaryColor1.BackColor)),
                        new XElement("PrimaryColor2", ColorTranslator.ToHtml(panelPrimaryColor2.BackColor)),
                        new XElement("OutlineColor1", ColorTranslator.ToHtml(panelOutlineColor1.BackColor)),
                        new XElement("OutlineColor2", ColorTranslator.ToHtml(panelOutlineColor2.BackColor)),
                        new XElement("OutputFormat", comboBoxFormat.Text)
                    )
                );
                document.Save(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonOpenFile1_Click(object sender, EventArgs e)
        {
            openFileDialogSubtitle.Title = "Open subtitle file 1";
            openFileDialogSubtitle.FileName = string.Empty;
            if (openFileDialogSubtitle.ShowDialog(this) == DialogResult.OK)
            {
                OpenFile1(openFileDialogSubtitle.FileName);
            }
        }

        private void OpenFile1(string fileName)
        {
            var lines = new List<string>();
            foreach (var line in File.ReadAllLines(fileName))
            {
                lines.Add(line);
            }
            SubtitleFormat format = new SubRip();
            if (!format.IsMine(lines, fileName))
                format = new AdvancedSubStationAlpha();
            if (!format.IsMine(lines, fileName))
                format = new SubStationAlpha();
            if (format.IsMine(lines, fileName))
            {
                textBox1.Text = fileName;
                _subtitle1 = new Subtitle();
                format.LoadSubtitle(_subtitle1, lines, fileName);
                subtitleListView1.Fill(_subtitle1);
                if (_subtitle1.Paragraphs.Count > 0)
                    subtitleListView1.SelectIndexAndEnsureVisible(0);
                GeneratePreview();
            }
        }

        private void buttonOpenFile2_Click(object sender, EventArgs e)
        {
            openFileDialogSubtitle.Title = "Open subtitle file 2";
            openFileDialogSubtitle.FileName = string.Empty;
            if (openFileDialogSubtitle.ShowDialog(this) == DialogResult.OK)
            {
                OpenFile2(openFileDialogSubtitle.FileName);
            }
        }

        private void OpenFile2(string fileName)
        {
            var lines = new List<string>();
            foreach (var line in File.ReadAllLines(fileName))
            {
                lines.Add(line);
            }
            SubtitleFormat format = new SubRip();
            if (!format.IsMine(lines, fileName))
                format = new AdvancedSubStationAlpha();
            if (!format.IsMine(lines, fileName))
                format = new SubStationAlpha();

            if (format.IsMine(lines, fileName))
            {
                textBox2.Text = fileName;
                _subtitle2 = new Subtitle();
                format.LoadSubtitle(_subtitle2, lines, fileName);
                subtitleListView2.Fill(_subtitle2);
                if (_subtitle2.Paragraphs.Count > 0)
                    subtitleListView1.SelectIndexAndEnsureVisible(0);
                GeneratePreview();
            }
        }

        private void PreviewTimerTick(object sender, EventArgs e)
        {
            _previewTimer.Stop();
            GeneratePreviewReal();
        }

        private void GeneratePreview()
        {
            if (_previewTimer.Enabled)
            {
                _previewTimer.Stop();
                _previewTimer.Start();
            }
            else
            {
                _previewTimer.Start();
            }
        }

        protected void GeneratePreviewReal()
        {
            if (pictureBoxPreview.Image != null)
                pictureBoxPreview.Image.Dispose();
            var bmp = new Bitmap(pictureBoxPreview.Width, pictureBoxPreview.Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {

                // Draw background
                const int rectangleSize = 9;
                for (int y = 0; y < bmp.Height; y += rectangleSize)
                {
                    for (int x = 0; x < bmp.Width; x += rectangleSize)
                    {
                        Color c = Color.WhiteSmoke;
                        if (y % (rectangleSize * 2) == 0)
                        {
                            if (x % (rectangleSize * 2) == 0)
                                c = Color.LightGray;
                        }
                        else
                        {
                            if (x % (rectangleSize * 2) != 0)
                                c = Color.LightGray;
                        }
                        g.FillRectangle(new SolidBrush(c), x, y, rectangleSize, rectangleSize);
                    }
                }

                DrawText(_textOne, g, bmp, comboBoxFontName1, numericUpDownFontSize1.Value, checkBoxFontBold1, radioButtonAlignTop1, checkBoxFontItalic1, checkBoxFontUnderline1, panelPrimaryColor1.BackColor, panelOutlineColor1.BackColor);
                DrawText(_textTwo, g, bmp, comboBoxFontName2, numericUpDownFontSize2.Value, checkBoxFontBold2, radioButtonAlignTop2, checkBoxFontItalic2, checkBoxFontUnderline2, panelPrimaryColor2.BackColor, panelOutlineColor2.BackColor);
            }
            pictureBoxPreview.Image = bmp;
        }

        private void DrawText(string text, Graphics g, Bitmap bmp, ComboBox comboboxfontName, decimal fontSize, CheckBox checkBoxFontBold, RadioButton radioButtonAlignTop, CheckBox checkBoxFontItalic, CheckBox checkBoxFontUnderline, Color fontColor, Color backColor)
        {
            Font font;
            try
            {
                font = new Font(comboboxfontName.Text, (float)fontSize);
            }
            catch
            {
                font = new Font(Font, FontStyle.Regular);
            }
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
            var path = new GraphicsPath();

            bool newLine = false;
            var sb = new StringBuilder();
            sb.Append(text);

            var measuredWidth = TextDraw.MeasureTextWidth(font, sb.ToString(), checkBoxFontBold.Checked) + 1;
            var measuredHeight = TextDraw.MeasureTextHeight(font, sb.ToString(), checkBoxFontBold.Checked) + 1;

            //if (radioButtonTopLeft.Checked || radioButtonMiddleLeft.Checked || radioButtonBottomLeft.Checked)
            //    left = (float)numericUpDownMarginLeft.Value;
            //else if (radioButtonTopRight.Checked || radioButtonMiddleRight.Checked || radioButtonBottomRight.Checked)
            //    left = bmp.Width - (measuredWidth + ((float)numericUpDownMarginRight.Value));
            //else
            float left = ((float)(bmp.Width - measuredWidth * 0.8 + 15) / 2);

            float top;
            if (radioButtonAlignTop.Checked)
                top = 10;
            //else if (radioButtonMiddleLeft.Checked || radioButtonMiddleCenter.Checked || radioButtonMiddleRight.Checked)
            //    top = (bmp.Height - measuredHeight) / 2;
            else
                top = bmp.Height - measuredHeight - 10;
            //top -= (int)numericUpDownShadowWidth.Value;
            //if (radioButtonTopCenter.Checked || radioButtonMiddleCenter.Checked || radioButtonBottomCenter.Checked)
            //    left -= (int)(numericUpDownShadowWidth.Value / 2);

            const int leftMargin = 0;
            int pathPointsStart = -1;

            //if (false) //radioButtonOpaqueBox.Checked)
            //{
            //    g.FillRectangle(new SolidBrush(backColor), left, top, measuredWidth + 3, measuredHeight + 3);
            //}

            TextDraw.DrawText(font, sf, path, sb, checkBoxFontItalic.Checked, checkBoxFontBold.Checked, checkBoxFontUnderline.Checked, left, top, ref newLine, leftMargin, ref pathPointsStart);

            var outline = (int)numericUpDownOutline1.Value;

            // draw shadow
            if (numericUpDownShadowWidth1.Value > 0) // && radioButtonOutline1.Checked)
            {
                var shadowPath = (GraphicsPath)path.Clone();
                for (int i = 0; i < (int)numericUpDownShadowWidth1.Value; i++)
                {
                    var translateMatrix = new Matrix();
                    translateMatrix.Translate(1, 1);
                    shadowPath.Transform(translateMatrix);

                    using (var p1 = new Pen(Color.FromArgb(250, Color.Black), outline)) //(()) panelBackColor.BackColor), outline);
                        g.DrawPath(p1, shadowPath);
                }
            }

            if (outline > 0)
            {
                g.DrawPath(new Pen(backColor, outline), path);
            }
            g.FillPath(new SolidBrush(fontColor), path);
        }

        private void PluginForm_Load(object sender, EventArgs e)
        {
            comboBoxFontName1.Text = "Arial";
            numericUpDownFontSize1.Value = 20;

            comboBoxFontName2.Text = "Arial";
            numericUpDownFontSize2.Value = 20;

            panelPrimaryColor1.BackColor = Color.White;
            panelOutlineColor1.BackColor = Color.Black;

            panelPrimaryColor2.BackColor = Color.White;
            panelOutlineColor2.BackColor = Color.Black;

            radioButtonAlignTop1.Checked = true;
            radioButtonAlignBottom1.Checked = false;

            radioButtonAlignTop2.Checked = false;
            radioButtonAlignBottom2.Checked = true;

            comboBoxFormat.SelectedIndex = 0;

            LoadSettingsIfThereIs();

            GeneratePreview();
        }

        private string GetSsaColorString(Color c)
        {
            if (_isSubStationAlpha)
                return Color.FromArgb(0, c.B, c.G, c.R).ToArgb().ToString(CultureInfo.InvariantCulture);
            return string.Format("&H00{0:X2}{1:X2}{2:X2}", c.B, c.G, c.R);
        }

        private void buttonPrimaryColor1_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelPrimaryColor1.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelPrimaryColor1.BackColor = colorDialogSSAStyle.Color;
                SetSsaStyle("style1", "primarycolour", GetSsaColorString(colorDialogSSAStyle.Color));
                GeneratePreview();
            }
        }

        private void buttonOutlineColor1_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelOutlineColor1.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelOutlineColor1.BackColor = colorDialogSSAStyle.Color;
                SetSsaStyle("style1", _isSubStationAlpha ? "tertiarycolour" : "outlinecolour", GetSsaColorString(colorDialogSSAStyle.Color));
                GeneratePreview();
            }
        }

        private void buttonPrimaryColor2_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelPrimaryColor2.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelPrimaryColor2.BackColor = colorDialogSSAStyle.Color;
                SetSsaStyle("style2", "primarycolour", GetSsaColorString(colorDialogSSAStyle.Color));
                GeneratePreview();
            }
        }

        private void buttonOutlineColor2_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelOutlineColor2.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelOutlineColor2.BackColor = colorDialogSSAStyle.Color;
                SetSsaStyle("style2", _isSubStationAlpha ? "tertiarycolour" : "outlinecolour", GetSsaColorString(colorDialogSSAStyle.Color));
                GeneratePreview();
            }
        }

        private void SetSsaStyle(string styleName, string propertyName, string propertyValue)
        {
            lock (_lockObj)
            {
                int propertyIndex = -1;
                int nameIndex = -1;
                var sb = new StringBuilder();
                foreach (var line in _header.Split(Utilities.NewLineChars, StringSplitOptions.None))
                {
                    string s = line.Trim().ToLower();
                    if (s.StartsWith("format:", StringComparison.Ordinal))
                    {
                        if (line.Length > 10)
                        {
                            var format = line.ToLower().Substring(8).Split(',');
                            for (int i = 0; i < format.Length; i++)
                            {
                                string f = format[i].Trim().ToLower();
                                if (f == "name")
                                    nameIndex = i;
                                if (f == propertyName)
                                    propertyIndex = i;
                            }
                        }
                        sb.AppendLine(line);
                    }
                    else if (s.Replace(" ", string.Empty).StartsWith("style:", StringComparison.Ordinal))
                    {
                        if (line.Length > 10)
                        {
                            bool correctLine = false;
                            var format = line.Substring(6).Split(',');
                            for (int i = 0; i < format.Length; i++)
                            {
                                string f = format[i].Trim();
                                if (i == nameIndex)
                                    correctLine = f.Equals(styleName, StringComparison.OrdinalIgnoreCase);
                            }
                            if (correctLine)
                            {
                                sb.Append(line.Substring(0, 6) + " ");
                                format = line.Substring(6).Split(',');
                                for (int i = 0; i < format.Length; i++)
                                {
                                    string f = format[i].Trim();
                                    if (i == propertyIndex)
                                    {
                                        //                                        MessageBox.Show("Style:" + styleName + ": changing property " + propertyName + " " + f + " to " + propertyValue);
                                        sb.Append(propertyValue);
                                    }
                                    else
                                    {
                                        sb.Append(f);
                                    }
                                    if (i < format.Length - 1)
                                        sb.Append(',');
                                }
                                sb.AppendLine();
                            }
                            else
                            {
                                sb.AppendLine(line);
                            }
                        }
                        else
                        {
                            sb.AppendLine(line);
                        }
                    }
                    else
                    {
                        sb.AppendLine(line);
                    }
                }
                _header = sb.ToString().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            }
        }

        private void checkBoxFontBold1_CheckedChanged(object sender, EventArgs e)
        {
            SetSsaStyle("style1", "bold", checkBoxFontBold1.Checked ? "1" : "0");
            GeneratePreview();
        }


        private void checkBoxFontItalic1_CheckedChanged(object sender, EventArgs e)
        {
            SetSsaStyle("style1", "italic", checkBoxFontItalic1.Checked ? "1" : "0");
            GeneratePreview();
        }

        private void checkBoxFontUnderline1_CheckedChanged(object sender, EventArgs e)
        {
            SetSsaStyle("style1", "underline", checkBoxFontUnderline1.Checked ? "1" : "0");
            GeneratePreview();
        }

        private void checkBoxFontBold2_CheckedChanged(object sender, EventArgs e)
        {
            SetSsaStyle("style2", "bold", checkBoxFontBold2.Checked ? "1" : "0");
            GeneratePreview();
        }

        private void checkBoxFontItalic2_CheckedChanged(object sender, EventArgs e)
        {
            SetSsaStyle("style2", "italic", checkBoxFontItalic2.Checked ? "1" : "0");
            GeneratePreview();
        }

        private void checkBoxFontUnderline2_CheckedChanged(object sender, EventArgs e)
        {
            SetSsaStyle("style2", "underline", checkBoxFontUnderline2.Checked ? "1" : "0");
            GeneratePreview();
        }

        private void radioButtonAlignTop1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonAlignBottom1.Checked)
            {
                SetSsaStyle("style1", "alignment", "2");
            }
            else
            {
                SetSsaStyle("style1", "alignment", _isSubStationAlpha ? "6" : "8");
            }
            GeneratePreview();
        }

        private void radioButtonAlignBottom1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonAlignBottom1.Checked)
            {
                SetSsaStyle("style1", "alignment", "2");
            }
            else
            {
                SetSsaStyle("style1", "alignment", _isSubStationAlpha ? "6" : "8");
            }
            GeneratePreview();
        }

        private void radioButtonAlignTop2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonAlignBottom2.Checked)
            {
                SetSsaStyle("style2", "alignment", "2");
            }
            else
            {
                SetSsaStyle("style2", "alignment", _isSubStationAlpha ? "6" : "8");
            }
            GeneratePreview();
        }

        private void radioButtonAlignBottom_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonAlignBottom2.Checked)
            {
                SetSsaStyle("style2", "alignment", "2");
            }
            else
            {
                SetSsaStyle("style2", "alignment", _isSubStationAlpha ? "6" : "8");
            }
            GeneratePreview();
        }

        private void numericUpDownOutline1_ValueChanged(object sender, EventArgs e)
        {
            SetSsaStyle("style1", "outline", numericUpDownOutline1.Value.ToString(CultureInfo.InvariantCulture));
            GeneratePreview();
        }

        private void numericUpDownShadowWidth1_ValueChanged(object sender, EventArgs e)
        {
            SetSsaStyle("style1", "shadow", numericUpDownShadowWidth1.Value.ToString(CultureInfo.InvariantCulture));
            GeneratePreview();
        }

        private void numericUpDownOutline2_ValueChanged(object sender, EventArgs e)
        {
            SetSsaStyle("style2", "outline", numericUpDownOutline2.Value.ToString(CultureInfo.InvariantCulture));
            GeneratePreview();
        }

        private void numericUpDownShadowWidth2_ValueChanged(object sender, EventArgs e)
        {
            SetSsaStyle("style2", "shadow", numericUpDownShadowWidth2.Value.ToString(CultureInfo.InvariantCulture));
            GeneratePreview();
        }

        private void numericUpDownFontSize1_ValueChanged(object sender, EventArgs e)
        {
            SetSsaStyle("style1", "fontsize", numericUpDownFontSize1.Value.ToString(CultureInfo.InvariantCulture));
            GeneratePreview();
        }

        private void numericUpDownFontSize2_ValueChanged(object sender, EventArgs e)
        {
            SetSsaStyle("style2", "fontsize", numericUpDownFontSize2.Value.ToString(CultureInfo.InvariantCulture));
            GeneratePreview();
        }

        private void PluginForm_ResizeEnd(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void PluginForm_Resize(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void comboBoxFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            _isSubStationAlpha = comboBoxFormat.SelectedIndex == 1;
            SetHeader();
            comboBoxFontName1_TextChanged(null, null);
            comboBoxFontName2_TextChanged(null, null);
            numericUpDownFontSize1_ValueChanged(null, null);
            numericUpDownFontSize2_ValueChanged(null, null);
            checkBoxFontBold1_CheckedChanged(null, null);
            checkBoxFontBold2_CheckedChanged(null, null);
            checkBoxFontItalic1_CheckedChanged(null, null);
            checkBoxFontItalic2_CheckedChanged(null, null);
            checkBoxFontUnderline1_CheckedChanged(null, null);
            checkBoxFontUnderline2_CheckedChanged(null, null);
            radioButtonAlignTop1_CheckedChanged(null, null);
            radioButtonAlignTop2_CheckedChanged(null, null);

            SetSsaStyle("style1", "primarycolour", GetSsaColorString(panelPrimaryColor1.BackColor));
            SetSsaStyle("style2", "primarycolour", GetSsaColorString(panelPrimaryColor2.BackColor));

            SetSsaStyle("style1", _isSubStationAlpha ? "tertiarycolour" : "outlinecolour", GetSsaColorString(panelOutlineColor1.BackColor));
            SetSsaStyle("style2", _isSubStationAlpha ? "tertiarycolour" : "outlinecolour", GetSsaColorString(panelOutlineColor2.BackColor));

            numericUpDownOutline1_ValueChanged(null, null);
            numericUpDownOutline2_ValueChanged(null, null);
        }

        private void SetHeader()
        {
            if (!_isSubStationAlpha)
            {
                _header = @"[Script Info]
; This is an Advanced Sub Station Alpha v4+ script.
Title: 
ScriptType: v4.00+
Collisions: Normal
PlayDepth: 0

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: style1,Tahoma,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,1,8,10,10,10,1
Style: style2,Tahoma,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1

[Events]";
            }
            else
            {
                _header = @"[Script Info]
; This is a Sub Station Alpha v4 script.
Title: a
ScriptType: v4.00
Collisions: Normal
PlayDepth: 0

[V4 Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding
Style: style1,tahoma,20,-1,-256,-16777216,-16777216,-1,0,1,2,1,6,10,10,10,0,1
Style: style2,tahoma,20,-1,-256,-16777216,-16777216,-1,0,1,2,1,2,10,10,10,0,1

[Events]";
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void subtitleListView1_DragDrop(object sender, DragEventArgs e)
        {
            var dragAndDropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (dragAndDropFiles.Length == 1)
            {
                OpenFile1(dragAndDropFiles[0]);
            }
            else
            {
                MessageBox.Show("Drop only one file");
            }
        }

        private void subtitleListView2_DragDrop(object sender, DragEventArgs e)
        {
            var dragAndDropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (dragAndDropFiles.Length == 1)
            {
                OpenFile2(dragAndDropFiles[0]);
            }
            else
            {
                MessageBox.Show("Drop only one file");
            }
        }

        private void subtitleListView1_DragEnter(object sender, DragEventArgs e)
        {
            // make sure they're actually dropping files (not text or anything else)
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void subtitleListView2_DragEnter(object sender, DragEventArgs e)
        {
            // make sure they're actually dropping files (not text or anything else)
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void comboBoxFontName2_TextChanged(object sender, EventArgs e)
        {
            SetSsaStyle("style2", "fontname", comboBoxFontName2.Text);
            GeneratePreview();
        }

        private void comboBoxFontName1_TextChanged(object sender, EventArgs e)
        {
            SetSsaStyle("style1", "fontname", comboBoxFontName1.Text);
            GeneratePreview();
        }

    }
}