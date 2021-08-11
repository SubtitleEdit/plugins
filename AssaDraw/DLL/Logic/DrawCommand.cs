using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AssaDraw.Logic
{
    public class DrawCommand
    {
        public List<DrawCoordinate> Points { get; set; }

        public DrawCommand()
        {
            Points = new List<DrawCoordinate>();
        }

        public DrawCommand(DrawCommand drawCommand)
        {
            Points = new List<DrawCoordinate>();
            foreach (var point in drawCommand.Points)
            {
                AddPoint(point.DrawCommandType, point.X, point.Y, point.PointColor);
            }
        }

        public DrawCoordinate AddPoint(DrawCommandType drawCommandType, int x, int y, Color pointColor)
        {
            var coordinate = new DrawCoordinate(this, drawCommandType, x, y, pointColor);
            Points.Add(coordinate);
            return coordinate;
        }
        public DrawCoordinate AddPoint(DrawCommandType drawCommandType, float x, float y, Color pointColor)
        {
            var coordinate = new DrawCoordinate(this, drawCommandType, x, y, pointColor);
            Points.Add(coordinate);
            return coordinate;
        }

        public string ToAssa()
        {
            if (Points == null || Points.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var state = DrawCommandType.None;
            for (int i = 0; i < Points.Count; i++)
            {
                var point = Points[i];
                if (i == 0)
                {
                    sb.Append($"m {(int)Math.Round(point.X)} {(int)Math.Round(point.Y)} ");
                }
                else
                {
                    if (state != point.DrawCommandType)
                    {
                        if (point.DrawCommandType == DrawCommandType.Line)
                        {
                            sb.Append("l ");
                        }
                        else if (point.DrawCommandType == DrawCommandType.BezierCurve)
                        {
                            sb.Append("b ");
                        }

                        state = point.DrawCommandType;
                    }

                    sb.Append($"{(int)Math.Round(point.X)} {(int)Math.Round(point.Y)} ");
                }
            }

            return sb.ToString().Trim();
        }

        public int GetFastHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;

                foreach (var point in Points)
                {
                    hash = hash * 23 + point.GetFastHashCode();
                }

                return hash;
            }
        }
    }
}
