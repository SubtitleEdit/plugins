using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            {
                return;
            }

            var filePath = Path.Combine(FileUtils.Dictionaries, "narratorNames.xml");
            try
            {
                var xelem = XDocument.Load(filePath);
                xelem.Root.ReplaceAll((from name in _listNewName
                                       select new XElement("name", name)).ToList());
                /*
                    foreach (var name in _listNewName)
                        xelem.Root.Add(new XElement("name", name));
                */

                xelem.Save(filePath);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        public static bool Contains(string name)
        {
            if (ListNames == null)
                return false;

            // ListNames is already loaded with all names to UpperCase
            return ListNames.Contains(name.ToUpperInvariant());
        }

        private static List<string> _listNames = LoadNames(Path.Combine(FileUtils.Dictionaries, "names.xml")); // Uppercase names
        private static List<string> _listNewName = LoadNames(Path.Combine(FileUtils.Dictionaries, "narratorNames.xml"));

        public static ICollection<string> ListNames => _listNames.Concat(_listNewName).ToList();

        public static List<string> LoadNames(string fileName)
        {
            if (_listNames != null && fileName.EndsWith("names.xml", StringComparison.OrdinalIgnoreCase))
            {
                _listNames.Clear();
            }

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

                    if (!Directory.Exists(FileUtils.Dictionaries))
                    {
                        Directory.CreateDirectory(FileUtils.Dictionaries);
                    }

                    xdoc.Save(Path.Combine(FileUtils.Dictionaries, "narratorNames.xml"), SaveOptions.None);
                    return (from elem in xdoc.Root.Elements()
                            select elem.Value.ToUpperInvariant()).ToList();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
            return new List<string>();
        }

    }
}
