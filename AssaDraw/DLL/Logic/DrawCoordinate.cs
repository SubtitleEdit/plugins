using System.Drawing;

namespace AssaDraw.Logic
{
    public class DrawCoordinate
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point Point => new Point(X, Y);

        public Color PointColor { get; set; }

        public DrawCommand DrawCommand { get; set; }

        public DrawCommandType DrawCommandType { get; set; }

        public bool IsBeizer => DrawCommandType == DrawCommandType.BezierCurve || DrawCommandType == DrawCommandType.BezierCurveSupport1 || DrawCommandType == DrawCommandType.BezierCurveSupport2;

        public DrawCoordinate(DrawCommand drawCommand, DrawCommandType drawCommandType)
        {
            DrawCommand = drawCommand;
            DrawCommandType = drawCommandType;
        }

        public DrawCoordinate(DrawCommand drawCommand, DrawCommandType drawCommandType, int x, int y, Color pointColor)
        {
            DrawCommand = drawCommand;
            DrawCommandType = drawCommandType;
            X = x;
            Y = y;
            PointColor = pointColor;
        }

        public string GetText(int x, int y)
        {
            var command = "Command";
            if (DrawCommand.Points[0] == this)
            {
                command = "Move";
            }
            else if (DrawCommandType == DrawCommandType.Line)
            {
                command = "Line";
            }
            else if (DrawCommandType == DrawCommandType.Move)
            {
                command = "Move";
            }
            else if (IsBeizer)
            {
                if (DrawCommandType == DrawCommandType.BezierCurveSupport1)
                {
                    command = "Curve support 1";
                }
                else if (DrawCommandType == DrawCommandType.BezierCurveSupport2)
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
                hash = hash * 23 + DrawCommandType.GetHashCode();
                return hash;
            }
        }
    }
}
