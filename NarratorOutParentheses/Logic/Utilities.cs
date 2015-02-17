using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class Utilities
    {
        //private static readonly List<string> _listNames = LoadName();
        private static List<string> _listNames = LoadNames(GetSubtilteEditNameList());
        private static List<string> _listNewName = new List<string>();


        private static string _filePath;

        public static List<string> ListNames
        {
            get { return _listNames; }
        }

        /*
        public static void LoadFromListName()
        {
            _listNames = LoadNames(GetSettingsFileName());
            _subtitleEditNames = false;
        }*/

        public static List<string> LoadNames(string fileName)
        {
            if (_listNames != null)
                _listNames = null;

            if (File.Exists(fileName))
            {
                return (from elem in XElement.Load(fileName).Elements()
                        select elem.Value.ToUpperInvariant()).ToList();
            }
            /*else
            {
                try
                {
                    // Create file
                    var xdoc = new XDocument(
                        new XElement("names",
                            new XElement("name", "CISCO"),
                            new XElement("name", "JOHN"),
                            new XElement("name", "MAN"),
                            new XElement("name", "WOMAN"),
                            new XElement("name", "CAITLIN")
                            ));
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(settingFile));
                    //File.Create(settingFile);
                    xdoc.Save(fileName, SaveOptions.None);
                    return (from elem in xdoc.Root.Elements()
                            select elem.Value).ToList();
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }*/
            return new List<string>();
        }

        public static string GetSubtilteEditNameList()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\", StringComparison.Ordinal))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Dictionaries");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtiitle Edit", "Dictionary"));
            var fullPath = Path.Combine(path, "names_etc.xml");
            _filePath = Path.Combine(path, "names_etc.xml");
            return _filePath;
        }

        public static int NumberOfLines(string text)
        {
            var ln = 0;
            var idx = -1;
            do
            {
                ln++;
                idx = text.IndexOf('\n', idx + 1);
            } while (idx > -1);
            return ln;
        }

        public static string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public static bool IsInteger(string s)
        {
            int i;
            return int.TryParse(s, out i);
        }

        internal static string RemoveHtmlFontTag(string s)
        {
            s = Regex.Replace(s, "(?i)</?font>", string.Empty);
            return RemoveTag(s, "<font");
        }

        public static string RemoveHtmlTags(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            if (s.IndexOf('<') < 0)
                return s;
            s = Regex.Replace(s, "(?i)</?[uib]>", string.Empty);
            return RemoveHtmlFontTag(s).Trim();
        }

        public static string RemoveTag(string text, string tag)
        {
            var idx = text.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
            while (idx >= 0)
            {
                var endIndex = text.IndexOf('>', idx + tag.Length);
                if (endIndex < idx) break;
                text = text.Remove(idx, endIndex - idx + 1);
                idx = text.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
            }
            return text;
        }

        public static string RemoveBrackets(string inputString)
        {
            return Regex.Replace(inputString, @"^[\[\(]|[\]\)]$", string.Empty).Trim();
        }

        public static bool FixIfInList(string name)
        {
            if (_listNames == null)
                return false;

            // _listName is already loaded with all names to UpperCase
            return _listNames.Contains(name.ToUpperInvariant());
        }

        public static void AddNameToList(string name)
        {
            if (name == null || name.Trim().Length == 0)
                return;

            var normalCase = name;
            name = name.ToUpperInvariant();
            if (!_listNames.Contains(name) && !_listNewName.Contains(normalCase))
            {
                _listNames.Add(name);
                _listNewName.Add(normalCase);
                SaveToXmlFile();
            }
        }

        private static void SaveToXmlFile()
        {
            if (_listNewName == null || _listNewName.Count == 0)
                return;
            try
            {
                var xelem = XDocument.Load(_filePath);
                foreach (var name in _listNewName)
                {
                    xelem.Root.Add(new XElement("name", name));
                }
                xelem.Save(_filePath);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
    }
}