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
using System.IO.IsolatedStorage;
using System.Threading;
using System.Threading.Tasks;
using ListViewSorter;
using System.Diagnostics;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class MainForm : Form
    {
        private static readonly string exception1 = "Options";
        private static readonly string exception2 = "Formal to Slang";
        private static readonly string exception3 = "Guide";
        private bool applyClicked = false;
        private readonly Subtitle _subtitle;
        private bool _allowFixes;
        private int _totalFixes;
        // Break from loops on Form Closing
        private bool closeForm = false;
        CancellationTokenSource source = null;
        public string FixedSubtitle { get; set; }
        // Sort Column Line#
        private ListViewColumnSorter lvwColumnSorter = null;

        public MainForm()
        {
            InitializeComponent();
            string AV = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Text = "Persian Subtitle Fixes (Persian Common Errors)" + " - v" + AV;
            StartPosition = FormStartPosition.CenterScreen;
            FormClosing += MainForm_FormClosing;
            //// Sort Column Line#
            //lvwColumnSorter = new ListViewColumnSorter();
            //listViewFixes.ListViewItemSorter = lvwColumnSorter;
            //listViewFixes.Sorting = SortOrder.Ascending;
            //listViewFixes.AutoArrange = true;
            //lvwColumnSorter._SortModifier = ListViewColumnSorter.SortModifiers.SortByText;
        }
        public MainForm(Subtitle subtitle, string name, string description, Form parentForm)
            : this()
        {
            // TODO: Complete member initialization
            _subtitle = subtitle;
            Resize += (s, arg) =>
            {
                listViewFixes.Columns[listViewFixes.Columns.Count - 1].Width = -2;
                //this.listViewFixes.Columns.Count -1
            };
            buttonOK.Enabled = false;
            GetGroupNames();
        }
        public void StartThread()
        {
            source = new CancellationTokenSource();
            var token = source.Token;

            try
            {
                var task = Task.Run(() =>
                {
                    StartCodingThread(token);
                    //FindAndListFixes();
                }, token);

                var WaitLabel = new Label();
                //-------------------------------------------------------------
                buttonOK.Enabled = false;
                buttonApply.Enabled = false;
                buttonCheckAll.Enabled = false;
                buttonInvertCheck.Enabled = false;
                foreach (Control c in groupBox1.Controls)
                {
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
                Rectangle screenRectangle = RectangleToScreen(ClientRectangle);
                int titleHeight = screenRectangle.Top - Top;
                //int LX = Left + Width / 2 - (WaitForm.Width / 2) + listViewFixes.Bounds.X/2;
                //int LY = Top + Height / 2 - (WaitForm.Height / 2) + listViewFixes.Bounds.Y/2;
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
                WaitLabel.BringToFront();
                labelPercent.Visible = true;
                progressBar1.Visible = true;
                var t = new System.Windows.Forms.Timer();
                t.Interval = 500;
                t.Tick += (s, e) =>
                {
                    if (task.IsCompleted == true)
                    {
                        if (applyClicked == true)
                            buttonOK.Enabled = true;
                        labelPercent.Visible = false;
                        progressBar1.Visible = false;
                        WaitLabel.SendToBack();
                        WaitLabel.Hide();
                        //-------------------------------------------------------------
                        buttonOK.Enabled = true;
                        buttonApply.Enabled = true;
                        buttonCheckAll.Enabled = true;
                        buttonInvertCheck.Enabled = true;
                        foreach (Control c in groupBox1.Controls)
                        {
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
                labelPercent.Text = "Error";
            }
        }
        public void GetGroupNames()
        {
            // Get file content from Embedded Resource
            string fileContents = GetResourceTextFile("PersianErrors.multiple_replace.xml");
            var listGN = new List<string>();
            XmlDocument docGN = new XmlDocument();
            docGN.LoadXml(fileContents);
            XmlNodeList nodesGN = docGN.GetElementsByTagName("Group");
            foreach (XmlNode node in nodesGN)
            {
                //Console.WriteLine(node.Name);
                foreach (XmlNode childN in node.SelectNodes("Name"))
                {
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
                box.Checked = true;
                box.Tag = i.ToString();
                box.Text = listGN[j];
                box.Name = "checkBox" + i.ToString();
                box.AutoSize = true;
                box.UseVisualStyleBackColor = true;
                //box.Click += new EventHandler(CheckboxHandler);
                box.Location = new Point(10, (i + 1) * 20); //vertical
                                                      //box.Location = new Point(i * 50, 10); //horizontal
                groupBox1.Controls.Add(box); // Add CheckBoxes inside GroupBox1
            }
            foreach (Control c in groupBox1.Controls)
            {
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
        }
        private int CountCheckedCB()
        {
            int CountCheckedCB = 0;
            foreach (Control c in groupBox1.Controls)
            {
                if ((c is CheckBox box) && box.Checked)
                {
                    CountCheckedCB++;
                }
            }
            return CountCheckedCB;
        }
        private void StartCodingThread(CancellationToken token)
        {
            FindAndListFixes();
            if (token.IsCancellationRequested)
            {
                //clean up code
                token.ThrowIfCancellationRequested();
            }
        }
        private void StartCoding()
        {
            FindAndListFixes();
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
        static string GetIsolatedTextFile(string name)
        {
            // Get file content from Embedded Resource
            string fileContents = GetResourceTextFile("PersianErrors.multiple_replace.xml");
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            //if (isoStore.FileExists("multiple_replace.xml")) { isoStore.DeleteFile("multiple_replace.xml"); }
            if (isoStore.FileExists(name))
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(name, FileMode.Open, isoStore))
                {
                    using (StreamReader reader = new StreamReader(isoStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            else
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(name, FileMode.CreateNew, isoStore))
                {
                    using (StreamWriter writer = new StreamWriter(isoStream))
                    {
                        writer.WriteLine(fileContents);
                    }
                }
                return null;
            }
        }
        public void FindAndListFixes()
        {
            labelPercent.Visible = true;
            progressBar1.Visible = true;
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Step = 1;
            _totalFixes = 0;

            //========== Creating List ============================================
            List<Tuple<string, string, string>> listT = new List<Tuple<string, string, string>>();
            listT.Clear();
            foreach (Control cc in groupBox1.Controls)
            {
                if (closeForm == true)
                    break;
                if ((cc is CheckBox box) && box.Checked)
                {
                    var fileContent = GetResourceTextFile("PersianErrors.multiple_replace.xml"); // Load from Embedded Resource
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(fileContent); // Load from String
                                              //doc.Load(fileContent); // Load from URL
                    XmlNodeList nodes = doc.GetElementsByTagName("Group");
                    foreach (XmlNode node in nodes)
                    {
                        if (closeForm == true)
                            break;
                        //Console.WriteLine(node.Name);
                        foreach (XmlNode childN in node.SelectNodes("Name"))
                        {
                            //Console.WriteLine("Group " + childN.Name + ": " + childN.InnerText);
                            if (childN.InnerText == cc.Text)
                            {
                                foreach (XmlNode child in node.SelectNodes("Enabled"))
                                {
                                    //Console.WriteLine("Group " + child.Name + ": " + child.InnerText);
                                    if (child.InnerText == "True")
                                    {
                                        foreach (XmlNode child1 in node.SelectNodes("MultipleSearchAndReplaceItem"))
                                        {
                                            //Console.WriteLine(child3.ChildNodes[0].Name); // Enabled
                                            //Console.WriteLine(child3.ChildNodes[1].Name); // FindWhat
                                            //Console.WriteLine(child3.ChildNodes[2].Name); // ReplaceWith
                                            //Console.WriteLine(child3.ChildNodes[3].Name); // SearchType
                                            //Console.WriteLine(child3.ChildNodes[4].Name); // Description
                                            if (child1.ChildNodes[0].InnerText == "True")
                                            {
                                                if (child1.ChildNodes[3].InnerText == "Normal")
                                                {
                                                    //Console.WriteLine(child1.ChildNodes[1].Name); // FindWhat
                                                    //Console.WriteLine(child1.ChildNodes[2].Name); // ReplaceWith
                                                    listT.Add(new Tuple<string, string, string>(child1.ChildNodes[3].InnerText, @child1.ChildNodes[1].InnerText, @child1.ChildNodes[2].InnerText));
                                                }
                                                else if (child1.ChildNodes[3].InnerText == "RegularExpression")
                                                {
                                                    //Console.WriteLine(child1.ChildNodes[1].Name); // FindWhat
                                                    //Console.WriteLine(child1.ChildNodes[2].Name); // ReplaceWith
                                                    listT.Add(new Tuple<string, string, string>(child1.ChildNodes[3].InnerText, @child1.ChildNodes[1].InnerText, @child1.ChildNodes[2].InnerText));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //========== Replacing List ===========================================
            //Parallel.ForEach(_subtitle.Paragraphs, p =>
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (closeForm == true)
                    break;
                // Progress bar
                int TC = _subtitle.Paragraphs.Count;
                int CC = p.Number;
                int VC = CC * 100 / TC;
                progressBar1.Value = VC;
                if (Enumerable.Range(50, 100).Contains(VC))
                {
                    labelPercent.BackColor = Color.FromArgb(6, 176, 37);
                }
                else { labelPercent.BackColor = SystemColors.ControlLight; }
                labelPercent.Text = VC.ToString() + "%";
                labelWorking.Text = "Working On Line#\n" + CC + "/" + TC;

                //if (p.Text[0] == '"')
                //    p.Text = "\\\"" + p.Text.Substring(1);
                //if (p.Text[p.Text.Length - 1] == '"')
                //    p.Text = p.Text.Remove(p.Text.Length - 1, 1) + "\\\"";
                p.Text = p.Text.Replace("<br />", Environment.NewLine).Replace("</ br>", Environment.NewLine);
                string Before = @p.Text;
                string After = @p.Text;
                After = After.Replace("<br />", Environment.NewLine).Replace("</ br>", Environment.NewLine);

                foreach (var listTC in listT)
                {
                    if (closeForm == true)
                        break;
                    // Item1 SearchType, Item2 FindWhat, Item3 ReplaceWith
                    string searchType = listTC.Item1;
                    string findWhat = @listTC.Item2;
                    string replaceWith = @listTC.Item3;
                    findWhat = findWhat.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
                    replaceWith = replaceWith.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
                    
                    if (!string.IsNullOrEmpty(findWhat))
                    {
                        if (searchType == "Normal")
                        {
                            After = After.Replace(@findWhat, @replaceWith);
                            //AddFixToListView(searchType, @findWhat, @replaceWith);
                        }
                        else if (searchType == "RegularExpression")
                        {
                            if (After.Contains(Environment.NewLine))
                                After = Regex.Replace(After, @findWhat, @replaceWith, RegexOptions.Multiline);
                            else if (!After.Contains(Environment.NewLine))
                                After = Regex.Replace(After, @findWhat, @replaceWith, RegexOptions.Singleline);

                            //After = After.Replace("\"،\"", "\"، \"");
                            //AddFixToListView(searchType, @findWhat, @replaceWith);
                        }
                    }
                }
                After.Trim();
                if (After != Before)
                {
                    if (AllowFix(p))
                    {
                        p.Text = After;
                    }
                    else
                    {
                        if (!_allowFixes)
                        {
                            Before = Utilities.RemoveHtmlTags(Before);
                            After = Utilities.RemoveHtmlTags(After);
                            if (listViewFixes.InvokeRequired)
                            {
                                listViewFixes.Invoke((MethodInvoker)delegate ()
                                {
                                    var item = new ListViewItem() { Checked = true, UseItemStyleForSubItems = true };
                                    var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString());
                                    item.SubItems.Add(subItem);
                                    subItem = new ListViewItem.ListViewSubItem(item, Before.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
                                    item.SubItems.Add(subItem);
                                    subItem = new ListViewItem.ListViewSubItem(item, After.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
                                    item.SubItems.Add(subItem);
                                    //item.Tag = p; // save paragraph in Tag
                                    Task.Run(() => listViewFixes.Items.Add(item));
                                });
                            }
                            _totalFixes++;
                            labelTotal.Text = string.Format("Total Fixes: {0}", _totalFixes);
                            labelTotal.ForeColor = _totalFixes <= 0 ? Color.Red : Color.Blue;
                        }
                    }
                }
            }
            labelWorking.Text = null;
            labelPercent.Visible = false;
            progressBar1.Visible = false;
        }
        private bool AllowFix(Paragraph p)
        {
            if (!_allowFixes)
                return false;
            string ln = p.Number.ToString();
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.SubItems[1].Text == ln)
                    return item.Checked;
            }
            return false;
        }
        private void AddFixToListView(string p, string before, string after)
        {
            if (listViewFixes.InvokeRequired)
            {
                listViewFixes.Invoke((MethodInvoker)delegate ()
                {
                    var item = new ListViewItem() { Checked = true, UseItemStyleForSubItems = true };
                    var subItem = new ListViewItem.ListViewSubItem(item, p);
                    item.SubItems.Add(subItem);
                    subItem = new ListViewItem.ListViewSubItem(item, before);
                    item.SubItems.Add(subItem);
                    subItem = new ListViewItem.ListViewSubItem(item, after);
                    item.SubItems.Add(subItem);
                    //item.Tag = p; // save paragraph in Tag
                    Task.Run(() => listViewFixes.Items.Add(item));
                });
            }
        }
        private void SelectionHandler(object sender, EventArgs e)
        {
            if (this.listViewFixes.Items.Count <= 0)
                return;

            if ((sender as Button).Text == "Select All")
                foreach (ListViewItem item in this.listViewFixes.Items)
                    item.Checked = true;
            else
                foreach (ListViewItem item in this.listViewFixes.Items)
                    item.Checked = !item.Checked;
        }
        private void ButtonApply_Click(object sender, EventArgs e)
        {
            applyClicked = true;
            Cursor.Current = Cursors.WaitCursor;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            StartThread();
            //FindAndListFixes();
            listViewFixes.EndUpdate();
            Cursor.Current = Cursors.Default;
        }
        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (listViewFixes.Items.Count != 0)
            {
                Cursor.Current = Cursors.WaitCursor;
                _allowFixes = true;
                StartCoding();
                FixedSubtitle = _subtitle.ToText(new SubRip());
                Cursor.Current = Cursors.Default;
            }
            DialogResult = DialogResult.OK;
        }
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            closeForm = true;
            Task.Delay(500).Wait();
            if (applyClicked == true)
                source.Cancel();
            listViewFixes.Items.Clear();
            DialogResult = DialogResult.Cancel;
        }
        private void CheckboxHandler(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _totalFixes = 0;
            //closeForm = true;
            if (applyClicked == true)
                source.Cancel();
            listViewFixes.Items.Clear();
            StartThread();
            Cursor.Current = Cursors.Default;

            // Search and find Checkboxes
            foreach (Control c in groupBox1.Controls)
            {
                if (c is CheckBox box)
                {
                    if (box.Checked || !box.Checked)
                    {
                        box.Enabled = false;
                        buttonCheckAll.Enabled = false;
                        buttonInvertCheck.Enabled = false;
                    }
                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        private void LabelNote_Click(object sender, EventArgs e)
        {

        }
        private void LinkLabelEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("mailto:msasanmh@gmail.com");
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs ex)
        {
            if (ex.CloseReason == CloseReason.UserClosing)
            {
                closeForm = true;
                Task.Delay(500).Wait();
                if (applyClicked == true)
                    source.Cancel();
                listViewFixes.Items.Clear();

                //source.Dispose();
                //listViewFixes.Dispose();

                // Confirm user wants to close
                //switch (MessageBox.Show(this, "Are you sure?", "Do you still want ... ?", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                //{
                //    //Stay on this form
                //    case DialogResult.No:
                //        e.Cancel = true;
                //        break;
                //    default:
                //        break;
                //}
            }
            if (ex.CloseReason == CloseReason.WindowsShutDown)
            {
                // Eg. Autosave and clear up ressources
                closeForm = true;
                Task.Delay(500).Wait();
                if (applyClicked == true)
                    source.Cancel();
                listViewFixes.Items.Clear();
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
