using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace AssaDraw.Logic
{
    public static class SvgWriter
    {
        private const string SvgFileTemplate =
@"<svg id='svg' version='1.1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' width='[WIDTH]' height='[HEIGHT]' viewBox='0, 0, [WIDTH], [HEIGHT]'>
    <g id='svgg'>
    </g>
</svg>";

        public static void SaveSvg(string fileName, List<DrawShape> shapes, int width, int height)
        {
            var xml = SvgFileTemplate
                .Replace("[WIDTH]", width.ToString(CultureInfo.InvariantCulture))
                .Replace("[HEIGHT]", height.ToString(CultureInfo.InvariantCulture))
                .Replace('\'', '"');

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            var namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            namespaceManager.AddNamespace("ns", "http://www.w3.org/2000/svg");
            var gElement = xmlDocument.DocumentElement.SelectSingleNode("//ns:g", namespaceManager);

            var sortedShapes = shapes.OrderBy(p => p.Layer).ToList();
            for (var index = 0; index < sortedShapes.Count; index++)
            {
                var shape = sortedShapes[index];
                // <path id="path0" d="" stroke="none" fill="#ffffff" fill-rule="evenodd"></path>
                XmlNode pathNode = xmlDocument.CreateNode(XmlNodeType.Element, "path", "http://www.w3.org/2000/svg");
                XmlAttribute id = xmlDocument.CreateAttribute("id");
                XmlAttribute d = xmlDocument.CreateAttribute("d");
                XmlAttribute stroke = xmlDocument.CreateAttribute("stroke");
                XmlAttribute fill = xmlDocument.CreateAttribute("fill");
                XmlAttribute fillRule = xmlDocument.CreateAttribute("fill-rule");

                id.InnerText = $"path{index:000)}";
                d.InnerText = shape.ToAssa().Replace("b", "c").ToUpper() + " Z";
                stroke.InnerText = "none";
                fill.InnerText = ColorTranslator.ToHtml(shape.ForeColor == Color.Transparent ? Color.White : shape.ForeColor);
                fillRule.InnerText = "evenodd";

                pathNode.Attributes.Append(id);
                pathNode.Attributes.Append(d);
                pathNode.Attributes.Append(stroke);
                pathNode.Attributes.Append(fill);
                pathNode.Attributes.Append(fillRule);
                gElement.AppendChild(pathNode);
            }

            xmlDocument.Save(fileName);
        }
    }
}
