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
