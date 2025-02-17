using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class ColorConfig // : Configuration<ColorConfig>
    {
        /// <summary>
        /// Narrators color.
        /// </summary>
        public int Narrator { get; set; }

        /// <summary>
        /// Moods color.
        /// </summary>
        public int Moods { get; set; }

        /// <summary>
        /// Music paragraph color
        /// </summary>
        public int Music { get; set; }

        private ColorConfig()
        {
        }

        public static ColorConfig LoadOrCreateConfigurations()
        {
            // old config file
            try
            {
                File.Delete("hicolor.xml");
            }
            catch
            {
            }

            var configFile = GetConfigFile();
            if (File.Exists(configFile))
            {
                var xdoc = XDocument.Load(configFile);
                var colorConfig = new ColorConfig()
                {
                    Narrator = Convert.ToInt32(xdoc.Root.Element("Narrator").Value),
                    Moods = Convert.ToInt32(xdoc.Root.Element("Moods").Value),
                    Music = Convert.ToInt32(xdoc.Root.Element("Music").Value)
                };
                return colorConfig;
            }

            return new ColorConfig();
        }

        public void Save()
        {
            try
            {
                var configFile = GetConfigFile();
                XDocument xdoc = File.Exists(configFile) ? XDocument.Load(configFile) : new XDocument(new XElement("HIColorer"));
                xdoc.Root.Element("Narrator").Value = Narrator.ToString();
                xdoc.Root.Element("Moods").Value = Moods.ToString();
                xdoc.Root.Element("Music").Value = Music.ToString();
                xdoc.Save(configFile);
            }
            catch (Exception e)
            {
                // ignore
            }
        }

        private static string GetConfigFile() => Path.Combine(Configuration.PluginsDirectory, "hicolor-config.xml");
    }
}