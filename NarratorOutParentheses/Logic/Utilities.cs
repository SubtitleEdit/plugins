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
        private static readonly List<string> _listNames = LoadName();
        private static List<string> LoadName()
        {
            var settingFile = GetSettingsFileName();
            if (File.Exists(settingFile))
            {
                return (from elem in XElement.Load(settingFile).Elements()
                        select elem.Value).ToList();
            }
            else
            {
                try
                {
                    // Create file
                    var xdoc = new XDocument(
                        new XElement("names",
                            new XElement("name", "CISCO"),
                            new XElement("name", "JOHN"),
                            new XElement("name", "MAN"),
                            new XElement("name", "WOMAN")
                            ));
                    /*
                    Directory.CreateDirectory(Path.GetDirectoryName(settingFile));
                    File.Create(settingFile);*/

                    xdoc.Save(settingFile, SaveOptions.None);
                    return (from elem in xdoc.Root.Elements()
                            select elem.Value).ToList();
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
            return new List<string>();
        }

        private static string GetSettingsFileName()
        {
            // "C:\Users\Ivandrofly\Desktop\SubtitleEdit\Plugins\SeLinesUnbreaker.xml"
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\"))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "NameList.xml");
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
            s = RemoveParagraphTag(s);
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

        public static string RemoveParagraphTag(string s)
        {
            s = Regex.Replace(s, "(?i)</?p>", string.Empty);
            return RemoveTag(s, "<p");
        }

        public static bool FixIfInList(string name)
        {
            var settingFile = GetSettingsFileName();
            var xml = XElement.Load(settingFile);
            foreach (var item in xml.Elements())
            {
                if (item.Value == name.ToUpper())
                    return true;
            }
            return false;
        }

        public static void AddNameToList(string name)
        {
            // Todo: make this async
            if (name == null)
                return;

            name = name.ToUpper();
            if (!_listNames.Contains(name))
            {
                _listNames.Add(name);
                SaveToXmlFile();
            }
        }


        private static void SaveToXmlFile()
        {
            if (_listNames == null || _listNames.Count == 0)
                return;
            var settingFile = GetSettingsFileName();

            // <names></names>
            XElement xelem = XElement.Load(settingFile);
            foreach (var name in _listNames)
            {
                xelem.Add(new XElement("name", name));
            }
            try
            {
                // todo: this should be inside try catch
                xelem.Save(settingFile);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
    }
}