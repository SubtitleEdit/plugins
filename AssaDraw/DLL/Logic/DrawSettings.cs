using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Xml;

namespace AssaDraw.Logic
{
    public static class DrawSettings
    {
        public static Color ShapeLineColor { get; set; }
        public static Color ActiveShapeLineColor { get; set; }
        public static Color BackgroundColor { get; set; }
        public static Color OffScreenColor { get; set; }

        private static void Initialize()
        {
            ShapeLineColor = Color.Black;
            ActiveShapeLineColor = Color.Red;
            BackgroundColor = Color.LightGray;
            OffScreenColor = Color.White;
        }

        private static string GetSettingsFileName()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var path = Path.GetDirectoryName(codeBase);
            if (path != null && path.StartsWith("file:\\", StringComparison.Ordinal))
            {
                path = path.Remove(0, 6);
            }

            if (codeBase.EndsWith("AssaDraw.exe", StringComparison.InvariantCultureIgnoreCase))
            {
                return Path.Combine(path, "AssaDraw.xml");
            }

            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
            {
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            }

            return Path.Combine(path, "AssaDraw.xml");
        }

        public static void LoadSettings()
        {
            Initialize();
            try
            {
                var doc = new XmlDocument();
                doc.Load(GetSettingsFileName());
                ShapeLineColor = ColorTranslator.FromHtml(doc.DocumentElement.SelectSingleNode("ShapeLineColor").InnerText);
                ActiveShapeLineColor = ColorTranslator.FromHtml(doc.DocumentElement.SelectSingleNode("ActiveShapeLineColor").InnerText);
                BackgroundColor = ColorTranslator.FromHtml(doc.DocumentElement.SelectSingleNode("BackgroundColor").InnerText);
                OffScreenColor = ColorTranslator.FromHtml(doc.DocumentElement.SelectSingleNode("OffScreenColor").InnerText);
            }
            catch
            {
                // ignore
            }
        }

        public static void SaveSettings()
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml("<AssaDraw><ShapeLineColor/><ActiveShapeLineColor/><BackgroundColor/><OffScreenColor/></AssaDraw>");
                doc.DocumentElement.SelectSingleNode("ShapeLineColor").InnerText = ColorTranslator.ToHtml(ShapeLineColor);
                doc.DocumentElement.SelectSingleNode("ActiveShapeLineColor").InnerText = ColorTranslator.ToHtml(ActiveShapeLineColor);
                doc.DocumentElement.SelectSingleNode("BackgroundColor").InnerText = ColorTranslator.ToHtml(BackgroundColor);
                doc.DocumentElement.SelectSingleNode("OffScreenColor").InnerText = ColorTranslator.ToHtml(OffScreenColor);
                doc.Save(GetSettingsFileName());
            }
            catch
            {
                // ignore
            }
        }
    }
}
