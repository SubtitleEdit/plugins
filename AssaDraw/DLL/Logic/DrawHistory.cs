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

        public DrawHistoryItem MakeHistoryItem(List<DrawCommand> drawCommands, DrawCommand activeDrawCommand, DrawCommand oldDrawCommand, DrawCoordinate activePoint, float zoomFactor)
        {
            var newCmds = new List<DrawCommand>();
            foreach (var cmd in drawCommands)
            {
                var newCmd = new DrawCommand();
                foreach (var p in cmd.Points)
                {
                    newCmd.AddPoint(p.DrawCommandType, p.X, p.Y, p.PointColor);
                }
                newCmds.Add(newCmd);
            }

            return new DrawHistoryItem()
            {
                DrawCommands = newCmds,
                ActiveDrawCommand = activeDrawCommand,
                OldDrawCommand = oldDrawCommand,
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
