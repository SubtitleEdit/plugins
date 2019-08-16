using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace OnlineCasing
{
    public static class SettingUtils
    {
        //public static readonly string SettingFile = Path.Combine(FileUtils.Plugins, "online-casing.xml");

        public static readonly string SettingFile = Path.Combine(FileUtils.Plugins, "onlie-casing-setting.json");

        public static string GetApiKey()
        {
            EnsureSettingFile();
            string apiKeyBase64 = XElement.Load(SettingFile).Element("apikey").Value;
            return Encoding.Default.GetString(Convert.FromBase64String(apiKeyBase64));
        }

        private static void EnsureSettingFile()
        {
            if (File.Exists(SettingFile))
            {
                return;
            }
            try
            {
                new XElement("online-casing", new XElement("apikey")).Save(SettingFile);

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public static void UpdateApiKey(string apiKeyBase64)
        {
            EnsureSettingFile();

            // converto to base64 
            // store in xml file
            XElement xEl = XElement.Load(SettingFile);
            xEl.Element("apikey").Value = apiKeyBase64;
            try
            {
                xEl.Save(SettingFile);

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
}
