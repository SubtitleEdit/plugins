using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class NameList
    {
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


        public static bool IsInListName(string name)
        {
            if (ListNames == null)
                return false;

            // ListNames is already loaded with all names to UpperCase
            return ListNames.Contains(name.ToUpperInvariant());
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

    }
}
