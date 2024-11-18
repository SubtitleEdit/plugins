using System;
using System.Drawing;

namespace AssaDraw.Logic
{
    public class DrawCoordinate
    {
        public float X { get; set; }
        public float Y { get; set; }

        public PointF Point => new PointF(X, Y);

        public Color PointColor { get; set; }

        public DrawShape DrawShape { get; set; }

        public DrawCoordinateType DrawType { get; set; }

        public bool IsBeizer => DrawType == DrawCoordinateType.BezierCurve || DrawType == DrawCoordinateType.BezierCurveSupport1 || DrawType == DrawCoordinateType.BezierCurveSupport2;

        public DrawCoordinate(DrawShape drawShape, DrawCoordinateType drawType)
        {
            DrawShape = drawShape;
            DrawType = drawType;
        }

        public DrawCoordinate(DrawShape drawShape, DrawCoordinateType drawType, float x, float y, Color pointColor)
        {
            DrawShape = drawShape;
            DrawType = drawType;
            X = x;
            Y = y;
            PointColor = pointColor;
        }

        public DrawCoordinate(DrawShape drawShape, DrawCoordinateType drawType, int x, int y, Color pointColor)
        {
            DrawShape = drawShape;
            DrawType = drawType;
            X = x;
            Y = y;
            PointColor = pointColor;
        }

        public string GetText(float x, float y)
        {
            return GetText((int)Math.Round(x), (int)Math.Round(y));
        }


        public string GetText(int x, int y)
        {
            var command = "Command";
            if (DrawShape.Points[0] == this)
            {
                command = "Move";
            }
            else if (DrawType == DrawCoordinateType.Line)
            {
                command = "Line";
            }
            else if (DrawType == DrawCoordinateType.Move)
            {
                command = "Move";
            }
            else if (IsBeizer)
            {
                if (DrawType == DrawCoordinateType.BezierCurveSupport1)
                {
                    command = "Curve support 1";
                }
                else if (DrawType == DrawCoordinateType.BezierCurveSupport2)
                {
                    command = "Curve support 2";
                }
                else
                {
                    command = "Curve";
                }
            }

            return $"{command} to {x},{y}";
        }

        internal int GetFastHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                hash = hash * 23 + DrawType.GetHashCode();
                return hash;
            }
        }
    }
}
