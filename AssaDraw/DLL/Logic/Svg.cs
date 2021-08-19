using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;

namespace AssaDraw.Logic
{
    public class Svg
    {
        public static List<DrawShape> LoadSvg(string fileName)
        {
            var shapes = new List<DrawShape>();
            var xml = new XmlDocument();
            xml.Load(fileName);
            int layer = 100;
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ns", "http://www.w3.org/2000/svg");
            var gElements = xml.DocumentElement.SelectNodes("//ns:g", nsmgr);
            foreach (XmlNode gNode in gElements)
            {
                var pathNode = gNode.SelectSingleNode("ns:path", nsmgr);
                ReadPath(pathNode, layer, shapes);

                var rectNode = gNode.SelectSingleNode("ns:rect", nsmgr);
                ReadRect(rectNode, layer, shapes);

                layer--;
            }
            return shapes;
        }

        private static void ReadPath(XmlNode pathNode, int layer, List<DrawShape> shapes)
        {
            if (pathNode?.Attributes == null)
            {
                return;
            }

            var color = GetColor(pathNode);

            if (pathNode.Attributes["d"] != null)
            {
                try
                {
                    var drawCodes = pathNode.Attributes["d"].InnerText;
                    drawCodes = drawCodes.Replace("m", " m ");
                    drawCodes = drawCodes.Replace("M", " m ");
                    drawCodes = drawCodes.Replace("c", " b ");
                    drawCodes = drawCodes.Replace("C", " b ");
                    drawCodes = drawCodes.Replace("l", " l ");
                    drawCodes = drawCodes.Replace("L", " l ");
                    drawCodes = drawCodes.Replace("z", " ");
                    drawCodes = drawCodes.Replace("Z", " ");
                    drawCodes = drawCodes.Replace("-", " -");
                    drawCodes = drawCodes.Replace(",", " ");
                    drawCodes = drawCodes.Replace("  ", " ");
                    drawCodes = drawCodes.Replace("  ", " ");
                    drawCodes = drawCodes.Replace("  ", " ");
                    var newShapes = ImportShape(drawCodes, layer, color, false);
                    shapes.AddRange(newShapes);
                }
                catch
                {
                    // ignore
                }
            }
        }

