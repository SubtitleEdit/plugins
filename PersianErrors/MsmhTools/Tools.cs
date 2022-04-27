using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MsmhTools
{
    public static class Tools
    {
        public static void WriteAllText(string filePath, string fileContent, Encoding encoding)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter writer = new StreamWriter(fileStream, encoding);
            //fileStream.SetLength(0); // Overwrite File When FileMode is FileMode.OpenOrCreate
            writer.AutoFlush = true;
            writer.Write(fileContent);
            writer.Close();
            fileStream.Close();
        }
        public static string ToXmlWithWriteMode(this DataSet ds, XmlWriteMode xmlWriteMode)
        {
            var ms = new MemoryStream();
            TextWriter sw = new StreamWriter(ms);
            ds.WriteXml(sw, xmlWriteMode);
            sw.Close();
            ms.Close();
            return new UTF8Encoding(false).GetString(ms.ToArray());
        }
        public static bool IsBool(this string s)
        {
            if (bool.TryParse(s, out _))
                return true;
            return false;
        }
        public static string SettingsFilePath()
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string pluginFolder = Path.Combine(assemblyFolder, "Plugins");
            string dllName = Assembly.GetExecutingAssembly().GetName().Name.ToString();
            string settingsFile = Path.Combine(pluginFolder, dllName + ".xml");
            return settingsFile;
        }
        public static bool IsSettingsValid(string filePath)
        {
            if (File.Exists(filePath))
            {
                bool isXmlValid = IsValidXML(File.ReadAllText(filePath));
                if (isXmlValid == true)
                {
                    Console.WriteLine("Settings File Is Valid.");
                    return true;
                }
                else
                {
                    Console.WriteLine("Settings File Is Not Valid.");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Settings File Not Exist.");
                return false;
            }
        }
        public static bool IsValidXML(string content)
        {
            try
            {
                if (string.IsNullOrEmpty(content) == false)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(content);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (XmlException ex)
            {
                Console.WriteLine("XML Error: " + ex.Message);
                return false;
            }
        }
        public static DataSet ToDataSet(string xmlFile, XmlReadMode xmlReadMode)
        {
            DataSet ds = new DataSet();
            ds.ReadXml(xmlFile, xmlReadMode);
            return ds;
        }
        public static void SetToolTip(this Control control, string titleMessage, string bodyMessage)
        {
            ToolTip tt = new ToolTip();
            tt.ToolTipIcon = ToolTipIcon.Info;
            tt.IsBalloon = false;
            tt.ShowAlways = true;
            tt.UseAnimation = true;
            tt.UseFading = true;
            tt.InitialDelay = 1000;
            tt.AutoPopDelay = 300;
            tt.AutomaticDelay = 300;
            tt.ToolTipTitle = titleMessage;
            tt.SetToolTip(control, bodyMessage);
        }
        public static void InvokeIt(this ISynchronizeInvoke sync, Action action)
        {
            // If the invoke is not required, then invoke here and get out.
            if (!sync.InvokeRequired)
            {
                action();
                return;
            }
            sync.Invoke(action, Array.Empty<object>());
            // Usage:
            // textBox1.InvokeIt(() => textBox1.Text = text);
        }
    }
}
