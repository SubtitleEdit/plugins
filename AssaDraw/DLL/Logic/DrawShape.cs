using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AssaDraw.Logic
{
    public class DrawShape
    {
        public List<DrawCoordinate> Points { get; set; }
        public Color ForeColor { get; set; }
        public int Layer { get; set; }
        public bool IsEraser { get; set; }
        public bool Hidden { get; set; }
        public bool Expanded { get; set; }
        public Color OutlineColor { get; set; }
        public int OutlineWidth { get; set; }

        public DrawShape()
        {
            ForeColor = Color.White;
            Points = new List<DrawCoordinate>();
        }

        public DrawShape(DrawShape drawShape)
        {
            ForeColor = drawShape.ForeColor;
            Points = new List<DrawCoordinate>();
            foreach (var point in drawShape.Points)
            {
                AddPoint(point.DrawType, point.X, point.Y, point.PointColor);
            }
        }

        public DrawCoordinate AddPoint(DrawCoordinateType drawType, int x, int y, Color pointColor)
        {
            var coordinate = new DrawCoordinate(this, drawType, x, y, pointColor);
            Points.Add(coordinate);
            return coordinate;
        }
        public DrawCoordinate AddPoint(DrawCoordinateType drawType, float x, float y, Color pointColor)
        {
            var coordinate = new DrawCoordinate(this, drawType, x, y, pointColor);
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
            var state = DrawCoordinateType.None;
            for (var i = 0; i < Points.Count; i++)
            {
                var point = Points[i];
                if (i == 0)
                {
                    sb.Append($"m {(int)Math.Round(point.X)} {(int)Math.Round(point.Y)} ");
                }
                else
                {
                    var newState = point.IsBeizer ? DrawCoordinateType.BezierCurve : point.DrawType;
                    if (state != newState)
                    {
                        if (point.DrawType == DrawCoordinateType.Line)
                        {
                            sb.Append("l ");
                        }
                        else if (point.IsBeizer)
                        {
                            sb.Append("b ");
                        }

                        state = newState;
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
