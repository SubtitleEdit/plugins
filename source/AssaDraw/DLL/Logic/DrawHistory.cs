using System.Collections.Generic;

namespace AssaDraw.Logic
{
    public class DrawHistory
    {
        private int _historyIndex; // active version
        private List<DrawHistoryItem> _history;

        public DrawHistory()
        {
            _history = new List<DrawHistoryItem>();
            _historyIndex = -1;
        }

        public void AddChange(DrawHistoryItem item)
        {
            while (_history.Count - 1 > _historyIndex)
            {
                _history.RemoveAt(_history.Count - 1);
            }

            _history.Add(item);
            _historyIndex++;
        }

        public DrawHistoryItem MakeHistoryItem(List<DrawShape> drawShapes, DrawShape activeDrawShape, DrawShape oldDrawShaped, DrawCoordinate activePoint, float zoomFactor)
        {
            var newCmds = new List<DrawShape>();
            foreach (var shape in drawShapes)
            {
                var newCmd = new DrawShape { ForeColor = shape.ForeColor, Layer = shape.Layer };
                foreach (var p in shape.Points)
                {
                    newCmd.AddPoint(p.DrawType, p.X, p.Y, p.PointColor);
                }
                newCmds.Add(newCmd);
            }

            return new DrawHistoryItem()
            {
                DrawShapes = newCmds,
                ActiveDrawShape = activeDrawShape,
                OldDrawShape = oldDrawShaped,
                ActivePoint = activePoint,
                ZoomFactor = zoomFactor,
            };
        }

        public DrawHistoryItem Undo()
        {
            if (_historyIndex < 1)
            {
                return null;
            }

            _historyIndex--;
            return _history[_historyIndex];
        }

        public DrawHistoryItem Redo()
        {
            if (_historyIndex > _history.Count - 2)
            {
                return null;
            }

            _historyIndex++;
            return _history[_historyIndex];
        }
    }
}
