using System.Drawing;

namespace AssaDraw.Logic
{
    public static class CirleBezier
    {
        /// <summary>
        /// draw circle using 4 points + 8 support points - see https://stackoverflow.com/questions/1734745/how-to-create-circle-with-b%C3%A9zier-curves
        /// </summary>
        public static DrawShape MakeCircle(float x, float y, float radius, int layer, Color c)
        {
            const float bezier4Point = 0.552284749831f;

            var shape = new DrawShape { ForeColor = c, Layer = layer };

            var pointTopX = x;
            var pointTopY = y - radius;
            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.Move, pointTopX, pointTopY, DrawSettings.PointColor));

            var pointTopXs1 = x + radius * bezier4Point;
            var pointTopYs1 = y - radius;
            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.BezierCurveSupport1, pointTopXs1, pointTopYs1, DrawSettings.PointHelperColor));

            var pointTopXs2 = x + radius;
            var pointTopYs2 = y - radius * bezier4Point;
            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.BezierCurveSupport1, pointTopXs2, pointTopYs2, DrawSettings.PointHelperColor));

            var pointRightX = x + radius;
            var pointRightY = y;
            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.BezierCurve, pointRightX, pointRightY, DrawSettings.PointColor));

            var pointRightXs1 = x + radius;
            var pointRightYs1 = y + radius * bezier4Point;
            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.BezierCurveSupport1, pointRightXs1, pointRightYs1, DrawSettings.PointHelperColor));

            var pointRightXs2 = x + radius * bezier4Point;
            var pointRightYs2 = y + radius;
            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.BezierCurveSupport2, pointRightXs2, pointRightYs2, DrawSettings.PointHelperColor));

            var pointBottomX = x;
            var pointBottomY = y + radius;
            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.BezierCurve, pointBottomX, pointBottomY, DrawSettings.PointColor));

            var pointBottomXs1 = x - radius * bezier4Point;
            var pointBottomYs1 = y + radius;
            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.BezierCurveSupport1, pointBottomXs1, pointBottomYs1, DrawSettings.PointHelperColor));

            var pointBottomXs2 = x - radius;
            var pointBottomYs2 = y + radius * bezier4Point;
            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.BezierCurveSupport2, pointBottomXs2, pointBottomYs2, DrawSettings.PointHelperColor));

            var pointLeftX = x - radius;
            var pointLeftY = y;
            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.BezierCurve, pointLeftX, pointLeftY, DrawSettings.PointColor));

            var pointLeftXs1 = x - radius;
            var pointLeftYs1 = y - radius * bezier4Point;
            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.BezierCurveSupport1, pointLeftXs1, pointLeftYs1, DrawSettings.PointHelperColor));

            var pointLeftXs2 = x - radius * bezier4Point;
            var pointLeftYs2 = y - radius;
            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.BezierCurveSupport2, pointLeftXs2, pointLeftYs2, DrawSettings.PointHelperColor));

            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.BezierCurve, pointTopX, pointTopY, DrawSettings.PointColor));

            return shape;
        }
    }
}
