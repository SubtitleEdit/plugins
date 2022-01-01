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

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class MainForm : Form
    {
        private readonly Subtitle _subtitle;
        private bool _allowFixes;
        private int _totalFixed;
        private int pCurrent;
        // Break from loops on Form Closing
        private bool closeForm = false;

        // Sort Column Line#
        private ListViewColumnSorter lvwColumnSorter = null;

        public string FixedSubtitle { get; set; }

        public MainForm()
        {
            InitializeComponent();
            Text = "Persian Subtitle Fixes (Persian Common Errors)";
            StartPosition = FormStartPosition.CenterScreen;
            FormClosing += MainForm_FormClosing;
            // Sort Column Line#
            lvwColumnSorter = new ListViewColumnSorter();
            listViewFixes.ListViewItemSorter = lvwColumnSorter;
            listViewFixes.Sorting = SortOrder.Ascending;
            listViewFixes.AutoArrange = true;
            lvwColumnSorter._SortModifier = ListViewColumnSorter.SortModifiers.SortByText;
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

            GetGroupNames();
            StartThread();
        }

        CancellationTokenSource source = null;
        public void StartThread()
        {
            source = new CancellationTokenSource();
            var token = source.Token;

            try
            {
                var task = Task.Run(() =>
                {
                    StartCodingThread(token);
                }, token);

                buttonOK.Enabled = false;
                var WaitLabel = new Label();
                var tf = new System.Windows.Forms.Timer();
                tf.Interval = 500;
                tf.Tick += (s, e) =>
                {
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
                    tf.Stop();
                };
                tf.Start();

                var t = new System.Windows.Forms.Timer();
                t.Interval = 500;
                t.Tick += (s, e) =>
                {
                    if (task.IsCompleted == true)
                    {
                        buttonOK.Enabled = true;
                        WaitLabel.SendToBack();
                        WaitLabel.Hide();
                        // Enable checkboxes:
                        foreach (Control c in groupBox1.Controls)
                        {
                            if (c is CheckBox box)
                            {
                                box.Enabled = true;
                            }
                        }
                        buttonCheckAll.Enabled = true;
                        buttonInvertCheck.Enabled = true;
                        // Sort Column Line#
                        lvwColumnSorter.SortColumn = 1;
                        lvwColumnSorter.Order = SortOrder.Ascending;
                        listViewFixes.Sort();
                        t.Stop();
                    }
                };
                t.Start();
            }
            catch (OperationCanceledException)
            {
                labelPercent.Text = "Err";
            }
            finally
            {
                //source.Dispose(); // Makes Error:
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
            int i = 1;
            int j = 1;
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
                box.Click += new EventHandler(CheckboxHandler);
                box.Location = new Point(10, i * 20); //vertical
                                                      //box.Location = new Point(i * 50, 10); //horizontal
                groupBox1.Controls.Add(box); // Add CheckBoxes inside GroupBox1
            }

            foreach (Control c in groupBox1.Controls)
            {
                if (c is CheckBox box)
                {
                    if (c.Text == "Guide" || c.Text == "Formal to Slang" || c.Text == "Options")
                    {
                        box.Checked = false;
                        box.Visible = false;
                    }
                    box.Enabled = false;
                }
            }
            buttonCheckAll.Enabled = false;
            buttonInvertCheck.Enabled = false;
            buttonApply.Visible = false;
            MinimumSize = new System.Drawing.Size(1000, 600);   // Main form minimum size
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
            _totalFixed = 0;
            foreach (Control c in groupBox1.Controls)
            {
                if (closeForm == true)
                    break;
                if ((c is CheckBox box) && box.Checked)
                {
                    box.ForeColor = Color.Blue;
                    labelWorking.Text = "Working on:\n" + c.Text;
                    FindAndListFixes(c.Text);
                    labelWorking.Text = null;
                    box.ForeColor = Color.Black;
                }
                if (token.IsCancellationRequested)
                {
                    //clean up code
                    token.ThrowIfCancellationRequested();
                }
            }
        }

        private void StartCoding()
        {
            _totalFixed = 0;
            foreach (Control c in groupBox1.Controls)
            {
                if (closeForm == true)
                    break;
                if ((c is CheckBox box) && box.Checked)
                {
                    //Thread thread = new Thread(() =>
                    //{
                    //    FindAndListFixes(c.Text);
                    //});
                    //thread.Start();
                    //thread.Join();
                    //Task.Run(() => FindAndListFixes(c.Text));
                    box.ForeColor = Color.Blue;
                    labelWorking.Text = "Working on:\n" + c.Text;
                    FindAndListFixes(c.Text);
                    labelWorking.Text = null;
                    box.ForeColor = Color.Black;
                }
            }
        }

        //// Reading Embedded Resource file: (Embedded Resource files are read only not writable)
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

        public void FindAndListFixes(string groupName)
        {
            //var fileContent = GetIsolatedTextFile("multiple_replace.xml");                       // Load from Isolated Storage
            var fileContent = GetResourceTextFile("PersianErrors.multiple_replace.xml");       // Load from Embedded Resource

            var listN = new List<string>();
            var listR = new List<string>();

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
                    if (childN.InnerText == groupName)
                    {
                        foreach (XmlNode child in node.SelectNodes("Enabled"))
                        {
                            //Console.WriteLine("Group " + child.Name + ": " + child.InnerText);
                            if (child.InnerText == "True")
                            {
                                foreach (XmlNode child3 in node.SelectNodes("MultipleSearchAndReplaceItem"))
                                {
                                    foreach (XmlNode child2 in child3.SelectNodes("Enabled"))
                                    {
                                        //Console.WriteLine(child2.Name + ": " + child2.InnerText);
                                        //list.Add(child2.InnerText);
                                        if (child2.InnerText == "True")
                                        {
                                            foreach (XmlNode child4 in child3.SelectNodes("SearchType"))
                                            {
                                                //Console.WriteLine(child4.Name + ": " + child4.InnerText);
                                                if (child4.InnerText == "Normal")
                                                {
                                                    foreach (XmlNode child5 in child3.SelectNodes("FindWhat"))
                                                    {
                                                        if (closeForm == true)
                                                            break;
                                                        //Console.WriteLine(child5.Name + ": " + child5.InnerText);
                                                        listN.Add(child5.InnerText);
                                                    }
                                                    foreach (XmlNode child5 in child3.SelectNodes("ReplaceWith"))
                                                    {
                                                        if (closeForm == true)
                                                            break;
                                                        //Console.WriteLine(child5.Name + ": " + child5.InnerText);
                                                        listN.Add(child5.InnerText);
                                                    }
                                                }
                                                else if (child4.InnerText == "RegularExpression")
                                                {
                                                    foreach (XmlNode child5 in child3.SelectNodes("FindWhat"))
                                                    {
                                                        if (closeForm == true)
                                                            break;
                                                        //Console.WriteLine(child5.Name + ": " + child5.InnerText);
                                                        listR.Add(child5.InnerText);
                                                    }
                                                    foreach (XmlNode child5 in child3.SelectNodes("ReplaceWith"))
                                                    {
                                                        if (closeForm == true)
                                                            break;
                                                        //Console.WriteLine(child5.Name + ": " + child5.InnerText);
                                                        listR.Add(child5.InnerText);
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
            }
            IDictionary<string, string> dicN = new Dictionary<string, string>();
            int a = 0;
            int b = 1;
            for (; b < listN.Count; b += 2, a += 2)
            {
                dicN[listN[a]] = listN[b];
            }
            IDictionary<string, string> dicR = new Dictionary<string, string>();
            int c = 0;
            int d = 1;
            for (; d < listR.Count; d += 2, c += 2)
            {
                dicR[listR[c]] = listR[d];
            }
            listViewFixes.BeginUpdate();

            progressBar1.Value = 0;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Step = 1;

            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (closeForm == true)
                    break;
                int CBC = _subtitle.Paragraphs.Count;
                pCurrent++;
                int VC = pCurrent / CountCheckedCB() * 100 / CBC;
                if (Enumerable.Range(98, 102).Contains(VC)) // Just in case set to 100%
                    VC = 100;
                labelPercent.Text = VC.ToString() + "%";
                if (Enumerable.Range(50, 100).Contains(VC))
                {
                    labelPercent.BackColor = Color.FromArgb(6, 176, 37);
                }
                else { labelPercent.BackColor = SystemColors.ControlLight; }

                progressBar1.Value = VC;
                // Solving \n issue
                p.Text = p.Text.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString);
                string oldText = p.Text;
                string text = p.Text;

                foreach (var m in dicN)
                {
                    if (closeForm == true)
                        break;
                    text = text.Replace(m.Key, m.Value).Trim();
                }
                foreach (var m in dicR)
                {
                    if (closeForm == true)
                        break;
                    text = Regex.Replace(text, m.Key, m.Value, RegexOptions.None).Trim();
                }

                if (text != oldText)
                {
                    if (AllowFix(p))
                    {
                        p.Text = text;
                    }
                    else
                    {
                        if (!_allowFixes)
                        {
                            oldText = Utilities.RemoveHtmlTags(oldText);
                            text = Utilities.RemoveHtmlTags(text);
                            Task.Run(() => AddFixToListView(p, oldText, text));
                            //Task.Run(() => AddFixToTuple(p, oldText, text));
                            _totalFixed++;
                        }
                    }
                }
            }
            if (!_allowFixes)
            {
                labelTotal.Text = string.Format("Total Fixes: {0}", _totalFixed);
                labelTotal.ForeColor = _totalFixed <= 0 ? Color.Red : Color.Blue;
            }
            listViewFixes.EndUpdate();
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

        private void AddFixToTuple(Paragraph p, string before, string after)
        {
            var listF = new List<Tuple<Paragraph, string, string>>();
            if (before != after)
            {
                listF.Add(new Tuple<Paragraph, string, string>(p, before, after));
                listF.Count();
            }
            int a = 0;
            for (; a <= listF.Count(); a++)
            {
                if (closeForm == true)
                    break;
                AddFixToListView(listF[a].Item1, listF[a].Item2, listF[a].Item3);
            }

        }

        private void AddFixToListView(Paragraph p, string before, string after)
        {
            if (before != after && listViewFixes.Items.Count >= 0)
            {
                var item = new ListViewItem() { Checked = true, UseItemStyleForSubItems = true };
                var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString());
                item.SubItems.Add(subItem);

                subItem = new ListViewItem.ListViewSubItem(item, before.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
                item.SubItems.Add(subItem);

                subItem = new ListViewItem.ListViewSubItem(item, after.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
                item.SubItems.Add(subItem);

                item.Tag = p; // save paragraph in Tag
                //if (listViewFixes.Items.)
                Task.Run(() => listViewFixes.Items.Add(item));
                listViewFixes.Refresh();
            }
        }

        private void ApplyFixes()
        {
            // Maybe next time!
            foreach (ListViewItem lv in listViewFixes.Items)
            {
                if (lv.SubItems[3].Text != "")
                {
                    lv.SubItems[2].Text = lv.SubItems[3].Text;
                    lv.SubItems[3].Text = null;
                    var text = lv.SubItems[2].Text;
                    string oldText = text;
                    text = text.Replace("a", "A").Trim();
                    if (text != oldText)
                    {
                        lv.SubItems[3].Text = text;
                    }
                }
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
            // Maybe next time!
            //listViewFixes.Items.Clear();
            //_totalFixed = 0;
            //_allowFixes = true;
            //startCoding();
            //FixedSubtitle = _subtitle.ToText(new SubRip());
            //ApplyFixes();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (listViewFixes.Items.Count != 0)
            {
                Cursor.Current = Cursors.WaitCursor;
                _allowFixes = true;
                pCurrent = 0;
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
            source.Cancel();
            listViewFixes.Items.Clear();
            DialogResult = DialogResult.Cancel;
        }

        private void CheckboxHandler(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _totalFixed = 0;
            pCurrent = 0;
            //closeForm = true;
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
                source.Cancel();
                listViewFixes.Items.Clear();
            }
        }

        private void listViewFixes_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            ListView myListView = (ListView)sender;
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.ColumnToSort)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.OrderOfSort == System.Windows.Forms.SortOrder.Ascending)
                {
                    lvwColumnSorter.OrderOfSort = System.Windows.Forms.SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.OrderOfSort = System.Windows.Forms.SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.ColumnToSort = e.Column;
                lvwColumnSorter.OrderOfSort = System.Windows.Forms.SortOrder.Ascending;
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
    }
}
