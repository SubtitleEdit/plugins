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

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class MainForm : Form
    {
        private Subtitle _subtitle;
        //private XElement _xmlSetting = null;
        private bool _allowFixes;
        private int _totalFixed;
        public string FixedSubtitle { get; set; }

        

        public MainForm()
        {
            InitializeComponent();
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

            getGroupNames();
            startCoding();
        }

        public void getGroupNames()
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
                if ((c is CheckBox))
                {
                    if (c.Text == "Guide" || c.Text == "Formal to Slang" || c.Text == "Options")
                        ((CheckBox)c).Visible = false;
                }
            }
            buttonApply.Visible = false;
            MinimumSize = new System.Drawing.Size(1000, 600);   // Main form minimum size
        }

        private void startCoding()
        {
            foreach (Control c in groupBox1.Controls)
            {
                if ((c is CheckBox) && ((CheckBox)c).Checked)
                {
                    //Thread thread = new Thread(() => FindAndListFixes(c.Text));
                    //thread.Start();
                    //thread.Join();
                    FindAndListFixes(c.Text);
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
                return (null);
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
                                                        //Console.WriteLine(child5.Name + ": " + child5.InnerText);
                                                        listN.Add(child5.InnerText);
                                                    }
                                                    foreach (XmlNode child5 in child3.SelectNodes("ReplaceWith"))
                                                    {
                                                        //Console.WriteLine(child5.Name + ": " + child5.InnerText);
                                                        listN.Add(child5.InnerText);
                                                    }
                                                }
                                                else if (child4.InnerText == "RegularExpression")
                                                {
                                                    foreach (XmlNode child5 in child3.SelectNodes("FindWhat"))
                                                    {
                                                        //Console.WriteLine(child5.Name + ": " + child5.InnerText);
                                                        listR.Add(child5.InnerText);
                                                    }
                                                    foreach (XmlNode child5 in child3.SelectNodes("ReplaceWith"))
                                                    {
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
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                string oldText = p.Text;
                string text = p.Text;

                foreach (var m in dicN) text = text.Replace(m.Key, m.Value).Trim();
                foreach (var m in dicR) text = Regex.Replace(text, m.Key, m.Value, RegexOptions.None).Trim();
                
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
                            AddFixToListView(p, oldText, text);
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

        private void AddFixToListView(Paragraph p, string before, string after)
        {
            if (before != after)
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
                listViewFixes.Items.Add(item);
            }
        }
        
        private void applyFixes()
        {
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

        private void buttonApply_Click(object sender, EventArgs e)
        {
            //listViewFixes.Items.Clear();
            //_totalFixed = 0;
            //   _allowFixes = true;
            //   startCoding();
            //   FixedSubtitle = _subtitle.ToText(new SubRip());
            applyFixes();
            
        }
        
        private void buttonOK_Click(object sender, EventArgs e)
        {
            _allowFixes = true;
            startCoding();
            FixedSubtitle = _subtitle.ToText(new SubRip());
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void CheckboxHandler(object sender, EventArgs e)
        {
            listViewFixes.Items.Clear();
            _totalFixed = 0;
            startCoding();

            //// Search and find Checkboxes
            //foreach (Control c in groupBox1.Controls)
            //{
            //    if ((c is CheckBox))
            //    {
            //        if (((CheckBox)c).Checked || !((CheckBox)c).Checked)
            //        {
            //            listViewFixes.Items.Clear();
            //            _totalFixed = 0;
            //            startCoding();
            //        }
            //    }
            //}
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void labelNote_Click(object sender, EventArgs e)
        {
            
        }

        private void linkLabelEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:msasanmh@gmail.com");
        }
    }
}
