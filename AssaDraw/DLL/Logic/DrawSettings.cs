using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using SubtitleEdit.Logic;

namespace AssaDraw.Logic
{
    public static class DrawSettings
    {
        public static bool Standalone { get; set; }
        public static bool UseScreenShotFromSe { get; set; } = true;
        public static bool HideSettingsAndTreeView { get; set; }
        public static Color ShapeLineColor { get; set; }
        public static Color ActiveShapeLineColor { get; set; }
        public static Color BackgroundColor { get; set; }
        public static Color ScreenSizeColor { get; set; }

        public static Color PointHelperColor = Color.FromArgb(100, Color.Green);
        public static Color PointColor = Color.FromArgb(100, Color.Red);

        private static void Initialize()
        {
            ShapeLineColor = Color.Black;
            ActiveShapeLineColor = Color.Red;
            BackgroundColor = Color.White;
            ScreenSizeColor = Color.Green;
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
                ScreenSizeColor = ColorTranslator.FromHtml(doc.DocumentElement.SelectSingleNode("ScreenSizeColor").InnerText);
                UseScreenShotFromSe = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("UseScreenShotFromSe").InnerText, CultureInfo.InvariantCulture);
                HideSettingsAndTreeView = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("HideSettingsAndTreeView").InnerText, CultureInfo.InvariantCulture);
                Configuration.LastColorPickerColor = ColorTranslator.FromHtml(doc.DocumentElement.SelectSingleNode("Color1").InnerText);
                Configuration.LastColorPickerColor1 = ColorTranslator.FromHtml(doc.DocumentElement.SelectSingleNode("Color2").InnerText);
                Configuration.LastColorPickerColor2 = ColorTranslator.FromHtml(doc.DocumentElement.SelectSingleNode("Color3").InnerText);
                Configuration.LastColorPickerColor3 = ColorTranslator.FromHtml(doc.DocumentElement.SelectSingleNode("Color4").InnerText);
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
                doc.LoadXml("<AssaDraw><ShapeLineColor/><ActiveShapeLineColor/><BackgroundColor/><ScreenSizeColor/><UseScreenShotFromSe/><HideSettingsAndTreeView/><Color1/><Color2/><Color3/><Color4/></AssaDraw>");
                doc.DocumentElement.SelectSingleNode("ShapeLineColor").InnerText = ColorTranslator.ToHtml(ShapeLineColor);
                doc.DocumentElement.SelectSingleNode("ActiveShapeLineColor").InnerText = ColorTranslator.ToHtml(ActiveShapeLineColor);
                doc.DocumentElement.SelectSingleNode("BackgroundColor").InnerText = ColorTranslator.ToHtml(BackgroundColor);
                doc.DocumentElement.SelectSingleNode("ScreenSizeColor").InnerText = ColorTranslator.ToHtml(ScreenSizeColor);
                doc.DocumentElement.SelectSingleNode("UseScreenShotFromSe").InnerText = UseScreenShotFromSe.ToString(CultureInfo.InvariantCulture);
                doc.DocumentElement.SelectSingleNode("HideSettingsAndTreeView").InnerText = HideSettingsAndTreeView.ToString(CultureInfo.InvariantCulture);
                doc.DocumentElement.SelectSingleNode("Color1").InnerText = ColorTranslator.ToHtml(Configuration.LastColorPickerColor);
                doc.DocumentElement.SelectSingleNode("Color2").InnerText = ColorTranslator.ToHtml(Configuration.LastColorPickerColor1);
                doc.DocumentElement.SelectSingleNode("Color3").InnerText = ColorTranslator.ToHtml(Configuration.LastColorPickerColor2);
                doc.DocumentElement.SelectSingleNode("Color4").InnerText = ColorTranslator.ToHtml(Configuration.LastColorPickerColor3);
                doc.Save(GetSettingsFileName());
            }
            catch
            {
                // ignore
            }
        }
    }
}
