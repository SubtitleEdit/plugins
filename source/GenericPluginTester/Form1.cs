using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace GenericPluginTester
{
    public partial class Form1 : Form
    {
        private readonly List<string> _dllFiles = new List<string>();
        private string _lastLoadedPlugin;

        public Form1()
        {
            InitializeComponent();
            string currentPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var pluginPath = Directory.GetParent(currentPath).Parent.Parent.Parent;
            if (File.Exists("LastLoadedPlugin.txt"))
                _lastLoadedPlugin = File.ReadAllText("LastLoadedPlugin.txt");
            LoadPlugins(pluginPath.FullName, _dllFiles);
            RefillDrownDown();
        }

        private void LoadPlugins(string fullName, List<string> dllFiles)
        {
            foreach (var dir in Directory.GetDirectories(fullName))
            {
                LoadPlugins(dir, dllFiles);
            }
            foreach (var dll in Directory.GetFiles(fullName, "*.dll"))
            {
                if (!dll.Contains(Path.Combine("obj", "Debug")) && !dll.Contains(Path.Combine("obj", "Release")) && IsPlugin(dll))
                {
                    dllFiles.Add(dll);
                }
            }
        }

        private bool IsPlugin(string dllFileName)
        {
            string name, description, text, shortcut, actionType;
            decimal version;
            System.Reflection.MethodInfo mi;
            GetPropertiesAndDoAction(dllFileName, out name, out text, out version, out description, out actionType, out shortcut, out mi);
            return !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(actionType) && mi != null;
        }

        public static object GetPropertiesAndDoAction(string pluginFileName, out string name, out string text, out decimal version, out string description, out string actionType, out string shortcut, out System.Reflection.MethodInfo mi)
        {
            name = null;
            text = null;
            version = 0;
            description = null;
            actionType = null;
            shortcut = null;
            mi = null;
            System.Reflection.Assembly assembly;
            try
            {
                assembly = System.Reflection.Assembly.Load(File.ReadAllBytes(pluginFileName));
            }
            catch
            {
                return null;
            }
            string objectName = Path.GetFileNameWithoutExtension(pluginFileName);
            if (assembly != null)
            {
                Type pluginType = assembly.GetType("Nikse.SubtitleEdit.PluginLogic." + objectName);
                if (pluginType == null)
                    return null;
                object pluginObject = null;
                try
                {
                    pluginObject = Activator.CreateInstance(pluginType);
                }
                catch
                {
                    // ignore plugin that failed to load
                    return pluginObject;
                }

                // IPlugin
                var t = pluginType.GetInterface("IPlugin");
                if (t == null)
                    return null;

                System.Reflection.PropertyInfo pi = t.GetProperty("Name");
                if (pi != null)
                    name = (string)pi.GetValue(pluginObject, null);

                pi = t.GetProperty("Text");
                if (pi != null)
                    text = (string)pi.GetValue(pluginObject, null);

                pi = t.GetProperty("Description");
                if (pi != null)
                    description = (string)pi.GetValue(pluginObject, null);

                pi = t.GetProperty("Version");
                if (pi != null)
                    version = Convert.ToDecimal(pi.GetValue(pluginObject, null));

                pi = t.GetProperty("ActionType");
                if (pi != null)
                    actionType = (string)pi.GetValue(pluginObject, null);

                mi = t.GetMethod("DoAction");

                pi = t.GetProperty("Shortcut");
                if (pi != null)
                    shortcut = (string)pi.GetValue(pluginObject, null);

                return pluginObject;
            }
            return null;
        }

        private void LoadPlugin(string dllFileName)
        {
            if (checkBoxShowOnlyDebug.Checked && !dllFileName.Contains(Path.DirectorySeparatorChar + "Debug" + Path.DirectorySeparatorChar))
                return;

            listBox1.Items.Add(dllFileName);
            if (dllFileName == _lastLoadedPlugin)
            {
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Title = "Open Subtitle...";
            openFileDialog1.Filter = "SubRip files | *.srt";
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBox1.Text = File.ReadAllText(openFileDialog1.FileName);
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            int selectedIndex = listBox1.SelectedIndex;
            if (selectedIndex < 0)
            {
                return;
            }
            string fileName = listBox1.Items[selectedIndex].ToString();
            RunPlugin(fileName);
            _lastLoadedPlugin = fileName;
            try
            {
                File.WriteAllText("LastLoadedPlugin.txt", fileName);
            }
            catch (Exception)
            {
            }
        }

        private void RunPlugin(string fileName)
        {
            try
            {
                string name, description, text, shortcut, actionType;
                decimal version;
                System.Reflection.MethodInfo mi;
                object pluginObject = GetPropertiesAndDoAction(fileName, out name, out text, out version, out description, out actionType, out shortcut, out mi);
                if (mi == null)
                {
                    return;
                }

                string rawText = textBox1.Text;

                // If you have trouble debuggin in VS 2015 then set these check-boxes:
                //  Tools -> Options -> Debugging: "Use managed compatibility mode" + "Use native compatibility mode"
                string pluginResult = (string)mi.Invoke(pluginObject,
                                      new object[]
                                      {
                                        this,
                                        rawText,
                                        25,
                                        "<br />",
                                        GetRandomName() + ".srt",
                                        "",
                                        rawText
                                      });

                if (!string.IsNullOrEmpty(pluginResult) && pluginResult.Length > 10 && text != pluginResult)
                {
                    textBox2.Text = pluginResult;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private string GetRandomName()
        {
            var random = new Random();
            var sb = new StringBuilder();
            for (int i = 0; i < random.Next(2, 6); i++)
            {
                sb.Append(char.ConvertFromUtf32(random.Next(97, 97 + 20)));
            }
            return sb.ToString();
        }

        private void checkBoxShowOnlyDebug_CheckedChanged(object sender, EventArgs e)
        {
            RefillDrownDown();
        }

        private void RefillDrownDown()
        {
            listBox1.Items.Clear();
            _dllFiles.Sort();
            foreach (var dll in _dllFiles)
            {
                LoadPlugin(dll);
            }
        }

    }
}
