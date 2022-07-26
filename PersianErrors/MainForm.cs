using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ListViewSorter;
using System.Diagnostics;
using Nikse.SubtitleEdit.Core.Common;
using MsmhTools;
using System.Data;
using System.Text;
using System.Xml.Linq;
using System.Resources;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class MainForm : Form
    {
        private static readonly string exception1 = "Options";
        private static readonly string exception2 = "Formal to Slang";
        private static readonly string exception3 = "Guide";
        private bool applyClicked = false;
        private static Subtitle SubCurrent { get; set; }
        public string SaveSubtitle { get; set; }
        private int _totalFixes;
        // Break from loops on Form Closing
        private bool closeForm = false;
        private static CancellationTokenSource SourceApply = null;
        private static Task TaskApply { get; set; }
        // Sort Column Line#
        private ListViewColumnSorter lvwColumnSorter = null;
        private readonly Dictionary<string, Regex> CompiledRegExList = new Dictionary<string, Regex>();
        public static DataSet DataSetSettings = new DataSet();
        private readonly static Label WaitLabel = new Label();

        public MainForm()
        {
            InitializeComponent();
            string AV = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Text = "Persian Subtitle Fixes (Persian Common Errors)" + " - v" + AV;
            StartPosition = FormStartPosition.CenterScreen;
            buttonApply.SetToolTip("Info", "You can apply multiple times.");

            // DataSet Name
            DataSetSettings.DataSetName = "Settings";

            // WaitLabel
            Rectangle screenRectangle = RectangleToScreen(ClientRectangle);
            int titleHeight = screenRectangle.Top - Top;
            WaitLabel.AutoSize = false;
            WaitLabel.Size = new Size(150, 75);
            WaitLabel.TextAlign = ContentAlignment.MiddleCenter;
            WaitLabel.Dock = DockStyle.None;
            WaitLabel.Text = "Please Wait\nمنتظر بمانید";
            WaitLabel.Font = new Font("Tahoma", 14, FontStyle.Regular);
            Color bgColorWL = Color.FromArgb(50, Color.Blue);
            WaitLabel.BackColor = bgColorWL;
            WaitLabel.Top = (ClientSize.Height - WaitLabel.Size.Height) / 2 - titleHeight / 2;
            WaitLabel.Left = (ClientSize.Width - WaitLabel.Size.Width) / 2;
            WaitLabel.Anchor = AnchorStyles.Top;
            Controls.Add(WaitLabel);
            WaitLabel.SendToBack();

            FormClosing += MainForm_FormClosing;

            //// Sort Column Line#
            //lvwColumnSorter = new ListViewColumnSorter();
            //listViewFixes.ListViewItemSorter = lvwColumnSorter;
            //listViewFixes.Sorting = SortOrder.Ascending;
            //listViewFixes.AutoArrange = true;
            //lvwColumnSorter._SortModifier = ListViewColumnSorter.SortModifiers.SortByText;
        }

        public MainForm(Subtitle subtitle, string name, string description, Form parentForm) : this()
        {
            // TODO: Complete member initialization
            SubCurrent = subtitle;
            Resize += (s, arg) =>
            {
                listViewFixes.Columns[listViewFixes.Columns.Count - 1].Width = -2;
            };
            buttonOK.Enabled = false;
            GetGroupNames();
        }

        private static string FindWhatRule(string findWhat)
        {
            findWhat = findWhat.Replace("\"", "\\\"");
            findWhat = findWhat.Replace("\\\\\"", "\\\"");
            findWhat = findWhat.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            return findWhat;
        }

        private static string ReplaceWithRule(string replaceWith)
        {
            //if (replaceWith == "") replaceWith = " ";
            replaceWith = replaceWith.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            return replaceWith;
        }

        public void GetGroupNames()
        {
            string settingsFile = Tools.SettingsFilePath();
            bool isSettingsValid = Tools.IsSettingsValid(settingsFile);
            DataSet ds = new DataSet();
            if (isSettingsValid == true)
                ds = Tools.ToDataSet(settingsFile, XmlReadMode.Auto);

            // Get file content from Embedded Resource
            string fileContents = GetResourceTextFile("PersianErrors.multiple_replace.xml");
            var listGN = new List<string>();
            XmlDocument docGN = new XmlDocument();
            docGN.LoadXml(fileContents);
            XmlNodeList nodesGN = docGN.GetElementsByTagName("Group");
            for (int a = 0; a < nodesGN.Count; a++)
            {
                XmlNode node = nodesGN[a];
                //Console.WriteLine(node.Name);
                for (int b = 0; b < node.SelectNodes("Name").Count; b++)
                {
                    XmlNode childN = node.SelectNodes("Name")[b];
                    //Console.WriteLine("Group " + childN.Name + ": " + childN.InnerText);
                    listGN.Add(childN.InnerText);
                }
            }
            IDictionary<string, string> dicGN = new Dictionary<string, string>();
            int i = 0;
            int j = 0;
            for (; j < listGN.Count; j += 1, i += 1)
            {
                dicGN[i.ToString()] = listGN[j];
                //string checkBox[i] = listGN[j];

                CheckBox box = new CheckBox();
                bool state = true;
                if (ds.Tables["CheckBoxes"] != null && isSettingsValid == true)
                {
                    string s = string.Empty;
                    if (ds.Tables["CheckBoxes"].Columns.Contains(listGN[j]))
                        s = (string)ds.Tables["CheckBoxes"].Rows[0][listGN[j]];
                    if (s.IsBool() == true)
                        state = bool.Parse(s);
                    else
                        state = true;
                }
                box.Checked = state;
                box.Tag = i.ToString();
                box.Text = listGN[j];
                box.Name = "checkBox" + i.ToString();
                box.AutoSize = true;
                box.UseVisualStyleBackColor = true;
                box.Click += new EventHandler(CheckboxHandler);
                box.MouseHover += Box_MouseHover;
                box.MouseLeave += Box_MouseLeave;
                box.Location = new Point(10, (i + 1) * 20); //vertical
                                                            //box.Location = new Point(i * 50, 10); //horizontal
                groupBox1.Controls.Add(box); // Add CheckBoxes inside GroupBox1
            }
            for (int d = 0; d < groupBox1.Controls.Count; d++)
            {
                var c = groupBox1.Controls[d];
                if (c is CheckBox box)
                {
                    if (c.Text == exception1 || c.Text == exception2 || c.Text == exception3)
                    {
                        box.Checked = false;
                        box.Visible = false;
                    }
                    box.Enabled = true;
                }
            }
            buttonCheckAll.Enabled = false;
            buttonInvertCheck.Enabled = false;
            buttonApply.Visible = true;
            MinimumSize = new Size(1000, 600);   // Main form minimum size
            CheckboxHandler(null, null);
        }

        public void StartThread()
        {
            SourceApply = new CancellationTokenSource();
            var token = SourceApply.Token;

            try
            {
                TaskApply = Task.Run(() =>
                {
                    FindAndListFixes(token);
                    //FindAndListFixes();
                }, token);

                //-------------------------------------------------------------
                buttonOK.Enabled = false;
                buttonApply.Enabled = false;
                buttonCheckAll.Enabled = false;
                buttonInvertCheck.Enabled = false;
                for (int n = 0; n < groupBox1.Controls.Count; n++)
                {
                    var c = groupBox1.Controls[n];
                    if (c is CheckBox box)
                    {
                        if (c.Text == exception1 || c.Text == exception2 || c.Text == exception3)
                        {
                            box.Checked = false;
                            box.Visible = false;
                        }
                        box.Enabled = false;
                    }
                }
                //-------------------------------------------------------------
                WaitLabel.BringToFront();
                progressBar1.Visible = true;
                var t = new System.Windows.Forms.Timer();
                t.Interval = 500;
                t.Tick += (s, e) =>
                {
                    if (TaskApply.IsCompleted == true)
                    {
                        if (applyClicked == true)
                            buttonOK.Enabled = true;
                        //labelPercent.Visible = false;
                        //progressBar1.Visible = false;
                        WaitLabel.SendToBack();
                        //-------------------------------------------------------------
                        buttonOK.Enabled = true;
                        buttonApply.Enabled = true;
                        buttonCheckAll.Enabled = true;
                        buttonInvertCheck.Enabled = true;
                        for (int n = 0; n < groupBox1.Controls.Count; n++)
                        {
                            var c = groupBox1.Controls[n];
                            if (c is CheckBox box)
                            {
                                if (c.Text == exception1 || c.Text == exception2 || c.Text == exception3)
                                {
                                    box.Checked = false;
                                    box.Visible = false;
                                }
                                box.Enabled = true;
                            }
                        }
                        //-------------------------------------------------------------
                        //// Sort Column Line#
                        //lvwColumnSorter.SortColumn = 1;
                        //lvwColumnSorter.Order = SortOrder.Ascending;
                        //listViewFixes.Sort();
                        t.Stop();
                    }
                };
                t.Start();
            }
            catch (OperationCanceledException)
            {
                //
            }
        }

        static string GetResourceTextFile(string path)
        {
            // Determine path
            var assembly = Assembly.GetExecutingAssembly();
            // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
            path = assembly.GetManifestResourceNames().Single(str => str.EndsWith(path));
            Stream stream = assembly.GetManifestResourceStream(path);
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        
        public void FindAndListFixes(CancellationToken token)
        {
            DateTime startTime = DateTime.Now;
            _totalFixes = 0;
            progressBar1.Visible = true;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Step = 1;
            progressBar1.Value = 0;
            //========== Creating List ============================================
            List<Tuple<string, string, string>> ReplaceExpressionList = new List<Tuple<string, string, string>>();
            ReplaceExpressionList.Clear();
            var fileContent = GetResourceTextFile("PersianErrors.multiple_replace.xml"); // Load from Embedded Resource

            XDocument doc = XDocument.Parse(fileContent);
            var groups = doc.Root.Elements().Elements();

            for (int n = 0; n < groupBox1.Controls.Count; n++)
            {
                if (token.IsCancellationRequested == true || closeForm == true)
                    return;
                var cc = groupBox1.Controls[n];
                if ((cc is CheckBox box) && box.Checked)
                {
                    for (int a = 0; a < groups.Count(); a++)
                    {
                        var group = groups.ToList()[a];
                        string groupName = group.Element("Name").Value;

                        if (groupName == box.Text)
                        {
                            var rules = group.Elements("MultipleSearchAndReplaceItem");
                            for (int b = 0; b < rules.Count(); b++)
                            {
                                var rule = rules.ToList()[b];
                                bool ruleEnabled = Convert.ToBoolean(rule.Element("Enabled").Value);
                                if (ruleEnabled)
                                {
                                    string findWhat = rule.Element("FindWhat").Value;
                                    string replaceWith = rule.Element("ReplaceWith").Value;
                                    string searchType = rule.Element("SearchType").Value;

                                    findWhat = FindWhatRule(findWhat);
                                    replaceWith = ReplaceWithRule(replaceWith);

                                    if (searchType == "RegularExpression" && !CompiledRegExList.ContainsKey(findWhat))
                                    {
                                        CompiledRegExList.Add(findWhat, new Regex(findWhat, RegexOptions.Compiled | RegexOptions.Multiline,
                                            TimeSpan.FromMilliseconds(2000)));
                                    }

                                    ReplaceExpressionList.Add(new Tuple<string, string, string>(findWhat, replaceWith, searchType));
                                }
                            }
                        }
                    }
                }
            }
            //========== Replacing List ===========================================
            List<ListViewItem> fixes = new List<ListViewItem>();
            fixes.Clear();

            // Set a timeout interval of 2 seconds.
            AppDomain domain = AppDomain.CurrentDomain;
            domain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(2000));

            for (int pn = 0; pn < SubCurrent.Paragraphs.Count; pn++)
            {
                Paragraph p = SubCurrent.Paragraphs[pn];
                if (token.IsCancellationRequested == true || closeForm == true)
                    break;
                // Progress bar
                int TC = SubCurrent.Paragraphs.Count;
                progressBar1.Maximum = TC;
                progressBar1.Minimum = 0;
                int CC = p.Number;
                int VC = 0;
                if (TC != 0) // Solving: Attempted to divide by zero on exit.
                    VC = CC * 100 / TC;
                else
                    return;
                if (VC > 100)
                    return;
                progressBar1.Value = CC;
                progressBar1.StartTime = startTime;
                //== Fixing issue caused by the animation (Bug)
                if (CC > 1)
                    progressBar1.Value = CC - 1;
                if (VC == 100)
                    progressBar1.Value = progressBar1.Maximum;
                //== End of fix
                labelWorking.Text = "Working On Line#\n" + CC + "/" + TC;

                p.Text = p.Text.Replace("<br />", Environment.NewLine).Replace("</ br>", Environment.NewLine);
                string Before = @p.Text;
                string After = @p.Text;

                for (int i = 0; i < ReplaceExpressionList.Count; i++)
                {
                    var list = ReplaceExpressionList[i];
                    string findWhat = list.Item1;
                    string replaceWith = list.Item2;
                    string searchType = list.Item3;

                    if (searchType == "RegularExpression")
                    {
                        Regex regExFindWhat = CompiledRegExList[findWhat];

                        try
                        {
                            if (regExFindWhat.IsMatch(After))
                            {
                                After = regExFindWhat.Replace(After, replaceWith);
                            }
                        }
                        catch (RegexMatchTimeoutException ex)
                        {
                            Debug.WriteLine("Regex timed out!");
                            Debug.WriteLine("- Timeout interval specified: " + ex.MatchTimeout.TotalMilliseconds);
                            Debug.WriteLine("- Pattern: " + ex.Pattern);
                            Debug.WriteLine("- Input: " + ex.Input);
                        }
                    }
                    else if (searchType == "Normal")
                    {
                        After = After.Replace(findWhat, replaceWith);
                    }
                }

                After = After.Trim();
                if (After != Before)
                {
                    //Before = Utilities.RemoveHtmlTags(Before);
                    //After = Utilities.RemoveHtmlTags(After);

                    if (token.IsCancellationRequested == true || closeForm == true)
                        return;
                    var item = new ListViewItem(string.Empty) { Tag = p, Checked = true, UseItemStyleForSubItems = true };
                    var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString());
                    item.SubItems.Add(subItem);
                    subItem = new ListViewItem.ListViewSubItem(item, Before.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
                    item.SubItems.Add(subItem);
                    subItem = new ListViewItem.ListViewSubItem(item, After.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
                    item.SubItems.Add(subItem);
                    if (token.IsCancellationRequested == true || closeForm == true)
                        return;
                    //listViewFixes.InvokeIt(() => listViewFixes.Items.Add(item));
                    fixes.Add(item);

                    _totalFixes++;
                    labelTotal.Text = string.Format("Total Fixes: {0}", _totalFixes);
                    labelTotal.ForeColor = _totalFixes <= 0 ? Color.Green : Color.Blue;
                }
            }
            labelTotal.Text = string.Format("Total Fixes: {0}", _totalFixes);
            labelTotal.ForeColor = _totalFixes <= 0 ? Color.Green : Color.Blue;
            listViewFixes.InvokeIt(() => listViewFixes.Items.AddRange(fixes.ToArray()));
            labelWorking.Text = null;
        }

        private static void SettingsSaveCheckBoxes(string checkBoxName, string value)
        {
            string tableName = "CheckBoxes";
            if (!DataSetSettings.Tables.Contains(tableName))
            {
                DataTable dataTable = new DataTable();
                dataTable.TableName = tableName;
                DataSetSettings.Tables.Add(dataTable);
            }
            var dt = DataSetSettings.Tables[tableName];

            if (!dt.Columns.Contains(checkBoxName))
                dt.Columns.Add(checkBoxName);

            if (dt.Rows.Count == 0)
            {
                DataRow dataRow1 = dt.NewRow();
                dataRow1[checkBoxName] = value;
                dt.Rows.Add(dataRow1);
            }
            else
            {
                DataRow dataRow1 = dt.Rows[0];
                if (dataRow1 != null)
                    dataRow1[checkBoxName] = value;
            }
        }

        private void CheckboxHandler(object sender, EventArgs e)
        {
            // Search and find Checkboxes
            int n = 0;
            for (int i = 0; i < groupBox1.Controls.Count; i++)
            {
                var c = groupBox1.Controls[i];
                if (c is CheckBox box)
                {
                    SettingsSaveCheckBoxes(box.Text, box.Checked.ToString());
                    if (box.Checked == true)
                        n++;
                }
            }
            if (n > 0)
                buttonApply.Enabled = true;
            else
                buttonApply.Enabled = false;
        }

        private void Box_MouseHover(object sender, EventArgs e)
        {
            CheckBox ch = sender as CheckBox;
            if (ch.Text.Equals("Fix Unicode Control Char"))
                pictureBox1.Image = global::PersianErrors.Properties.Resources.FixUnicodeControlChar;
            else if (ch.Text.Equals("Change Arabic Chars to Persian"))
                pictureBox1.Image = global::PersianErrors.Properties.Resources.ChangeArabicCharsToPersian;
            else if (ch.Text.Equals("Remove Unneeded Spaces"))
                pictureBox1.Image = global::PersianErrors.Properties.Resources.RemoveUnneededSpaces;
            else if (ch.Text.Equals("Add Missing Spaces"))
                pictureBox1.Image = global::PersianErrors.Properties.Resources.AddMissingSpaces;
            else if (ch.Text.Equals("Fix Dialog Hyphen"))
                pictureBox1.Image = global::PersianErrors.Properties.Resources.FixDialogHyphen;
            else if (ch.Text.Equals("Fix Wrong Chars"))
                pictureBox1.Image = global::PersianErrors.Properties.Resources.FixWrongChars;
            else if (ch.Text.Equals("Fix Misplaced Chars"))
                pictureBox1.Image = global::PersianErrors.Properties.Resources.FixMisplacedChars;
            else if (ch.Text.Equals("Fix Abbreviations"))
                pictureBox1.Image = global::PersianErrors.Properties.Resources.FixAbbreviations;
            else if (ch.Text.Equals("Space to Invisible Space"))
                pictureBox1.Image = global::PersianErrors.Properties.Resources.SpaceToInvisibleSpace;
            else if (ch.Text.Equals("OCR"))
                pictureBox1.Image = global::PersianErrors.Properties.Resources.OCR;
            else if (ch.Text.Equals("Remove Leading Dots"))
                pictureBox1.Image = global::PersianErrors.Properties.Resources.RemoveLeadingDots;
            else if (ch.Text.Equals("Remove Dot from the End of Line"))
                pictureBox1.Image = global::PersianErrors.Properties.Resources.RemoveDotFromTheEndOfLine;
        }

        private void Box_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.Image = global::PersianErrors.Properties.Resources.ICON;
        }

        private void LinkLabelEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("mailto:msasanmh@gmail.com");
        }

        private void LinkLabelHomepage_Click(object sender, EventArgs e)
        {
            string url = "https://github.com/msasanmh/PersianSubtitleFixes";
            var startInfo = new ProcessStartInfo(url)
            {
                UseShellExecute = true
            };
            Process.Start(startInfo);
        }

        private void SelectionHandler(object sender, EventArgs e)
        {
            if (listViewFixes.Items.Count <= 0)
                return;

            if ((sender as Button).Text == "Select All")
                foreach (ListViewItem item in listViewFixes.Items)
                    item.Checked = true;
            else
                foreach (ListViewItem item in listViewFixes.Items)
                    item.Checked = !item.Checked;
        }

        private void ApplyPreview()
        {
            if (listViewFixes.Items.Count != 0)
            {
                int count = 0;
                for (int pn = 0; pn < SubCurrent.Paragraphs.Count; pn++)
                {
                    Paragraph p = SubCurrent.Paragraphs[pn];
                    string ln = p.Number.ToString();
                    for (int n = 0; n < listViewFixes.Items.Count; n++)
                    {
                        var item = listViewFixes.Items[n];
                        if (item.SubItems[1].Text == ln)
                        {
                            if (item.Checked == false)
                            {
                                p.Text = item.SubItems[2].Text;
                                p.Text = p.Text.Replace(Configuration.ListViewLineSeparatorString, Environment.NewLine);
                            }
                            else if (item.Checked == true)
                            {
                                count++;
                                p.Text = item.SubItems[3].Text;
                                p.Text = p.Text.Replace(Configuration.ListViewLineSeparatorString, Environment.NewLine);
                            }
                        }
                    }

                }
                //----------------------------------------------------------------
                if (count > 0)
                {
                    //SubTemp = new(SubCurrent);
                }
            }
        }

        private void ButtonApply_Click(object sender, EventArgs e)
        {
            WaitLabel.BringToFront();
            Task.Delay(50).Wait();
            applyClicked = true;
            Cursor.Current = Cursors.WaitCursor;
            ApplyPreview();
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            StartThread();
            listViewFixes.EndUpdate();
            Cursor.Current = Cursors.Default;
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Tools.WriteAllText(Tools.SettingsFilePath(), DataSetSettings.ToXmlWithWriteMode(XmlWriteMode.IgnoreSchema), new UTF8Encoding(false));
            ApplyPreview();
            SaveSubtitle = SubCurrent.ToText(new SubRip());
            Cursor.Current = Cursors.Default;
            DialogResult = DialogResult.OK;
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Tools.WriteAllText(Tools.SettingsFilePath(), DataSetSettings.ToXmlWithWriteMode(XmlWriteMode.IgnoreSchema), new UTF8Encoding(false));
            if (applyClicked == true)
            {
                if (TaskApply != null)
                {
                    if (!TaskApply.IsCompleted)
                    {
                        SourceApply.Cancel();
                        closeForm = true;
                        TaskApply.ContinueWith(t => Close(),
                            TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    else
                        Close();
                }
                else
                    Close();
            }
            else
                Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs ex)
        {
            if (ex.CloseReason == CloseReason.UserClosing || ex.CloseReason == CloseReason.WindowsShutDown)
            {
                Tools.WriteAllText(Tools.SettingsFilePath(), DataSetSettings.ToXmlWithWriteMode(XmlWriteMode.IgnoreSchema), new UTF8Encoding(false));
                if (applyClicked == true)
                {
                    if (TaskApply != null)
                    {
                        if (!TaskApply.IsCompleted)
                        {
                            ex.Cancel = true;
                            SourceApply.Cancel();
                            closeForm = true;
                            TaskApply.ContinueWith(t => Close(),
                                TaskScheduler.FromCurrentSynchronizationContext());
                        }
                    }
                }
            }
        }

        private void listViewFixes_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView myListView = (ListView)sender;
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.ColumnToSort)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.OrderOfSort == SortOrder.Ascending)
                {
                    lvwColumnSorter.OrderOfSort = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.OrderOfSort = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.ColumnToSort = e.Column;
                lvwColumnSorter.OrderOfSort = SortOrder.Ascending;
            }
            // Perform the sort with these new sort options.
            myListView.Sort();
        }

        //// Unhandled exception handler
        static void Main(string[] args)
        {
            // Add the event handler for handling UI thread exceptions to the event.
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            // Add the event handler for handling non-UI thread exceptions to the event.
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            // Set the unhandled exception mode to force all Windows Forms errors
            // to go through our handler.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            // Log the exception, display it, etc
            Debug.WriteLine(e.Exception.Message);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Log the exception, display it, etc
            Debug.WriteLine((e.ExceptionObject as Exception).Message);
        }

        private void listViewFixes_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        
    }
}