        private static Color GetColor(XmlNode svgNode)
        {
            var color = Color.Black;
            var colorFound = false;
            if (svgNode.Attributes["fill"] != null)
            {
                try
                {
                    var colorCode = svgNode.Attributes["fill"].InnerText;
                    color = ColorTranslator.FromHtml(colorCode);
                    colorFound = true;
                }
                catch
                {
                    // ignore
                }
            }

            if (!colorFound)
            {
                if (svgNode.Attributes["style"] != null)
                {
                    // style="fill:white;fill-rule:nonzero;"
                    // style = "fill:rgb(38,87,135);fill-rule:nonzero;"
                    var styleText = svgNode.Attributes["style"].InnerText;
                    var regexFill = new Regex(@"fill\s*:\s*[a-zA-Z()\d,\s]*");
                    var match = regexFill.Match(styleText);
                    if (match.Success)
                    {
                        try
                        {
                            var colorCode = match.Value.Split(':')[1].Trim();

                            if (colorCode.StartsWith("rgb(", StringComparison.Ordinal))
                            {
                                var arr = colorCode.Remove(0, 4).TrimEnd(')')
                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                color = Color.FromArgb(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
                            }
                            else
                            {
                                color = ColorTranslator.FromHtml(colorCode);
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }
            }

            return color;
        }

        private static void ReadRect(XmlNode rectNode, int layer, List<DrawShape> shapes)
        {
            // <rect x="293.907" y="-343.56" width="4.795" height="19.617" style="fill:rgb(38,87,135);fill-rule:nonzero;"/>

            if (rectNode?.Attributes?["x"] == null || rectNode.Attributes["y"] == null ||
                rectNode.Attributes["width"] == null || rectNode.Attributes["height"] == null)
            {
                return;
            }

            var color = GetColor(rectNode);

            var xAsString = rectNode?.Attributes?["x"].InnerText;
            var yAsString = rectNode?.Attributes?["y"].InnerText;
            var widthAsString = rectNode?.Attributes?["width"].InnerText;
            var heightAsString = rectNode?.Attributes?["height"].InnerText;

            if (float.TryParse(xAsString, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var x) &&
                float.TryParse(yAsString, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var y) &&
                float.TryParse(widthAsString, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var width) &&
                float.TryParse(heightAsString, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var height))
            {
                var drawCodes = $"m {x.ToString(CultureInfo.InvariantCulture)} {y.ToString(CultureInfo.InvariantCulture)} l {(x + width).ToString(CultureInfo.InvariantCulture)} {y.ToString(CultureInfo.InvariantCulture)}  {(x + width).ToString(CultureInfo.InvariantCulture)} {(y + height).ToString(CultureInfo.InvariantCulture)} {x.ToString(CultureInfo.InvariantCulture)} {(y + height).ToString(CultureInfo.InvariantCulture)}";
                var newShapes = ImportShape(drawCodes, layer, color, false);
                shapes.AddRange(newShapes);
            }
        }

        private static List<DrawShape> ImportShape(string drawCodes, int layer, Color c, bool isEraser)
        {
            var arr = drawCodes.Split();
            int i = 0;
            int bezierCount = 0;
            var state = DrawCoordinateType.None;
            DrawCoordinate moveCoordinate = null;
            var drawShapes = new List<DrawShape>();
            DrawShape drawShape = null;
            while (i < arr.Length)
            {
                var v = arr[i];
                if (v == "m" && i < arr.Length - 2 &&
                    float.TryParse(arr[i + 1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var mX) &&
                    float.TryParse(arr[i + 2], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var mY))
                {
                    bezierCount = 0;
                    moveCoordinate = new DrawCoordinate(null, DrawCoordinateType.Move, (int)Math.Round(mX), (int)Math.Round(mY), DrawSettings.PointColor);
                    state = DrawCoordinateType.Move;
                    i += 2;
                }
                else if (v == "l")
                {
                    state = DrawCoordinateType.Line;
                    bezierCount = 0;
                    if (moveCoordinate != null)
                    {
                        drawShape = new DrawShape();
                        drawShape.Layer = layer;
                        drawShape.ForeColor = c;
                        drawShape.IsEraser = isEraser;
                        drawShape.AddPoint(state, moveCoordinate.X, moveCoordinate.Y, DrawSettings.PointColor);
                        moveCoordinate = null;
                        drawShapes.Add(drawShape);
                    }
                }
                else if (v == "b")
                {
                    state = DrawCoordinateType.BezierCurve;
                    if (moveCoordinate != null)
                    {
                        drawShape = new DrawShape();
                        drawShape.Layer = layer;
                        drawShape.ForeColor = c;
                        drawShape.IsEraser = isEraser;
                        drawShape.AddPoint(state, moveCoordinate.X, moveCoordinate.Y, DrawSettings.PointColor);
                        moveCoordinate = null;
                        drawShapes.Add(drawShape);
                    }
                    bezierCount = 1;
                }
                else if (state == DrawCoordinateType.Line && drawShape != null && i < arr.Length - 1 &&
                    float.TryParse(arr[i + 0], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var lX) &&
                    float.TryParse(arr[i + 1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var lY))
                {
                    drawShape.AddPoint(state, (int)Math.Round(lX), (int)Math.Round(lY), DrawSettings.PointColor);
                    i++;
                }
                else if (state == DrawCoordinateType.BezierCurve && drawShape != null &&
                    float.TryParse(arr[i + 0], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var bX) &&
                    float.TryParse(arr[i + 1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var bY))
                {
                    bezierCount++;
                    if (bezierCount > 3)
                    {
                        bezierCount = 1;
                    }

                    if (bezierCount == 2)
                    {
                        drawShape.AddPoint(DrawCoordinateType.BezierCurveSupport1, (int)Math.Round(bX), (int)Math.Round(bY), DrawSettings.PointHelperColor);
                    }
                    else if (bezierCount == 3)
                    {
                        drawShape.AddPoint(DrawCoordinateType.BezierCurveSupport2, (int)Math.Round(bX), (int)Math.Round(bY), DrawSettings.PointHelperColor);
                    }
                    else
                    {
                        drawShape.AddPoint(state, (int)Math.Round(bX), (int)Math.Round(bY), DrawSettings.PointColor);
                    }
                    i++;
                }

                i++;
            }

            return drawShapes;
        }
    }
}
