using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MsmhTools
{
    public static class Extensions
    {
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// Creates color with corrected brightness.
        /// </summary>
        /// <param name="color">Color to correct.</param>
        /// <param name="correctionFactor">The brightness correction factor. Must be between -1 and 1. 
        /// Negative values produce darker colors.</param>
        /// <returns>
        /// Corrected <see cref="Color"/> structure.
        /// </returns>
        public static Color ChangeBrightness(this Color color, float correctionFactor)
        {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }
            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// Check Color is Light or Dark.
        /// </summary>
        /// <returns>
        /// Returns "Dark" or "Light" as string.
        /// </returns>
        public static string DarkOrLight(this Color color)
        {
            if (color.R * 0.2126 + color.G * 0.7152 + color.B * 0.0722 < 255 / 2)
            {
                return "Dark";
            }
            else
            {
                return "Light";
            }
        }
    }
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
        public static Control GetTopParent(Control control)
        {
            Control parent = control;
            if (control.Parent != null)
            {
                parent = control.Parent;
                if (parent.Parent != null)
                    while (parent.Parent != null)
                        parent = parent.Parent;
            }
            return parent;
        }
    }
}
