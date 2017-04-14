using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal static class Utilities
    {

        #region ExtensionMethods

        public static string[] SplitToLines(this string s)
        {
            return s.Replace(Environment.NewLine, "\n").Replace('\r', '\n').Split('\n');
        }

        #endregion


        private static readonly string DictionaryFolder = GetDicTionaryFolder();
        private static List<string> _listNames = LoadNames(Path.Combine(DictionaryFolder, "names.xml")); // Uppercase names
        private static List<string> _listNewName = LoadNames(Path.Combine(DictionaryFolder, "narratorNames.xml"));

        public static ICollection<string> ListNames
        {
            get { return _listNames.Concat(_listNewName).ToList(); }
        }

        public static List<string> LoadNames(string fileName)
        {
            if (_listNames != null && fileName.EndsWith("names.xml", StringComparison.OrdinalIgnoreCase))
                _listNames.Clear();

            if (File.Exists(fileName))
            {
                return (from elem in XElement.Load(fileName).Elements()
                        select elem.Value.ToUpperInvariant()).ToList();
            }
            else
            {
                try
                {
                    // Create file (user defined named)
                    var xdoc = new XDocument(
                        new XElement("names",
                            new XElement("name", "JOHN"),
                            new XElement("name", "MAN"),
                            new XElement("name", "WOMAN"),
                            new XElement("name", "CAITLIN")
                            ));

                    if (!Directory.Exists(DictionaryFolder))
                        Directory.CreateDirectory(DictionaryFolder);
                    xdoc.Save(Path.Combine(DictionaryFolder, "narratorNames.xml"), SaveOptions.None);
                    return (from elem in xdoc.Root.Elements()
                            select elem.Value.ToUpperInvariant()).ToList();
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
            return new List<string>();
        }

        public static string GetDicTionaryFolder()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\", StringComparison.Ordinal))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Dictionaries");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtiitle Edit", "Dictionary"));
            return path;
        }

        public static int NumberOfLines(string text)
        {
            var ln = 0;
            var idx = 0;
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

        public static string RemoveHtmlFontTag(string s)
        {
            s = Regex.Replace(s, "(?i)</?font>", string.Empty);
            return RemoveTag(s, "<font");
        }

        public static string RemoveHtmlTags(string s)
        {
            if (string.IsNullOrEmpty(s) || s.IndexOf('<') < 0)
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

        public static bool IsInListName(string name)
        {
            if (ListNames == null)
                return false;

            // ListNames is already loaded with all names to UpperCase
            return ListNames.Contains(name.ToUpperInvariant());
        }

        public static void AddNameToList(string name)
        {
            if (name == null || name.Trim().Length == 0)
                return;
            //var normalCase = Regex.Replace(name.ToLowerInvariant(), "\\b\\w", x => x.Value.ToUpperInvariant(), RegexOptions.Compiled);
            name = name.ToUpperInvariant();
            if (!ListNames.Contains(name))
            {
                _listNewName.Add(name);
                SaveToXmlFile();
            }
        }

        private static void SaveToXmlFile()
        {
            if (_listNewName == null || _listNewName.Count == 0)
                return;
            var filePath = Path.Combine(DictionaryFolder, "narratorNames.xml");
            try
            {
                var xelem = XDocument.Load(filePath);
                xelem.Root.ReplaceAll((from name in _listNewName
                                       select new XElement("name", name)).ToList());
                /*
                    foreach (var name in _listNewName)
                        xelem.Root.Add(new XElement("name", name));*/

                xelem.Save(filePath);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        public static int CountTagInText(string text, char tag)
        {
            var total = 0;
            var idx = text.IndexOf(tag);
            while (idx >= 0)
            {
                total++;
                idx = text.IndexOf(tag, idx + 1);
            }
            return total;
        }

        public static int CountTagInText(string text, string tag)
        {
            var total = 0;
            var idx = text.IndexOf(tag, StringComparison.Ordinal);
            while (idx >= 0)
            {
                total++;
                idx = text.IndexOf(tag, idx + 1, StringComparison.Ordinal);
            }
            return total;
        }

        public static bool IsBetweenNumbers(string text, int pos)
        {
            if (text.Length <= 2 || pos + 1 >= text.Length || pos - 1 < 0)
                return false;
            return (text[pos + 1] >= 0x30 && text[pos + 1] <= 0x39) && (text[pos - 1] >= 0x30 && text[pos - 1] <= 0x39);
        }

        public static bool IsStartsWithHtmlTag(string text)
        {
            if (string.IsNullOrEmpty(text) || text[0] != '<')
                return false;
            return (text[0] == '<' && text[2] == '>');
        }

        public static bool IsNewLineStartsWithHtml(string text)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains('<') || !text.Contains(Environment.NewLine))
                return false;
            var newLine = text.Substring(text.IndexOf(Environment.NewLine) + 2);
            return IsStartsWithHtmlTag(newLine);
        }
    }
}