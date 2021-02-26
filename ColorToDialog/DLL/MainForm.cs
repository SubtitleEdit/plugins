using ColorToDialog.Logic;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace ColorToDialog
{
    public partial class MainForm : Form
    {
        private Subtitle _subtitle;
        private string _dash;
        private readonly Subtitle _subtitleOriginal;

        public string FixedSubtitle { get; private set; }

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(Subtitle sub, string title, string description, Form parentForm)
            : this()
        {
            Text = title;
            _subtitle = sub;
            _subtitleOriginal = new Subtitle(sub);
            _dash = "- ";
            comboBoxDash.SelectedIndex = 0;
            RestoreSettings();
            GeneratePreview();
            listView1_Resize(null, null);
        }

        public sealed override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        private void GeneratePreview()
        {
            var dash = _dash;
            if (_subtitle == null || string.IsNullOrEmpty(dash))
            {
                return;
            }

            _dash = dash;
            try
            {
                _subtitle = new Subtitle(_subtitleOriginal);
                for (var index = 0; index < _subtitleOriginal.Paragraphs.Count; index++)
                {
                    var p = _subtitleOriginal.Paragraphs[index];
                    if (p.Text.Contains("<font ", StringComparison.OrdinalIgnoreCase))
                    {
                        var after = GetFixedText(p.Text);
                        if (after != p.Text)
                        {
                            AddToListView(p, after);
                            p.Text = after.Trim();
                        }
                    }
                }

                FixedSubtitle = _subtitle.ToText(new SubRip());

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private string GetFixedText(string text)
        {
            var result = new StringBuilder();
            var sb = new StringBuilder();
            var i = 0;
            var lastColor = Color.White;
            var currentColor = Color.White;
            var noColorOn = false;
            while (i < text.Length)
            {
                var s = text.Substring(i);
                if (s.StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
                {
                    noColorOn = false;
                    result.Append(sb);
                    sb.Clear();
                    lastColor = currentColor;
                    currentColor = GetColorFromFontString(s, Color.White);
                    if (i == 0)
                    {
                        lastColor = currentColor;
                    }

                    if (result.Length > 0 && (currentColor.R != lastColor.R || currentColor.G != lastColor.G || currentColor.B != lastColor.B))
                    {
                        var newText = AddStartDash(result.ToString());
                        result = new StringBuilder(newText.TrimEnd() + Environment.NewLine);
                        sb.Append(_dash);
                        lastColor = currentColor;
                    }
                    var len = s.IndexOf('>');
                    if (len < 0)
                    {
                        return text;
                    }

                    i += len + 1;
                }
                else if (s.StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                {
                    i += "</font>".Length;
                    noColorOn = true;
                }
                else
                {
                    if (noColorOn && text[i].ToString().Trim().Length > 0)
                    {
                        lastColor = currentColor;
                        currentColor = Color.White;
                    }

                    if (result.Length > 0 && (currentColor.R != lastColor.R || currentColor.G != lastColor.G || currentColor.B != lastColor.B))
                    {
                        var newText = AddStartDash(result.ToString());
                        result = new StringBuilder(newText.TrimEnd() + Environment.NewLine);
                        sb.AppendLine();
                        sb.Append(_dash);
                    }
                    sb.Append(text[i]);
                    i++;
                }
            }

            result.Append(sb);
            var resultString = result.ToString().Trim();
            while (resultString.Contains(Environment.NewLine + Environment.NewLine))
            {
                resultString = resultString.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            }

            while (resultString.Contains("-  "))
            {
                resultString = resultString.Replace("-  ", "- ");
            }

            return resultString;
        }

        private string AddStartDash(string text)
        {
            var s = text.Trim();
            if (!s.Contains(Environment.NewLine))
            {
                return _dash + s;
            }

            var idx = s.LastIndexOf(Environment.NewLine, StringComparison.Ordinal);
            if (idx >= 0)
            {
                s = s.Insert(idx + Environment.NewLine.Length, _dash);
            }

            return s;
        }

        private static Color GetColorFromFontString(string text, Color defaultColor)
        {
            string s = text.TrimEnd();
            int start = s.IndexOf("<font ", StringComparison.OrdinalIgnoreCase);
            var endFont = s.IndexOf("</font>", StringComparison.OrdinalIgnoreCase);
            if (endFont > 0)
            {
                s = s.Substring(0, endFont + "</font>".Length);
            }

            if (start >= 0 && s.EndsWith("</font>", StringComparison.OrdinalIgnoreCase))
            {
                int end = s.IndexOf('>', start);
                if (end > 0)
                {
                    string f = s.Substring(start, end - start);
                    if (f.Contains(" color=", StringComparison.OrdinalIgnoreCase))
                    {
                        int colorStart = f.IndexOf(" color=", StringComparison.OrdinalIgnoreCase);
                        if (s.IndexOf('"', colorStart + " color=".Length + 1) > 0)
                        {
                            end = s.IndexOf('"', colorStart + " color=".Length + 1);
                        }

                        s = s.Substring(colorStart, end - colorStart);
                        s = s.Replace(" color=", string.Empty);
                        s = s.Trim('\'').Trim('"').Trim('\'');
                        try
                        {
                            if (s.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
                            {
                                var arr = s.Remove(0, 4).TrimEnd(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                return Color.FromArgb(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
                            }
                            return ColorTranslator.FromHtml(s);
                        }
                        catch
                        {
                            return defaultColor;
                        }
                    }
                }
            }
            return defaultColor;
        }

        private void AddToListView(Paragraph p, string after)
        {
            var item = new ListViewItem(p.Number.ToString(CultureInfo.InvariantCulture)) { Tag = p };
            item.SubItems.Add(p.Text.Replace(Environment.NewLine, "<br />"));
            item.SubItems.Add(after.Replace(Environment.NewLine, "<br />"));
            listView1.Items.Add(item);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            SaveSettings();
            FixedSubtitle = _subtitle.ToText(new SubRip());
            DialogResult = DialogResult.OK;
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            var size = listView1.Width / 2 - 10;
            listView1.Columns[1].Width = size;
            listView1.Columns[2].Width = -2;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private string GetSettingsFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path != null && path.StartsWith("file:\\", StringComparison.Ordinal))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "ColorToDialog.xml");
        }

        private void RestoreSettings()
        {
            string fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.Load(fileName);
                _dash = doc.DocumentElement.SelectSingleNode("Dash").InnerText;
                if (_dash == "-")
                {
                    comboBoxDash.SelectedIndex = 1;
                }
                else
                {
                    comboBoxDash.SelectedIndex = 0;
                }
            }
            catch
            {
                comboBoxDash.SelectedIndex = 0;
                _dash = "- ";
            }
        }

        private void SaveSettings()
        {
            var fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml("<ColorToDialog><Dash/></ColorToDialog>");
                doc.DocumentElement.SelectSingleNode("Dash").InnerText = _dash;
                doc.Save(fileName);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            listView1.Columns[2].Width = -2;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MainForm_ResizeEnd(sender, e);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            MainForm_ResizeEnd(sender, e);
        }

        private void comboBoxDash_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxDash.SelectedIndex == 1)
            {
                _dash = "-";
            }
            else
            {
                _dash = "- ";
            }

            GeneratePreview();
        }

        private void comboBoxDash_TextChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }
    }
}