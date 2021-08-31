using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace AssaDraw.Logic
{
    /*
    "d" attribute (path) SVG defines 6 types of path commands, for a total of 20 commands:

    MoveTo: M, m
    LineTo: L, l, H, h, V, v
    Cubic Bézier Curve: C, c, S, s
    Quadratic Bézier Curve: Q, q, T, t (3 points - only one control point)
    Elliptical Arc Curve: A, a 
    ClosePath: Z, z

    Note: Commands are case-sensitive. 
    An upper-case command specifies absolute coordinates, while a lower-case command specifies coordinates relative to the current position.

    MoveTo instructions can be thought of as picking up the drawing instrument, and setting it down somewhere else—in other words, moving the current point

    "H/h": horizontal line - only x

    "V/v": vertical line: only y

     */
    public class SvgReader
    {
        public static List<DrawShape> LoadSvg(string fileName)
        {
            var shapes = new List<DrawShape>();
            var xml = new XmlDocument();
            xml.Load(fileName);
            int layer = 100;
            var namespaceManager = new XmlNamespaceManager(xml.NameTable);
            namespaceManager.AddNamespace("ns", "http://www.w3.org/2000/svg");
            if (xml.DocumentElement != null)
            {
                var elements = xml.DocumentElement.SelectNodes("//*", namespaceManager);
                if (elements == null)
                {
                    return shapes;
                }

                foreach (var node in elements)
                {
                    if (node is XmlElement element)
                    {
                        if (element.Name == "path")
                        {
                            ReadPath(element, layer, shapes);
                            layer--;
                        }
                        else if (element.Name == "rect")
                        {
                            ReadRect(element, layer, shapes);
                            layer--;
                        }
                        else if (element.Name == "circle")
                        {
                            ReadCircle(element, layer, shapes);
                            layer--;
                        }
                    }
                }
            }

            return shapes;
        }

        private static void ReadCircle(XmlElement circleNode, int layer, List<DrawShape> shapes)
        {
            // <circle r="3.625" cy="472.13062" cx="335.03351"

            if (circleNode?.Attributes["cx"] == null || circleNode.Attributes["cy"] == null || circleNode.Attributes["r"] == null)
            {
                return;
            }

            const float bezier4Point = 0.552284749831f;
            var color = GetColor(circleNode);

            var xAsString = circleNode.Attributes["cx"].InnerText;
            var yAsString = circleNode.Attributes["cy"].InnerText;
            var radiusString = circleNode.Attributes["r"].InnerText;

            if (float.TryParse(xAsString, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var x) &&
                float.TryParse(yAsString, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var y) &&
                float.TryParse(radiusString, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var radius))
            {
                // draw circle using 4 points + 8 support points - see https://stackoverflow.com/questions/1734745/how-to-create-circle-with-b%C3%A9zier-curves
                var pointTopX = x;
                var pointTopY = y - radius;

                var pointTopXs1 = x + radius * bezier4Point;
                var pointTopYs1 = y - radius;

                var pointTopXs2 = x + radius;
                var pointTopYs2 = y - radius * bezier4Point;

                var pointRightX = x + radius;
                var pointRightY = y;

                var pointRightXs1 = x + radius;
                var pointRightYs1 = y + radius * bezier4Point;

                var pointRightXs2 = x + radius * bezier4Point;
                var pointRightYs2 = y + radius;

                var pointBottomX = x;
                var pointBottomY = y + radius;

                var pointBottomXs1 = x - radius * bezier4Point;
                var pointBottomYs1 = y + radius;

                var pointBottomXs2 = x - radius;
                var pointBottomYs2 = y + radius * bezier4Point;

                var pointLeftX = x - radius;
                var pointLeftY = y;

                var pointLeftXs1 = x - radius;
                var pointLeftYs1 = y - radius * bezier4Point;

                var pointLeftXs2 = x - radius * bezier4Point;
                var pointLeftYs2 = y - radius;

                var drawCodes =
                    $"m {pointTopX.ToString(CultureInfo.InvariantCulture)} {pointTopY.ToString(CultureInfo.InvariantCulture)} b" +
                    $" {pointTopXs1.ToString(CultureInfo.InvariantCulture)} {pointTopYs1.ToString(CultureInfo.InvariantCulture)}" +
                    $" {pointTopXs2.ToString(CultureInfo.InvariantCulture)} {pointTopYs2.ToString(CultureInfo.InvariantCulture)}" +
                    $" {pointRightX.ToString(CultureInfo.InvariantCulture)} {pointRightY.ToString(CultureInfo.InvariantCulture)}" +
                    $" {pointRightXs1.ToString(CultureInfo.InvariantCulture)} {pointRightYs1.ToString(CultureInfo.InvariantCulture)}" +
                    $" {pointRightXs2.ToString(CultureInfo.InvariantCulture)} {pointRightYs2.ToString(CultureInfo.InvariantCulture)}" +
                    $" {pointBottomX.ToString(CultureInfo.InvariantCulture)} {pointBottomY.ToString(CultureInfo.InvariantCulture)}" +
                    $" {pointBottomXs1.ToString(CultureInfo.InvariantCulture)} {pointBottomYs1.ToString(CultureInfo.InvariantCulture)}" +
                    $" {pointBottomXs2.ToString(CultureInfo.InvariantCulture)} {pointBottomYs2.ToString(CultureInfo.InvariantCulture)}" +
                    $" {pointLeftX.ToString(CultureInfo.InvariantCulture)} {pointLeftY.ToString(CultureInfo.InvariantCulture)}" +
                    $" {pointLeftXs1.ToString(CultureInfo.InvariantCulture)} {pointLeftYs1.ToString(CultureInfo.InvariantCulture)}" +
                    $" {pointLeftXs2.ToString(CultureInfo.InvariantCulture)} {pointLeftYs2.ToString(CultureInfo.InvariantCulture)}" +
                    $" {pointTopX.ToString(CultureInfo.InvariantCulture)} {pointTopY.ToString(CultureInfo.InvariantCulture)}";
                var newShapes = ImportShape(drawCodes, layer, color, false);
                shapes.AddRange(newShapes);
            }
        }

        /// <summary>
        /// Convert the 20 path commands the shapes.
        /// </summary>
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
                    drawCodes = drawCodes.Replace("M", " M ");
                    drawCodes = drawCodes.Replace("c", " c ");
                    drawCodes = drawCodes.Replace("C", " C ");
                    drawCodes = drawCodes.Replace("s", " s ");
                    drawCodes = drawCodes.Replace("S", " S ");
                    drawCodes = drawCodes.Replace("l", " l ");
                    drawCodes = drawCodes.Replace("L", " L ");
                    drawCodes = drawCodes.Replace("V", " V ");
                    drawCodes = drawCodes.Replace("v", " v ");
                    drawCodes = drawCodes.Replace("H", " H ");
                    drawCodes = drawCodes.Replace("h", " h ");
                    drawCodes = drawCodes.Replace("Q", " Q ");
                    drawCodes = drawCodes.Replace("q", " q ");
                    drawCodes = drawCodes.Replace("T", " T ");
                    drawCodes = drawCodes.Replace("t", " t ");
                    drawCodes = drawCodes.Replace("A", " A ");
                    drawCodes = drawCodes.Replace("a", " a ");
                    drawCodes = drawCodes.Replace("Z", " Z ");
                    drawCodes = drawCodes.Replace("z", " z ");
                    drawCodes = drawCodes.Replace("-", " -");
                    drawCodes = drawCodes.Replace(",", " ");

                    var drawCodeList = drawCodes.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var newDrawCodeList = new List<string>();
                    var x = 0.0f;
                    var y = 0.0f;
                    var index = 0;
                    var countSinceLastCode = 0;
                    var lastCommandCode = string.Empty;
                    var relativeOn = false;
                    var lastMoveX = 0.0f;
                    var lastMoveY = 0.0f;
                    while (index < drawCodeList.Count)
                    {
                        var code = drawCodeList[index];
                        if ("MmCcSsLlVvHhQqTtAaZz".Contains(code))
                        {
                            relativeOn = "mcslvhqtaz".Contains(code);
                            countSinceLastCode = 0;
                            lastCommandCode = code;
                        }

                        countSinceLastCode++;

                        if (code == "M" && index + 2 < drawCodeList.Count &&
                            float.TryParse(drawCodeList[index + 1], NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var x1) &&
                            float.TryParse(drawCodeList[index + 2], NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var y1))
                        {
                            x = x1;
                            y = y1;
                            lastMoveX = x;
                            lastMoveY = y;
                            newDrawCodeList.Add(code);
                            newDrawCodeList.Add(x.ToString(CultureInfo.InvariantCulture));
                            newDrawCodeList.Add(y.ToString(CultureInfo.InvariantCulture));
                            index += 3;
                            continue;
                        }

                        if (code == "m" && index + 2 < drawCodeList.Count &&
                            float.TryParse(drawCodeList[index + 1], NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var x2) &&
                            float.TryParse(drawCodeList[index + 2], NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var y2))
                        {
                            x += x2;
                            y += y2;
                            lastMoveX = x;
                            lastMoveY = y;
                            newDrawCodeList.Add(code);
                            newDrawCodeList.Add(x.ToString(CultureInfo.InvariantCulture));
                            newDrawCodeList.Add(y.ToString(CultureInfo.InvariantCulture));
                            index += 3;
                            continue;
                        }

                        if (code == "Z" || code == "z")
                        {
                            x = lastMoveX;
                            y = lastMoveY;
                        }
                        else if (relativeOn && float.TryParse(code, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var relativeNumber))
                        {
                            if (lastCommandCode == "v")
                            {
                                y += relativeNumber;
                                newDrawCodeList.Add(x.ToString(CultureInfo.InvariantCulture));
                                newDrawCodeList.Add(y.ToString(CultureInfo.InvariantCulture));
                                index++;
                                continue;
                            }

                            if (lastCommandCode == "h")
                            {
                                x += relativeNumber;
                                newDrawCodeList.Add(x.ToString(CultureInfo.InvariantCulture));
                                newDrawCodeList.Add(y.ToString(CultureInfo.InvariantCulture));
                                index++;
                                continue;
                            }

                            if (countSinceLastCode % 2 == 0)
                            {
                                x += relativeNumber;
                                code = x.ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                y += relativeNumber;
                                code = y.ToString(CultureInfo.InvariantCulture);
                            }
                        }

                        if (!string.IsNullOrEmpty(code))
                        {
                            newDrawCodeList.Add(code);
                        }

                        if ((lastCommandCode.ToUpperInvariant() == "Q" || lastCommandCode.ToUpperInvariant() == "T") && countSinceLastCode == 3)
                        {
                            newDrawCodeList.Add(newDrawCodeList[newDrawCodeList.Count - 2]);
                            newDrawCodeList.Add(newDrawCodeList[newDrawCodeList.Count - 2]);
                        }

                        index++;
                    }

                    drawCodes = string.Join(" ", newDrawCodeList);

                    drawCodes = drawCodes.Replace("m", " m ");
                    drawCodes = drawCodes.Replace("M", " m ");
                    drawCodes = drawCodes.Replace("c", " b ");
                    drawCodes = drawCodes.Replace("C", " b ");
                    drawCodes = drawCodes.Replace("s", " b ");
                    drawCodes = drawCodes.Replace("S", " b ");
                    drawCodes = drawCodes.Replace("q", " b ");
                    drawCodes = drawCodes.Replace("Q", " b ");
                    drawCodes = drawCodes.Replace("t", " b ");
                    drawCodes = drawCodes.Replace("T", " b ");
                    drawCodes = drawCodes.Replace("l", " l ");
                    drawCodes = drawCodes.Replace("L", " l ");
                    drawCodes = drawCodes.Replace("v", " l ");
                    drawCodes = drawCodes.Replace("V", " l ");
                    drawCodes = drawCodes.Replace("h", " l ");
                    drawCodes = drawCodes.Replace("H", " l ");
                    drawCodes = drawCodes.Replace("z", " ");
                    drawCodes = drawCodes.Replace("Z", " ");
                    drawCodes = drawCodes.Replace("-", " -");
                    drawCodes = drawCodes.Replace(",", " ");
                    drawCodeList = drawCodes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    if (drawCodeList.Count > 3 && drawCodeList[0] == "m" &&
                        float.TryParse(drawCodeList[3], NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var _))
                    {
                        drawCodeList.Insert(3, "l");
                        drawCodes = string.Join(" ", drawCodeList);
                    }

                    drawCodes = drawCodes.Trim().Replace("  ", " ");
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
            if (svgNode.Attributes?["fill"] != null)
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

            if (!colorFound && svgNode.Attributes?["style"] != null)
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

            var xAsString = rectNode.Attributes["x"].InnerText;
            var yAsString = rectNode.Attributes["y"].InnerText;
            var widthAsString = rectNode.Attributes["width"].InnerText;
            var heightAsString = rectNode.Attributes["height"].InnerText;

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
            var arr = drawCodes.TrimEnd().Replace("  ", " ").Replace("  ", " ").Split();
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
                        drawShape = new DrawShape { Layer = layer, ForeColor = c, IsEraser = isEraser };
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
                        drawShape = new DrawShape { Layer = layer, ForeColor = c, IsEraser = isEraser };
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
