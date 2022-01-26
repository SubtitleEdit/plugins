using AssaDraw.ColorPicker;
using AssaDraw.Logic;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.CSharp;

namespace AssaDraw
{
    public partial class FormAssaDrawMain : Form
    {
        public Subtitle AssaDrawCodes { get; set; }

        private readonly List<DrawShape> _drawShapes;
        private float _zoomFactor = 1.0f;
        private DrawShape _activeDrawShape;
        private DrawShape _oldDrawShape;
        private DrawCoordinate _activePoint;
        private Point _moveActiveDrawShapeStart = new Point(int.MinValue, int.MinValue);
        private int _x;
        private int _y;
        private float _panX;
        private float _panY;
        private DrawCoordinate _mouseDownPoint;
        private bool _panOn;
        private long _drawingStartTicks;
        private long _drawingEndTicks;

        private readonly object _treeViewLock = new object();
        private bool _treeViewInvalidated;

        private readonly DrawHistory _history;
        private readonly object _historyLock = new object();
        private int _historyHash;

        private string _fileName;

        private Bitmap _backgroundImage;
        private bool _backgroundOff;

        private readonly Regex _regexStart = new Regex(@"\{[^{]*\\p1[^}]*}");
        private readonly Regex _regexEnd = new Regex(@"\{[^{]*\\p0[^}]*}");
        private string _assaStartTag = "{\\p1}";
        private string _assaEndTag = "{\\p0}";

        private LibMpvDynamic _mpv;
        private string _mpvTextFileName;

        private const string StyleName = "AssaDraw";

        public FormAssaDrawMain(string text, string videoFileName, string videoPosition, string fileName)
        {
            InitializeComponent();
            DrawSettings.LoadSettings();
            _x = int.MinValue;
            _y = int.MinValue;
            _drawShapes = new List<DrawShape>();
            numericUpDownX.Enabled = false;
            numericUpDownY.Enabled = false;
            EnableDisableCurrentShapeActions();
            _drawingEndTicks = DateTime.UtcNow.Ticks;

            var left = Math.Max(labelResolution.Left + labelResolution.Width, labelActivePoint.Left + labelActivePoint.Width);
            numericUpDownWidth.Left = left + 3;
            numericUpDownHeight.Left = numericUpDownWidth.Left + numericUpDownWidth.Width + 3;
            numericUpDownX.Left = left + 3;
            numericUpDownY.Left = numericUpDownX.Left + numericUpDownX.Width + 3;

            _history = new DrawHistory();

            var historyTimer = new Timer { Interval = 500 };
            historyTimer.Tick += _historyTimer_Tick;
            historyTimer.Start();

            if (text == "standalone")
            {
                DrawSettings.Standalone = true;
                buttonCancel.Visible = false;
                buttonOk.Visible = false;
                if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                {
                    ImportAssaDrawing(fileName);
                }
            }
            else if (!string.IsNullOrEmpty(text))
            {
                var sub = new Subtitle();
                new AdvancedSubStationAlpha().LoadSubtitle(sub, text.SplitToLines(), string.Empty);
                ImportAssaDrawingFromFileText(text);
            }

            if (DrawSettings.HideSettingsAndTreeView)
            {
                HideSettingsAndTreeView();
            }

            toolStripButtonPreview.Enabled = LibMpvDynamic.IsInstalled;
            ShowTitle();
            MouseWheel += FormAssaDrawMain_MouseWheel;

            if (DrawSettings.UseScreenShotFromSe && !string.IsNullOrEmpty(videoFileName) && !string.IsNullOrEmpty(videoPosition) && File.Exists(videoFileName))
            {
                GetVideoBackground(videoFileName, videoPosition);
            }

            textBoxColorPicker.Visible = false;
            panelColorPicker.Visible = false;
            panelColorPickerChoice.Visible = false;
            buttonSetColor.Visible = false;
            buttonSetOutlineColor.Visible = false;

            timerTreeViewUpdate.Start();
        }

        private void GetVideoBackground(string videoFileName, string videoPosition)
        {
            var bw = new BackgroundWorker();
            bw.RunWorkerCompleted += (o, args) =>
            {
                if (args.Result is string fileName)
                {
                    SetBackgroundImage(fileName);
                }
            };
            bw.DoWork += (o, args) =>
            {
                try
                {
                    args.Result = VideoPreviewGenerator.GetScreenShot(videoFileName, videoPosition);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"An error occurred while trying to get screen shot from {videoFileName} at pos {videoPosition}: {e}");
                }
            };
            bw.RunWorkerAsync();
        }

        private void SetAssaStartEndTags(string text)
        {
            var startMatch = _regexStart.Match(text);
            var endMatch = _regexEnd.Match(text);
            if (startMatch.Success && startMatch.Value.Length > 3 &&
                endMatch.Success && startMatch.Value.Length > 3)
            {
                _assaStartTag = startMatch.Value;
                _assaEndTag = endMatch.Value;
            }
            else
            {
                _assaStartTag = "{\\p1}";
                _assaEndTag = "{\\p0}";
            }
        }

        private void FormAssaDrawMain_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
                _zoomFactor += e.Delta / 1000.0f;
                ZoomChangedPostFix();
            }
            else if (ModifierKeys == (Keys.Control | Keys.Shift))
            {
                ScaleActiveShapes(e.Delta > 0 ? 1.1f : 0.9f);
                ZoomChangedPostFix();
            }
            else if (ModifierKeys == Keys.Alt)
            {
                ScaleAll(e.Delta > 0 ? 1.1f : 0.9f);
                ZoomChangedPostFix();
            }
        }

        private void ZoomChangedPostFix()
        {
            if (_zoomFactor < 0.1f)
            {
                _zoomFactor = 0.1f;
            }

            if (Math.Abs(_zoomFactor - 1.0f) < 0.1)
            {
                _zoomFactor = 1.0f;
            }

            ShowTitle();
            pictureBoxCanvas.Invalidate();
        }

        private void ShowTitle()
        {
            var version = (new Nikse.SubtitleEdit.PluginLogic.AssaDraw() as Nikse.SubtitleEdit.PluginLogic.IPlugin).Version.ToString(CultureInfo.InvariantCulture);

            if (toolStripButtonPreview.Checked)
            {
                Text = $"ASSA Draw {version} - Preview mode";
            }
            else if (Math.Abs(_zoomFactor - 1) < 0.001)
            {
                Text = $"ASSA Draw {version}";
            }
            else
            {
                Text = $"ASSA Draw {version} - Zoom is {(_zoomFactor * 100.0):##0.#}%";
            }
        }

        private void EnableDisableCurrentShapeActions()
        {
            var preview = toolStripButtonPreview.Checked;
            toolStripButtonClearCurrent.Enabled = _activeDrawShape != null && !preview;
            toolStripButtonCloseShape.Enabled = _activeDrawShape != null && _activeDrawShape.Points.Count > 2 && !preview;
            toolStripButtonMirrorHor.Enabled = _drawShapes.Count > 0 && _activeDrawShape != null && _x == int.MinValue && _y == int.MinValue && !preview;
            toolStripButtonMirrorVert.Enabled = _drawShapes.Count > 0 && _activeDrawShape != null && _x == int.MinValue && _y == int.MinValue && !preview;
        }

        private int ToZoomFactor(int v)
        {
            return (int)Math.Round(v * _zoomFactor);
        }

        private int ToZoomFactorX(float v)
        {
            return (int)Math.Round(v * _zoomFactor + _panX);
        }

        private int ToZoomFactorY(float v)
        {
            return (int)Math.Round(v * _zoomFactor + _panY);
        }

        private Point ToZoomFactorPoint(DrawCoordinate drawCoordinate)
        {
            return new Point(ToZoomFactorX(drawCoordinate.X), ToZoomFactorY(drawCoordinate.Y));
        }

        private int ToZoomFactor(decimal v)
        {
            return (int)Math.Round((float)v * _zoomFactor);
        }

        private int FromZoomFactor(int v)
        {
            return (int)Math.Round(v / _zoomFactor);
        }

        private int FromZoomFactor(float v)
        {
            return (int)Math.Round(v / _zoomFactor);
        }

        private void pictureBoxCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (pictureBoxCanvas.Width < 1 || pictureBoxCanvas.Height < 1 || toolStripButtonPreview.Checked)
            {
                return;
            }

            // draw background
            var bitmap = _backgroundImage != null && !_backgroundOff ? (Bitmap)_backgroundImage.Clone() : new Bitmap(ToZoomFactor((int)numericUpDownWidth.Value), ToZoomFactor((int)numericUpDownHeight.Value));
            var graphics = e.Graphics;
            DrawOffScreenBackground(graphics);

            if (_backgroundImage == null)
            {
                using (var brush = new SolidBrush(DrawSettings.BackgroundColor))
                {
                    graphics.FillRectangle(brush, new Rectangle((int)Math.Round(_panX), (int)Math.Round(_panY), bitmap.Width, bitmap.Height));
                }
            }
            else
            {
                graphics.DrawImage(bitmap, _panX, _panY, ToZoomFactor(bitmap.Width), ToZoomFactor(bitmap.Height));
            }

            DrawResolution(graphics);

            // draw shapes
            var activeLayer = _activeDrawShape == null && treeView1.Visible && treeView1.SelectedNode?.Tag is int layer ? layer : int.MinValue;
            foreach (var drawShape in _drawShapes.Where(p => !p.Hidden))
            {
                if (drawShape != _activeDrawShape)
                {
                    Draw(drawShape, graphics, drawShape.Layer == activeLayer);
                }

                foreach (var point in drawShape.Points)
                {
                    using (var pen3 = new Pen(new SolidBrush(point.PointColor), 2))
                    {
                        graphics.DrawLine(pen3, new Point(ToZoomFactorX(point.X) - 5, ToZoomFactorY(point.Y)), new Point(ToZoomFactorX(point.X) + 5, ToZoomFactorY(point.Y)));
                        graphics.DrawLine(pen3, new Point(ToZoomFactorX(point.X), ToZoomFactorY(point.Y) - 5), new Point(ToZoomFactorX(point.X), ToZoomFactorY(point.Y) + 5));
                    }
                }
            }

            Draw(_activeDrawShape, graphics, true);

            if (_activePoint != null)
            {
                using (var pen = new Pen(new SolidBrush(Color.FromArgb(255, _activePoint.PointColor)), 3))
                {
                    graphics.DrawLine(pen, new Point(ToZoomFactorX(_activePoint.X) - 7, ToZoomFactorY(_activePoint.Y)), new Point(ToZoomFactorX(_activePoint.X) + 7, ToZoomFactorY(_activePoint.Y)));
                    graphics.DrawLine(pen, new Point(ToZoomFactorX(_activePoint.X), ToZoomFactorY(_activePoint.Y) - 7), new Point(ToZoomFactorX(_activePoint.X), ToZoomFactorY(_activePoint.Y) + 7));
                }
            }

            bitmap.Dispose();
        }

        private void DrawOffScreenBackground(Graphics g)
        {
            int rectangleSize = 8;
            var dark = false;
            for (int y = 0; y < pictureBoxCanvas.Height; y += rectangleSize)
            {
                for (int x = 0; x < pictureBoxCanvas.Width; x += rectangleSize)
                {
                    var c = dark ? Color.Black : Color.WhiteSmoke;
                    if (y % (rectangleSize * 2) == 0)
                    {
                        if (x % (rectangleSize * 2) == 0)
                        {
                            c = dark ? Color.DimGray : Color.LightGray;
                        }
                    }
                    else
                    {
                        if (x % (rectangleSize * 2) != 0)
                        {
                            c = dark ? Color.DimGray : Color.LightGray;
                        }
                    }

                    using (var brush = new SolidBrush(c))
                    {
                        g.FillRectangle(brush, x, y, rectangleSize, rectangleSize);
                    }
                }
            }
        }

        private void DrawResolution(Graphics graphics)
        {
            using (var pen = new Pen(new SolidBrush(DrawSettings.ScreenSizeColor), 2))
            {
                graphics.DrawRectangle(pen, _panX - 1, _panY - 1, ToZoomFactor(numericUpDownWidth.Value + 2), ToZoomFactor(numericUpDownHeight.Value + 2));
            }
        }

        private void Draw(DrawShape drawShape, Graphics graphics, bool isActive)
        {
            if (drawShape == null || drawShape.Points.Count == 0 || drawShape.Hidden)
            {
                return;
            }

            var isCircle = false;
            if (isActive && toolStripButtonCircle.Checked && !_drawShapes.Contains(drawShape) && _x > int.MinValue && _y > int.MinValue)
            {
                var start = drawShape.Points.FirstOrDefault();
                if (start == null)
                {
                    return;
                }

                var xDiff = _x > start.X ? _x - start.X : start.X - _x;
                var yDiff = _y > start.Y ? _y - start.Y : start.Y - _y;
                var maxDiff = Math.Max(xDiff, yDiff);
                if (maxDiff < 1)
                {
                    return;
                }

                drawShape = CirleBezier.MakeCircle(start.X, start.Y, maxDiff, drawShape.Layer, drawShape.ForeColor);
                isCircle = true;
            }

            var isRectangle = false;
            if (isActive && toolStripButtonRectangle.Checked && !_drawShapes.Contains(drawShape) && _x > int.MinValue && _y > int.MinValue)
            {
                var start = drawShape.Points.FirstOrDefault();
                if (start == null)
                {
                    return;
                }

                var xDiff = _x - start.X;
                var yDiff = _y - start.Y;
                drawShape = MakeRectangle(start.X, start.Y, xDiff, yDiff, drawShape.Layer, drawShape.ForeColor);
                isRectangle = true;
            }

            var color = isActive ? DrawSettings.ActiveShapeLineColor : DrawSettings.ShapeLineColor;
            using (var pen = new Pen(new SolidBrush(color), 2))
            {
                int i = 0;
                while (i < drawShape.Points.Count)
                {
                    if (drawShape.Points[i].DrawType == DrawCoordinateType.Line)
                    {
                        if (i > 0 && i < drawShape.Points.Count - 1 && drawShape.Points[i].DrawType == DrawCoordinateType.Line && drawShape.Points[i - 1].IsBeizer)
                        {
                            graphics.DrawLine(pen, ToZoomFactorPoint(drawShape.Points[i - 1]), ToZoomFactorPoint(drawShape.Points[i]));

                        }
                        else if (i < drawShape.Points.Count - 1 && drawShape.Points[i].DrawType == DrawCoordinateType.Line && drawShape.Points[i + 1].DrawType == DrawCoordinateType.Line)
                        {
                            graphics.DrawLine(pen, ToZoomFactorPoint(drawShape.Points[i]), ToZoomFactorPoint(drawShape.Points[i + 1]));
                        }
                        else if (i < drawShape.Points.Count - 1 && i > 0)
                        {
                            graphics.DrawLine(pen, ToZoomFactorPoint(drawShape.Points[i - 1]), ToZoomFactorPoint(drawShape.Points[i]));
                        }

                        if (isActive && drawShape.Points.Count > 0 && (_x != int.MinValue || _y != int.MinValue) &&
                            !_drawShapes.Contains(_activeDrawShape) && !isCircle && !isRectangle)
                        {
                            using (var penNewLine = new Pen(new SolidBrush(DrawSettings.ActiveShapeLineColor), 2))
                            {
                                graphics.DrawLine(penNewLine, ToZoomFactorPoint(drawShape.Points[drawShape.Points.Count - 1]), new Point(ToZoomFactorX(_x), ToZoomFactorY(_y)));
                            }
                        }

                        i++;
                        if (i >= drawShape.Points.Count - 1 && !isCircle && !isRectangle)
                        {
                            var c = isActive ? DrawSettings.ActiveShapeLineColor : DrawSettings.ShapeLineColor;
                            if (drawShape == _activeDrawShape && !_drawShapes.Contains(drawShape))
                            {
                                c = DrawSettings.ShapeLineColor;
                            }

                            using (var penClosing = new Pen(new SolidBrush(c), 2))
                            {
                                graphics.DrawLine(penClosing, ToZoomFactorPoint(drawShape.Points[drawShape.Points.Count - 1]), ToZoomFactorPoint(drawShape.Points[0]));
                            }
                        }
                    }
                    else if (drawShape.Points[i].IsBeizer)
                    {

                        if (drawShape.Points.Count - i >= 3 && i > 0)
                        {
                            graphics.DrawBezier(pen, ToZoomFactorPoint(drawShape.Points[i - 1]), ToZoomFactorPoint(drawShape.Points[i]), ToZoomFactorPoint(drawShape.Points[i + 1]), ToZoomFactorPoint(drawShape.Points[i + 2]));
                            i += 2;
                        }
                        else if (drawShape.Points.Count - i >= 3 && i == 0)
                        {
                            graphics.DrawBezier(pen, ToZoomFactorPoint(drawShape.Points[i]), ToZoomFactorPoint(drawShape.Points[i + 1]), ToZoomFactorPoint(drawShape.Points[i + 2]), ToZoomFactorPoint(drawShape.Points[i + 3]));
                            i += 3;
                        }

                        if (isActive && drawShape.Points.Count > 0 && (_x != int.MinValue || _y != int.MinValue) && !_drawShapes.Contains(_activeDrawShape) && !isCircle)
                        {
                            using (var penNewLine = new Pen(new SolidBrush(DrawSettings.ActiveShapeLineColor), 2))
                            {
                                graphics.DrawLine(penNewLine, ToZoomFactorPoint(drawShape.Points[drawShape.Points.Count - 1]), new Point(ToZoomFactorX(_x), ToZoomFactorY(_y)));
                            }
                        }

                        i++;
                        if (i >= drawShape.Points.Count - 1 && !isCircle && !isRectangle)
                        {
                            var c = isActive ? DrawSettings.ActiveShapeLineColor : DrawSettings.ShapeLineColor;
                            if (drawShape == _activeDrawShape && !_drawShapes.Contains(drawShape))
                            {
                                c = DrawSettings.ShapeLineColor;
                            }

                            using (var penClosing = new Pen(new SolidBrush(c), 2))
                            {
                                graphics.DrawLine(penClosing, ToZoomFactorPoint(drawShape.Points[drawShape.Points.Count - 1]), ToZoomFactorPoint(drawShape.Points[0]));
                            }
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        private static DrawShape MakeRectangle(float x, float y, float width, float height, int layer, Color c)
        {
            var shape = new DrawShape { ForeColor = c, Layer = layer };

            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.Line, x, y, DrawSettings.PointColor));

            var pointX2 = x + width;
            var pointY2 = y;
            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.Line, pointX2, pointY2, DrawSettings.PointColor));

            var pointX3 = x + width;
            var pointY3 = y + height;
            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.Line, pointX3, pointY3, DrawSettings.PointColor));

            var pointX4 = x;
            var pointY4 = y + height;
            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.Line, pointX4, pointY4, DrawSettings.PointColor));

            var pointX5 = x;
            var pointY5 = y;
            shape.Points.Add(new DrawCoordinate(shape, DrawCoordinateType.Line, pointX5, pointY5, DrawSettings.PointColor));

            return shape;
        }

        private void pictureBoxCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (_mouseDownPoint != null || _moveActiveDrawShapeStart.X != int.MinValue || toolStripButtonPreview.Checked)
            {
                return;
            }

            if (toolStripButtonColorPicker.Checked)
            {
                Configuration.LastColorPickerColor = panelColorPicker.BackColor;
                panelColorPickerChoice.BackColor = panelColorPicker.BackColor;
                return;
            }

            var x = FromZoomFactor((int)Math.Round(e.Location.X - _panX));
            var y = FromZoomFactor((int)Math.Round(e.Location.Y - _panY));

            _activePoint = null;
            numericUpDownX.Enabled = false;
            numericUpDownY.Enabled = false;

            if (e.Button == MouseButtons.Left && _activeDrawShape != null && _activeDrawShape.Points.Count > 0 && !_drawShapes.Contains(_activeDrawShape))
            {
                // continue drawing
                if (toolStripButtonLine.Checked)
                {
                    _activeDrawShape.AddPoint(DrawCoordinateType.Line, x, y, DrawSettings.PointColor);
                }
                else if (toolStripButtonBeizer.Checked)
                {
                    // add two support points
                    var startX = _activeDrawShape.Points[_activeDrawShape.Points.Count - 1].X;
                    var startY = _activeDrawShape.Points[_activeDrawShape.Points.Count - 1].Y;
                    var endX = x;
                    var endY = y;
                    var oneThirdX = (int)Math.Round((endX - startX) / 3.0);
                    var oneThirdY = (int)Math.Round((endY - startY) / 3.0);
                    _activeDrawShape.AddPoint(DrawCoordinateType.BezierCurveSupport1, startX + oneThirdX, startY + oneThirdY, DrawSettings.PointHelperColor);
                    _activeDrawShape.AddPoint(DrawCoordinateType.BezierCurveSupport2, startX + oneThirdX + oneThirdX, startY + oneThirdY + oneThirdY, DrawSettings.PointHelperColor);

                    // add end point
                    _activeDrawShape.AddPoint(DrawCoordinateType.BezierCurve, endX, endY, DrawSettings.PointColor);
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                _activePoint = null;
                numericUpDownX.Enabled = false;
                numericUpDownY.Enabled = false;

                _oldDrawShape = null;
                if (toolStripButtonLine.Checked)
                {
                    _activeDrawShape = new DrawShape();
                    _activeDrawShape.AddPoint(DrawCoordinateType.Line, x, y, DrawSettings.PointColor);
                }
                else if (toolStripButtonBeizer.Checked)
                {
                    _activeDrawShape = new DrawShape();
                    _activeDrawShape.AddPoint(DrawCoordinateType.BezierCurve, x, y, DrawSettings.PointColor);
                }
                else if (toolStripButtonCircle.Checked)
                {
                    if (DateTime.UtcNow.Ticks - _drawingEndTicks > 10_000 * 250)
                    {
                        _activeDrawShape = new DrawShape();
                        _activeDrawShape.AddPoint(DrawCoordinateType.Line, x, y, DrawSettings.PointColor);
                        _drawingStartTicks = DateTime.UtcNow.Ticks;
                    }
                }
                else if (toolStripButtonRectangle.Checked)
                {
                    if (DateTime.UtcNow.Ticks - _drawingEndTicks > 10_000 * 250)
                    {
                        _activeDrawShape = new DrawShape();
                        _activeDrawShape.AddPoint(DrawCoordinateType.Line, x, y, DrawSettings.PointColor);
                        _drawingStartTicks = DateTime.UtcNow.Ticks;
                    }
                }
            }

            pictureBoxCanvas.Invalidate();
            EnableDisableCurrentShapeActions();
        }

        private DrawCoordinate GetClosePoint(int x, int y)
        {
            var maxDiff = float.MaxValue;
            DrawCoordinate pointDiff = null;

            foreach (var drawShape in _drawShapes.Where(p => !p.Hidden))
            {
                foreach (var point in drawShape.Points)
                {
                    var diff = Math.Abs(x - point.X) + Math.Abs(y - point.Y);
                    if (diff <= maxDiff)
                    {
                        maxDiff = diff;
                        pointDiff = point;
                    }
                }
            }

            if (maxDiff > 10)
            {
                return null;
            }

            return pointDiff;
        }

        private void pictureBoxCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (toolStripButtonPreview.Checked)
            {
                return;
            }

            var x = FromZoomFactor((int)Math.Round(e.Location.X - _panX));
            var y = FromZoomFactor((int)Math.Round(e.Location.Y - _panY));
            labelPosition.Text = $"Position {x},{y}";

            // pan
            if (_panOn)
            {
                Cursor = Cursors.SizeAll;
                _panX += e.Location.X - _mouseDownPoint.X;
                _panY += e.Location.Y - _mouseDownPoint.Y;
                _mouseDownPoint.X = e.Location.X;
                _mouseDownPoint.Y = e.Location.Y;
                pictureBoxCanvas.Invalidate();
                _activePoint = null;
                return;
            }

            if (toolStripButtonColorPicker.Checked)
            {
                Cursor = Cursors.Cross;
                if (_backgroundImage != null)
                {
                    if (x >= 0 && x < _backgroundImage.Width &&
                        y >= 0 && y < _backgroundImage.Height)
                    {
                        var c = _backgroundImage.GetPixel(x, y);
                        panelColorPicker.BackColor = _backgroundImage.GetPixel(x, y);
                        textBoxColorPicker.Text = $"{c.A:x2}{c.R:x2}{c.G:x2}{c.B:x2}";
                    }
                }

                return;
            }

            if (_mouseDownPoint != null)
            {
                _mouseDownPoint.X = x;
                _mouseDownPoint.Y = y;

                numericUpDownX.ValueChanged -= numericUpDownX_ValueChanged;
                numericUpDownX.Value = x;
                numericUpDownX.ValueChanged += numericUpDownX_ValueChanged;

                numericUpDownY.ValueChanged -= numericUpDownY_ValueChanged;
                numericUpDownY.Value = y;
                numericUpDownY.ValueChanged += numericUpDownY_ValueChanged;

                foreach (TreeNode node in treeView1.Nodes)
                {
                    foreach (TreeNode subNode in node.Nodes)
                    {
                        foreach (TreeNode subSubNode in subNode.Nodes)
                        {
                            if (subSubNode.Tag == _mouseDownPoint)
                            {
                                subSubNode.Text = _mouseDownPoint.GetText(x, y);
                                break;
                            }
                        }
                    }
                }

                pictureBoxCanvas.Invalidate();
                return;
            }

            if (_drawShapes.Contains(_activeDrawShape) || _activeDrawShape == null)
            {
                var closePoint = GetClosePoint(x, y);
                if (closePoint != null)
                {
                    Cursor = Cursors.Hand;
                    return;
                }
            }

            // Move whole active layer
            var activeLayer = _activeDrawShape == null && treeView1.Visible && treeView1.SelectedNode?.Tag is int layer ? layer : int.MinValue;
            if (_activeDrawShape == null && activeLayer > int.MinValue && ModifierKeys == Keys.Control &&
                _moveActiveDrawShapeStart.X != int.MinValue && _moveActiveDrawShapeStart.Y != int.MinValue)
            {
                Cursor = Cursors.SizeAll;
                var xAdjust = x - _moveActiveDrawShapeStart.X;
                var yAdjust = y - _moveActiveDrawShapeStart.Y;
                _moveActiveDrawShapeStart.X = x;
                _moveActiveDrawShapeStart.Y = y;
                var layerShapes = _drawShapes.Where(p => p.Layer == activeLayer).ToList();
                foreach (var drawShape in layerShapes)
                {
                    foreach (var p in drawShape.Points)
                    {
                        p.X += xAdjust;
                        p.Y += yAdjust;
                    }
                }
                pictureBoxCanvas.Invalidate();
                _treeViewInvalidated = true;

                _activePoint = null;
                return;
            }

            // Move ALL
            if (ModifierKeys == (Keys.Control | Keys.Shift) && _moveActiveDrawShapeStart.X != int.MinValue && _moveActiveDrawShapeStart.Y != int.MinValue)
            {
                Cursor = Cursors.SizeAll;
                var xAdjust = x - _moveActiveDrawShapeStart.X;
                var yAdjust = y - _moveActiveDrawShapeStart.Y;
                _moveActiveDrawShapeStart.X = x;
                _moveActiveDrawShapeStart.Y = y;
                foreach (var drawShape in _drawShapes)
                {
                    foreach (var p in drawShape.Points)
                    {
                        p.X += xAdjust;
                        p.Y += yAdjust;
                    }
                }
                pictureBoxCanvas.Invalidate();
                _treeViewInvalidated = true;
                _activePoint = null;
                return;
            }


            if (_activeDrawShape == null && _activePoint == null)
            {
                Cursor = Cursors.Default;
                return;
            }

            if (_activeDrawShape != null)
            {
                if (_activeDrawShape.Points.Count == 0 && _drawShapes.Contains(_activeDrawShape))
                {
                    _activeDrawShape.AddPoint(DrawCoordinateType.Line, x, y, DrawSettings.PointColor);
                }
                else
                {
                    _x = x;
                    _y = y;
                }

                pictureBoxCanvas.Invalidate();
            }

            // move active shape
            if (ModifierKeys == Keys.Control && _activeDrawShape != null && _drawShapes.Contains(_activeDrawShape) &&
                _moveActiveDrawShapeStart.X != int.MinValue && _moveActiveDrawShapeStart.Y != int.MinValue)
            {
                Cursor = Cursors.SizeAll;
                var xAdjust = x - _moveActiveDrawShapeStart.X;
                var yAdjust = y - _moveActiveDrawShapeStart.Y;
                _moveActiveDrawShapeStart.X = x;
                _moveActiveDrawShapeStart.Y = y;
                foreach (var p in _activeDrawShape.Points)
                {
                    p.X += xAdjust;
                    p.Y += yAdjust;
                }

                //TreeViewUpdate(_activeDrawShape);
                _treeViewInvalidated = true;

                pictureBoxCanvas.Invalidate();
                _activePoint = null;
                return;
            }

            Cursor = Cursors.Default;
        }

        private string WrapInPTag(string s, Color color, int outlineWidth, Color outlineColor)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            var regexAlpha = new Regex(@"\\1a&H[0123456789ABCDEFabcdef]{1,2}&");
            var regexColor = new Regex(@"\\1?c&H[0123456789ABCDEFabcdef]{1,8}&");
            var startTag = regexAlpha.Replace(_assaStartTag, string.Empty);
            startTag = regexColor.Replace(startTag, string.Empty);
            var idx = startTag.IndexOf("\\", StringComparison.Ordinal);
            if (idx > 0 && color != Color.Transparent)
            {
                startTag = startTag.Insert(idx, AdvancedSubStationAlpha.GetSsaColorStringForEvent(color).TrimEnd('&') + "&");
            }

            if (idx > 0 && outlineWidth > 0)
            {
                startTag = startTag.Insert(idx, $"\\bord{outlineWidth}");
                var temp = AdvancedSubStationAlpha.GetSsaColorStringForEvent(outlineColor).TrimEnd('&') + "&";
                temp = temp.Replace("\\c&", "\\3c&");
                temp = temp.Replace("\\1a&", "\\3a&");
                startTag = startTag.Insert(idx, temp);
            }

            return $"{startTag}{s.Trim()}{_assaEndTag}";
        }

        private static string WrapInIClipTag(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            return $"{{\\iclip({s.Trim()})}}";
        }

        private void TreeViewFill(List<DrawShape> drawShapes)
        {
            var selectedNodeTag = treeView1.SelectedNode?.Tag;
            var selectedShape = selectedNodeTag as DrawShape;
            var selectedLayer = selectedNodeTag as int?;
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();

            foreach (var layer in drawShapes.OrderBy(p => p.Layer).GroupBy(p => p.Layer))
            {
                var layerNode = new TreeNode($"Layer {layer.Key}" + (layer.First().Hidden ? " (hidden)" : string.Empty)) { Tag = layer.Key };
                treeView1.Nodes.Add(layerNode);
                foreach (var drawShape in layer)
                {
                    if (drawShape.ForeColor != Color.Transparent)
                    {
                        layerNode.ForeColor = drawShape.ForeColor;
                    }

                    var shapeNode = MakeShapeNode(drawShape);
                    layerNode.Nodes.Add(shapeNode);
                    if (drawShape == selectedShape)
                    {
                        treeView1.SelectedNode = shapeNode;
                    }
                }

                layerNode.Expand();
                if (selectedLayer == layer.Key)
                {
                    treeView1.SelectedNode = layerNode;
                }
            }

            treeView1.EndUpdate();
        }

        private static TreeNode MakeShapeNode(DrawShape drawShape)
        {
            var node = new TreeNode("Shape (" + (drawShape.IsEraser ? "erase" : "draw") + ")") { Tag = drawShape };
            foreach (var p in drawShape.Points)
            {
                var text = p.GetText(p.X, p.Y);
                var subNode = new TreeNode(text) { Tag = p };
                node.Nodes.Add(subNode);
            }

            if (drawShape.Expanded)
            {
                node.Expand();
            }

            return node;
        }

        private void TreeViewUpdate(DrawShape drawShape)
        {
            foreach (TreeNode node in treeView1.Nodes)
            {
                foreach (TreeNode subNode in node.Nodes)
                {
                    if (subNode.Tag is DrawShape shape && shape == drawShape)
                    {
                        subNode.Nodes.Clear();
                        foreach (var p in drawShape.Points)
                        {
                            var text = p.GetText(p.X, p.Y);
                            var pointNode = new TreeNode(text) { Tag = p };
                            subNode.Nodes.Add(pointNode);
                        }

                        break;
                    }
                }
            }
        }

        private void TreeViewAdd(DrawShape drawShape, bool focus)
        {
            if (treeView1.Nodes.Count == 0)
            {
                TreeViewFill(_drawShapes);
                return;
            }

            var activeLayer = treeView1.Visible && treeView1.SelectedNode?.Tag is int layer ? layer : int.MinValue;
            if (activeLayer > int.MinValue)
            {
                drawShape.Layer = activeLayer;
                var idx = treeView1.SelectedNode.Nodes.Add(MakeShapeNode(drawShape));
                if (focus)
                {
                    treeView1.SelectedNode = treeView1.SelectedNode.Nodes[idx];
                }

                return;
            }

            var nodeLast = treeView1.Nodes[treeView1.Nodes.Count - 1];
            {
                var idx = nodeLast.Nodes.Add(MakeShapeNode(drawShape));
                if (focus)
                {
                    treeView1.SelectedNode = nodeLast.Nodes[idx];
                }
            }
        }

        private void TreeViewRemove(DrawShape drawShape)
        {
            foreach (TreeNode node in treeView1.Nodes)
            {
                foreach (TreeNode subNode in node.Nodes)
                {
                    if (subNode.Tag is DrawShape shape && shape == drawShape)
                    {
                        node.Nodes.Remove(subNode);
                        return;
                    }
                }
            }
        }

        public static Bitmap MakeGrayScale(Bitmap original)
        {
            var newBitmap = new Bitmap(original.Width, original.Height);
            using (var g = Graphics.FromImage(newBitmap))
            {
                var colorMatrix = new ColorMatrix(
                    new float[][]
                    {
                        new float[] {.3f, .3f, .3f, 0, 0},
                        new float[] {.59f, .59f, .59f, 0, 0},
                        new float[] {.11f, .11f, .11f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0, 0, 1}
                    });

                using (var attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix);
                    g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;
        }

        private void FormAssaDrawMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.P)
            {
                if (toolStripButtonPreview.Enabled)
                {
                    e.SuppressKeyPress = true;
                    var timer1 = new Timer { Interval = 50 };
                    timer1.Tick += (o, s) =>
                    {
                        timer1.Stop();
                        timer1.Dispose();
                        toolStripButtonPreview_Click(null, null);
                    };
                    timer1.Start();
                }
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.V)
            {
                if (Clipboard.ContainsImage())
                {
                    _backgroundImage = new Bitmap(Clipboard.GetImage());
                    numericUpDownWidth.Value = _backgroundImage.Width;
                    numericUpDownHeight.Value = _backgroundImage.Height;
                    _backgroundOff = false;
                    pictureBoxCanvas.Invalidate();
                    e.SuppressKeyPress = true;
                }
                else if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText();
                    if (text.Contains("\\p1") && text.Contains("[V4+ Styles]"))
                    {
                        ImportAssaDrawingFromFileText(text);
                        e.SuppressKeyPress = true;
                    }
                    else if (text.Contains("\\p1"))
                    {
                        ImportAssaDrawingFromText(text, 0, Color.Transparent, false);
                        e.SuppressKeyPress = true;
                    }
                }
                else if (Clipboard.ContainsFileDropList())
                {
                    var files = Clipboard.GetFileDropList();
                    if (files.Count == 1)
                    {
                        ImportFile(files[0]);
                    }
                }
            }
            else if (e.Modifiers == Keys.Control && (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0))
            {
                _zoomFactor = 1; // reset zoom
                _panX = 0;
                _panY = 0;
                ShowTitle();
                pictureBoxCanvas.Invalidate();
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add))
            {
                _zoomFactor += 0.1f; // reset zoom
                ZoomChangedPostFix();
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract))
            {
                _zoomFactor -= 0.1f; // reset zoom
                ZoomChangedPostFix();
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add))
            {
                ScaleActiveShapes(1.1f);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract))
            {
                ScaleActiveShapes(0.9f);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.C)
            {
                buttonCopyAssaToClipboard_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.N)
            {
                toolStripButtonNew_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F2)
            {
                if (toolStripButtonColorPicker.Checked)
                {
                    toolStripButtonLine_Click(null, null);
                }
                else if (toolStripButtonLine.Checked)
                {
                    toolStripButtonBeizer_Click(null, null);
                }
                else if (toolStripButtonBeizer.Checked)
                {
                    toolStripButtonRectangle_Click(null, null);
                }
                else if (toolStripButtonRectangle.Checked)
                {
                    toolStripButtonCircle_Click(null, null);
                }
                else if (toolStripButtonCircle.Checked)
                {
                    toolStripButtonColorPicker_Click(null, null);
                }

                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F2)
            {
                if (_backgroundImage == null)
                {
                    chooseBackgroundImagesToolStripMenuItem_Click(null, null);
                }
                else if (_backgroundOff)
                {
                    _backgroundOff = false;
                }
                else
                {
                    _backgroundOff = true;
                }

                pictureBoxCanvas.Invalidate();
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.F2)
            {
                if (_backgroundImage != null)
                {
                    _backgroundImage = MakeGrayScale(_backgroundImage);
                    _backgroundOff = false;
                    pictureBoxCanvas.Invalidate();
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F3)
            {
                toolStripButtonColorPicker_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F4)
            {
                toolStripButtonLine_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F5)
            {
                toolStripButtonBeizer_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F6)
            {
                toolStripButtonRectangle_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F7)
            {
                toolStripButtonCircle_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F8)
            {
                toolStripButtonCloseShape_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F12)
            {
                if (treeView1.Visible)
                {
                    SuspendLayout();
                    HideSettingsAndTreeView();
                    ResumeLayout();
                }
                else
                {
                    SuspendLayout();
                    ShowSettingsAndTreeView();
                    ResumeLayout();
                }

                pictureBoxCanvas.Invalidate();
                e.SuppressKeyPress = true;
            }
            else if (toolStripButtonPreview.Checked && e.KeyCode == Keys.Escape)
            {
                toolStripButtonPreview_Click(null, null);
            }
            else if (_activeDrawShape != null && e.KeyCode == Keys.Escape)
            {
                _drawingStartTicks = 0;
                if (!_drawShapes.Contains(_activeDrawShape))
                {
                    toolStripButtonClearCurrent_Click(null, null);
                }
                else if (_oldDrawShape != null)
                {
                    _activeDrawShape.Points = _oldDrawShape.Points;
                    _oldDrawShape = null;
                    _activeDrawShape = null;
                    pictureBoxCanvas.Invalidate();
                }
                else
                {
                    _activeDrawShape = null;
                }

                e.SuppressKeyPress = true;
            }
            else if (_activeDrawShape != null && e.KeyCode == Keys.Enter)
            {
                toolStripButtonCloseShape_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (ActiveControl == treeView1 && e.KeyCode == Keys.Delete && treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Tag is DrawCoordinate)
                {
                    deletePointToolStripMenuItem_Click(null, null);
                    e.SuppressKeyPress = true;
                }
                else if (treeView1.SelectedNode.Tag is DrawShape)
                {
                    buttonRemoveShape_Click(null, null);
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Z)
            {
                _activeDrawShape = null;
                Undo();
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Y)
            {
                _activeDrawShape = null;
                Redo();
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Alt || e.Modifiers == Keys.Control)
            {
                var v = e.Modifiers == Keys.Alt ? 1 : 10;

                if (e.KeyCode == Keys.Up)
                {
                    AdjustPosition(0, -v, false);
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    AdjustPosition(0, v, false);
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Left)
                {
                    AdjustPosition(-v, 0, false);
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Right)
                {
                    AdjustPosition(v, 0, false);
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift))
            {
                var v = 2;

                if (e.KeyCode == Keys.Up)
                {
                    AdjustPosition(0, -v, true);
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    AdjustPosition(0, v, true);
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Left)
                {
                    AdjustPosition(-v, 0, true);
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Right)
                {
                    AdjustPosition(v, 0, true);
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == Keys.Shift)
            {
                if (e.KeyCode == Keys.Up)
                {
                    _panY--;
                    pictureBoxCanvas.Invalidate();
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    _panY++;
                    pictureBoxCanvas.Invalidate();
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Left)
                {
                    _panX--;
                    pictureBoxCanvas.Invalidate();
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Right)
                {
                    _panX++;
                    pictureBoxCanvas.Invalidate();
                    e.SuppressKeyPress = true;
                }
            }
        }

        private void ShowSettingsAndTreeView()
        {
            DrawSettings.HideSettingsAndTreeView = false;
            pictureBoxCanvas.Left = treeView1.Left + treeView1.Width + 15;
            pictureBoxCanvas.Width = Width - pictureBoxCanvas.Left - 27;
            labelPosition.Left = pictureBoxCanvas.Left;
            groupBoxSettings.Visible = true;
            treeView1.Visible = true;
        }

        private void HideSettingsAndTreeView()
        {
            DrawSettings.HideSettingsAndTreeView = true;
            var w = pictureBoxCanvas.Width + (pictureBoxCanvas.Left - treeView1.Left);
            pictureBoxCanvas.Left = treeView1.Left;
            pictureBoxCanvas.Width = w;
            labelPosition.Left = treeView1.Left;
            groupBoxSettings.Visible = false;
            treeView1.Visible = false;
        }

        private void ScaleActiveShapes(float factor)
        {
            var activeLayer = _activeDrawShape == null && treeView1.Visible && treeView1.SelectedNode?.Tag is int layer ? layer : int.MinValue;
            if (_activeDrawShape == null && activeLayer > int.MinValue)
            {
                var allPoints = new List<DrawCoordinate>();
                var layerShapes = _drawShapes.Where(p => p.Layer == activeLayer).ToList();
                foreach (var drawShape in layerShapes)
                {
                    allPoints.AddRange(drawShape.Points);
                }

                if (allPoints.Count == 0)
                {
                    return;
                }

                var minX = allPoints.Min(p => p.X);
                var minY = allPoints.Min(p => p.Y);
                var maxX = allPoints.Max(p => p.X);
                var maxY = allPoints.Max(p => p.Y);
                if (factor < 1 && (maxX - minX < 5 || maxY - minY < 5))
                {
                    return;
                }

                foreach (var point in allPoints)
                {
                    var x = point.X - minX;
                    var y = point.Y - minY;
                    var newX = x * factor + minX;
                    var newY = y * factor + minY;
                    point.X = newX;
                    point.Y = newY;
                }

                treeView1.BeginUpdate();
                foreach (var drawShape in layerShapes)
                {
                    TreeViewUpdate(drawShape);
                }

                treeView1.EndUpdate();
            }
            else if (_activeDrawShape == null)
            {
                return;
            }
            else
            {
                if (!ScaleShape(_activeDrawShape, factor))
                {
                    return;
                }

                TreeViewUpdate(_activeDrawShape);
            }

            pictureBoxCanvas.Invalidate();
        }

        private void ScaleAll(float factor)
        {
            var allPoints = _drawShapes.SelectMany(p => p.Points).ToList();
            if (allPoints.Count == 0)
            {
                return;
            }

            var minX = allPoints.Min(p => p.X);
            var minY = allPoints.Min(p => p.Y);
            var maxX = allPoints.Max(p => p.X);
            var maxY = allPoints.Max(p => p.Y);
            if (factor < 1 && (maxX - minX < 5 || maxY - minY < 5))
            {
                return;
            }

            foreach (var point in allPoints)
            {
                var x = point.X - minX;
                var y = point.Y - minY;
                var newX = x * factor + minX;
                var newY = y * factor + minY;
                point.X = newX;
                point.Y = newY;
            }

            _treeViewInvalidated = true;
            pictureBoxCanvas.Invalidate();
        }

        private bool ScaleShape(DrawShape shape, float factor)
        {
            var minX = shape.Points.Min(p => p.X);
            var minY = shape.Points.Min(p => p.Y);
            var maxX = shape.Points.Max(p => p.X);
            var maxY = shape.Points.Max(p => p.Y);
            if (factor < 1 && (maxX - minX < 5 || maxY - minY < 5))
            {
                return false;
            }

            foreach (var point in shape.Points)
            {
                var x = point.X - minX;
                var y = point.Y - minY;
                var newX = x * factor + minX;
                var newY = y * factor + minY;
                point.X = newX;
                point.Y = newY;
            }

            return true;
        }

        private void AdjustPosition(int xAdjust, int yAdjust, bool all)
        {
            if (!all && treeView1.SelectedNode?.Tag is DrawShape shape)
            {
                foreach (var p in shape.Points)
                {
                    p.X += xAdjust;
                    p.Y += yAdjust;
                }

                TreeViewUpdate(shape);
                pictureBoxCanvas.Invalidate();
                return;
            }

            if (!all && treeView1.SelectedNode?.Tag is int layer) // layer
            {
                foreach (var p in _drawShapes.Where(p => p.Layer == layer).SelectMany(drawShape => drawShape.Points))
                {
                    p.X += xAdjust;
                    p.Y += yAdjust;
                }

                _treeViewInvalidated = true;
                pictureBoxCanvas.Invalidate();
                return;
            }

            if (_activeDrawShape != null && !all)
            {
                foreach (var p in _activeDrawShape.Points)
                {
                    p.X += xAdjust;
                    p.Y += yAdjust;
                }

                TreeViewUpdate(_activeDrawShape);
                pictureBoxCanvas.Invalidate();
                return;
            }

            foreach (var p in _drawShapes.SelectMany(drawShape => drawShape.Points))
            {
                p.X += xAdjust;
                p.Y += yAdjust;
            }

            _treeViewInvalidated = true;
            pictureBoxCanvas.Invalidate();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            numericUpDownX.Enabled = false;
            numericUpDownY.Enabled = false;
            var tag = e.Node.Tag;
            if (e.Node.Nodes.Count == 0 && tag is DrawCoordinate point)
            {
                numericUpDownX.Value = (decimal)point.X;
                numericUpDownY.Value = (decimal)point.Y;
                _activePoint = point;
                _activeDrawShape = null;
                numericUpDownX.Enabled = true;
                numericUpDownY.Enabled = true;
            }
            else if (tag is DrawShape command)
            {
                _activePoint = null;
                numericUpDownX.Enabled = false;
                numericUpDownY.Enabled = false;
                _activeDrawShape = command;
                _oldDrawShape = new DrawShape(command);
                _x = int.MinValue;
                _y = int.MinValue;
            }
            else if (tag is int)
            {
                _activePoint = null;
                numericUpDownX.Enabled = false;
                numericUpDownY.Enabled = false;
                _activeDrawShape = null;
                _x = int.MinValue;
                _y = int.MinValue;
            }

            pictureBoxCanvas.Invalidate();
            EnableDisableCurrentShapeActions();
        }

        private void numericUpDownX_ValueChanged(object sender, EventArgs e)
        {
            if (!numericUpDownX.Enabled)
            {
                return;
            }

            var p = (DrawCoordinate)treeView1.SelectedNode.Tag;
            p.X = (int)numericUpDownX.Value;
            pictureBoxCanvas.Invalidate();
        }

        private void numericUpDownY_ValueChanged(object sender, EventArgs e)
        {
            if (!numericUpDownY.Enabled)
            {
                return;
            }

            var p = (DrawCoordinate)treeView1.SelectedNode.Tag;
            p.Y = (int)numericUpDownY.Value;
            pictureBoxCanvas.Invalidate();

            var i = treeView1.SelectedNode.Index;
            if (i == 0)
            {
                treeView1.SelectedNode.Text = $"Move to {p.X} {p.Y}";
            }
            else
            {
                treeView1.SelectedNode.Text = $"Line to {p.X} {p.Y}";
            }
        }

        private void buttonRemoveShape_Click(object sender, EventArgs e)
        {
            if (_activeDrawShape != null)
            {
                _drawShapes.Remove(_activeDrawShape);
            }

            TreeViewFill(_drawShapes);
            _activeDrawShape = null;
            pictureBoxCanvas.Invalidate();
        }

        private void pictureBoxCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            _moveActiveDrawShapeStart = new Point(int.MinValue, int.MinValue);
            _mouseDownPoint = null;
            _panOn = false;
        }

        private void pictureBoxCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            var x = FromZoomFactor(e.X - _panX);
            var y = FromZoomFactor(e.Y - _panY);
            _moveActiveDrawShapeStart = new Point(int.MinValue, int.MinValue);

            if (e.Button == MouseButtons.Left && ModifierKeys == Keys.Shift)
            {
                // pan
                _activePoint = null;
                _activeDrawShape = null;
                _mouseDownPoint = new DrawCoordinate(new DrawShape(), DrawCoordinateType.None, e.Location.X, e.Location.Y, Color.Transparent);
                _panOn = true;
                numericUpDownX.Enabled = false;
                numericUpDownY.Enabled = false;
                Cursor = Cursors.SizeAll;
                return;
            }

            var closePoint = GetClosePoint(x, y);
            if (closePoint != null)
            {
                if (e.Button == MouseButtons.Left)
                {
                    Cursor = Cursors.Hand;
                    _activePoint = closePoint;
                    _mouseDownPoint = closePoint;
                    pictureBoxCanvas.Invalidate();
                    SelectTreeViewNodePoint(closePoint);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    _activePoint = closePoint;
                    pictureBoxCanvas.Invalidate();
                    SelectTreeViewNodePoint(closePoint);
                    contextMenuStripTreeView.Show(pictureBoxCanvas, e.X, e.Y);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                contextMenuStripCanvasBackground.Show(pictureBoxCanvas, e.X, e.Y);
            }
            else if (ModifierKeys == Keys.Control && _activeDrawShape != null && _drawShapes.Contains(_activeDrawShape))
            {
                // move active shape
                _moveActiveDrawShapeStart = new Point(x, y);
                Cursor = Cursors.SizeAll;
            }
            else if (ModifierKeys == (Keys.Control | Keys.Shift) && _drawShapes.Count > 0)
            {
                // move ALL
                _moveActiveDrawShapeStart = new Point(x, y);
                Cursor = Cursors.SizeAll;
            }
            else
            {
                var activeLayer = _activeDrawShape == null && treeView1.Visible && treeView1.SelectedNode?.Tag is int layer ? layer : int.MinValue;
                if (_activeDrawShape == null && activeLayer > int.MinValue && ModifierKeys == Keys.Control)
                {
                    // move active shape
                    _moveActiveDrawShapeStart = new Point(x, y);
                    Cursor = Cursors.SizeAll;
                }
                else if (_activeDrawShape != null && toolStripButtonCircle.Checked &&
                         _drawingStartTicks > 0 && DateTime.UtcNow.Ticks - _drawingStartTicks > 10_000 * 250)
                {
                    toolStripButtonCloseShape_Click(null, null);
                    _drawingEndTicks = DateTime.UtcNow.Ticks;
                }
                else if (_activeDrawShape != null && toolStripButtonRectangle.Checked &&
                         _drawingStartTicks > 0 && DateTime.UtcNow.Ticks - _drawingStartTicks > 10_000 * 250)
                {
                    toolStripButtonCloseShape_Click(null, null);
                    _drawingEndTicks = DateTime.UtcNow.Ticks;
                }
            }
        }

        private void toolStripButtonCloseShape_Click(object sender, EventArgs e)
        {
            if (_activeDrawShape == null || _x == int.MinValue || _y == int.MinValue)
            {
                pictureBoxCanvas.Invalidate();
                return;
            }

            if (toolStripButtonCircle.Checked)
            {
                var start = _activeDrawShape.Points.FirstOrDefault();
                if (start == null)
                {
                    return;
                }

                var xDiff = _x > start.X ? _x - start.X : start.X - _x;
                var yDiff = _y > start.Y ? _y - start.Y : start.Y - _y;
                var maxDiff = Math.Max(xDiff, yDiff);
                if (maxDiff < 1)
                {
                    return;
                }

                _activeDrawShape = CirleBezier.MakeCircle(start.X, start.Y, maxDiff, _activeDrawShape.Layer, _activeDrawShape.ForeColor);
            }
            if (toolStripButtonRectangle.Checked && !_drawShapes.Contains(_activeDrawShape))
            {
                var start = _activeDrawShape.Points.FirstOrDefault();
                if (start == null)
                {
                    return;
                }

                var xDiff = _x - start.X;
                var yDiff = _y - start.Y;
                _activeDrawShape = MakeRectangle(start.X, start.Y, xDiff, yDiff, _activeDrawShape.Layer, _activeDrawShape.ForeColor);
                _activeDrawShape.Points.RemoveAt(_activeDrawShape.Points.Count - 1);
            }
            else if (_activeDrawShape.Points.Count <= 2)
            {
                return;
            }

            _activePoint = null;
            numericUpDownX.Enabled = false;
            numericUpDownY.Enabled = false;
            _drawingStartTicks = 0;

            if (_activeDrawShape != null && !_drawShapes.Contains(_activeDrawShape))
            {
                _activeDrawShape.ForeColor = Color.Transparent;
                var firstLayerZeoShape = _drawShapes.FirstOrDefault(p => p.Layer == 0);
                if (firstLayerZeoShape != null)
                {
                    _activeDrawShape.ForeColor = firstLayerZeoShape.ForeColor;
                }

                _drawShapes.Add(_activeDrawShape);
                TreeViewAdd(_activeDrawShape, true);
            }

            _x = int.MinValue;
            _y = int.MinValue;
            pictureBoxCanvas.Invalidate();
            EnableDisableCurrentShapeActions();
        }

        private void toolStripButtonClearCurrent_Click(object sender, EventArgs e)
        {
            deleteShapeToolStripMenuItem_Click(null, null);
        }

        private void ClearAll()
        {
            _x = int.MinValue;
            _y = int.MinValue;
            _zoomFactor = 1.0f;
            _panX = 0;
            _panY = 0;
            treeView1.Nodes.Clear();
            _activeDrawShape = null;
            _activePoint = null;
            _oldDrawShape = null;
            _activePoint = null;
            numericUpDownX.Enabled = false;
            numericUpDownY.Enabled = false;
            _drawShapes.Clear();
            pictureBoxCanvas.Invalidate();
        }

        private Subtitle GetAssaDrawCode()
        {
            var subtitle = new Subtitle();
            subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
            var assaDrawStyle = new SsaStyle
            {
                Name = StyleName,
                Alignment = "7",
                MarginVertical = 0,
                MarginLeft = 0,
                MarginRight = 0,
                ShadowWidth = 0,
                OutlineWidth = 0,
            };
            subtitle.Header = AdvancedSubStationAlpha.AddSsaStyle(assaDrawStyle, subtitle.Header);
            subtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + ((int)numericUpDownWidth.Value).ToString(CultureInfo.InvariantCulture), "[Script Info]", subtitle.Header);
            subtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + ((int)numericUpDownHeight.Value).ToString(CultureInfo.InvariantCulture), "[Script Info]", subtitle.Header);

            foreach (var layer in _drawShapes.OrderBy(p => p.Layer).GroupBy(p => p.Layer))
            {
                // make {\p1}...{\p0} tag
                var sb = new StringBuilder();
                var color = Color.White;
                var outlineColor = Color.White;
                var outlineWidth = 0; ;
                foreach (var drawShape in layer.Where(p => !p.IsEraser))
                {
                    color = drawShape.ForeColor;
                    outlineColor = drawShape.OutlineColor;
                    outlineWidth = drawShape.OutlineWidth;
                    sb.Append(drawShape.ToAssa());
                    sb.Append("  ");
                }
                var drawText = sb.ToString().Trim();
                if (!string.IsNullOrEmpty(drawText))
                {
                    drawText = WrapInPTag(drawText, color, outlineWidth, outlineColor);
                }

                // make {\iclip(...)} tag
                sb.Clear();
                foreach (var drawShape in layer.Where(p => p.IsEraser))
                {
                    sb.Append(drawShape.ToAssa());
                    sb.Append("  ");
                }
                var iClipText = string.Empty;
                var iClip = sb.ToString().Trim();
                if (!string.IsNullOrEmpty(iClip))
                {
                    iClipText = WrapInIClipTag(iClip);
                }

                // add subtitle if any content
                if (!string.IsNullOrEmpty(drawText) || !string.IsNullOrEmpty(iClipText))
                {
                    var p = new Paragraph((drawText + Environment.NewLine + iClipText).Trim(), 0, 10000)
                    {
                        Extra = StyleName,
                        Layer = layer.Key,
                    };
                    subtitle.Paragraphs.Add(p);
                }
            }

            return subtitle;
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            var sub = GetAssaDrawCode();
            if (sub.Paragraphs.Count == 0)
            {
                return;
            }

            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.FileName = Path.GetFileName(_fileName);
                saveFileDialog.Filter = "ASSA drawing|*.assadraw|SVG|*.svg";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _fileName = saveFileDialog.FileName;
                    if (_fileName.EndsWith(".svg", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SvgWriter.SaveSvg(_fileName, _drawShapes, (int)numericUpDownWidth.Value, (int)numericUpDownHeight.Value);
                    }
                    else
                    {
                        File.WriteAllText(_fileName, new AdvancedSubStationAlpha().ToText(sub, string.Empty));
                    }

                    ShowTitle();
                }
            }
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.FileName = string.Empty;
                openFileDialog.Filter = "Supported files|*.assadraw;*.ass;*.svg|ASSA drawing|*.assadraw|ASSA files|*.ass|SVG files|*.svg";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ClearAll();

                    if (openFileDialog.FileName.EndsWith(".svg", true, CultureInfo.InvariantCulture))
                    {
                        _drawShapes.AddRange(SvgReader.LoadSvg(openFileDialog.FileName));
                        pictureBoxCanvas.Invalidate();
                        TreeViewFill(_drawShapes);
                        return;
                    }

                    ImportAssaDrawing(openFileDialog.FileName);
                }
            }
        }

        private void ImportAssaDrawing(string fileName)
        {
            var text = File.ReadAllText(fileName);
            _fileName = fileName;
            ImportAssaDrawingFromFileText(text);
        }

        private void ImportAssaDrawingFromFileText(string text)
        {
            _activeDrawShape = null;
            var sub = new Subtitle();
            var format = new AdvancedSubStationAlpha();
            format.LoadSubtitle(sub, text.SplitToLines(), _fileName);

            var playResX = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", sub.Header);
            if (int.TryParse(playResX, out var w) && w >= numericUpDownWidth.Minimum && w <= numericUpDownWidth.Maximum)
            {
                numericUpDownWidth.Value = w;
            }

            var playResY = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResY", "[Script Info]", sub.Header);
            if (int.TryParse(playResY, out var h) && w >= numericUpDownHeight.Minimum && w <= numericUpDownHeight.Maximum)
            {
                numericUpDownHeight.Value = h;
            }

            var styles = AdvancedSubStationAlpha.GetStylesFromHeader(sub.Header);
            var regexAlpha = new Regex(@"\\1a&H[0123456789ABCDEFabcdef]{1,2}&");
            var regexColor = new Regex(@"\\1?c&H[0123456789ABCDEFabcdef]{1,8}&");
            var regexIClip = new Regex(@"{\\iclip\([mblspcn0123456789\s\.-]*\)}");
            var regexOutline = new Regex(@"\\bord[0123456789\.]*[}\\]");
            var regexOutlineColor = new Regex(@"\\3c&H[0123456789ABCDEFabcdef]{1,8}&");
            var regexOutlineAlpha = new Regex(@"\\3a&H[0123456789ABCDEFabcdef]{1,2}&");
            if (sub.Paragraphs.Count > 0)
            {
                foreach (var p in sub.Paragraphs)
                {
                    var c = Color.Transparent;
                    var outlineColor = Color.Transparent;
                    var outlineWidth = 0;
                    var styleName = styles.FirstOrDefault(s => s == p.Extra);
                    if (!string.IsNullOrEmpty(styleName))
                    {
                        var style = AdvancedSubStationAlpha.GetSsaStyle(styleName, sub.Header);
                        if (style != null)
                        {
                            c = style.Primary;
                        }
                    }

                    var match = regexColor.Match(p.Text);
                    if (match.Success)
                    {
                        var colorString = match.Value;
                        if (colorString.Length > 2)
                        {
                            if (colorString.StartsWith("\\1c", StringComparison.InvariantCultureIgnoreCase))
                            {
                                colorString = colorString.Remove(0, 3);
                            }
                            else
                            {
                                colorString = colorString.Remove(0, 2);
                            }
                        }
                        c = AdvancedSubStationAlpha.GetSsaColor(colorString, c);
                    }

                    match = regexAlpha.Match(p.Text);
                    if (match.Success)
                    {
                        var alpha = match.Value.Remove(0, 5).TrimEnd('&');
                        try
                        {
                            var a = int.Parse(alpha, NumberStyles.HexNumber);
                            c = Color.FromArgb(byte.MaxValue - a, c);
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    var borderOutlineMatch = regexOutline.Match(p.Text);
                    if (borderOutlineMatch.Success)
                    {
                        var outline = borderOutlineMatch.Value.Remove(0, 5).TrimEnd('\\');
                        if (float.TryParse(outline, NumberStyles.AllowDecimalPoint, CultureInfo.InstalledUICulture, out var f))
                        {
                            outlineWidth = (int)f;
                        }
                    }

                    match = regexOutlineColor.Match(p.Text);
                    if (match.Success)
                    {
                        var colorString = match.Value;
                        colorString = colorString.Remove(0, 3);
                        outlineColor = AdvancedSubStationAlpha.GetSsaColor(colorString, c);
                    }

                    match = regexOutlineAlpha.Match(p.Text);
                    if (match.Success)
                    {
                        var alpha = match.Value.Remove(0, 5).TrimEnd('&');
                        try
                        {
                            var a = int.Parse(alpha, NumberStyles.HexNumber);
                            outlineColor = Color.FromArgb(byte.MaxValue - a, outlineColor);
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    var clipMatch = regexIClip.Match(p.Text);
                    if (clipMatch.Success)
                    {
                        var drawText = p.Text.Remove(clipMatch.Index, clipMatch.Value.Length);
                        ImportAssaDrawingFromText(drawText, p.Layer, c, false, outlineWidth, outlineColor);

                        var eraseText = clipMatch.Value.Replace("{\\iclip(", string.Empty).TrimEnd('}').TrimEnd(')');
                        ImportAssaDrawingFromText(eraseText, p.Layer, c, true, outlineWidth, outlineColor);
                    }
                    else
                    {
                        ImportAssaDrawingFromText(p.Text, p.Layer, c, false, outlineWidth, outlineColor);
                    }
                }
                treeView1.Enabled = true;
                ShowTitle();
                return;
            }

            SetAssaStartEndTags(text);
            ImportAssaDrawingFromText(text, 0, Color.Transparent, false);
            ShowTitle();
        }

        private void ImportAssaDrawingFromText(string text, int layer, Color c, bool isEraser, int outlineWidth = 0, Color? outlineColorOrNull = null)
        {
            var outlineColor = outlineColorOrNull ?? Color.Transparent;
            text = _regexStart.Replace(text, string.Empty);
            text = _regexEnd.Replace(text, string.Empty);
            var arr = text.Split();
            var i = 0;
            var bezierCount = 0;
            var state = DrawCoordinateType.None;
            DrawCoordinate moveCoordinate = null;
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
                        drawShape.OutlineWidth = outlineWidth;
                        drawShape.OutlineColor = outlineColor;
                        drawShape.IsEraser = isEraser;
                        drawShape.AddPoint(state, moveCoordinate.X, moveCoordinate.Y, DrawSettings.PointColor);
                        moveCoordinate = null;
                        _drawShapes.Add(drawShape);
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
                        drawShape.OutlineWidth = outlineWidth;
                        drawShape.OutlineColor = outlineColor;
                        drawShape.IsEraser = isEraser;
                        drawShape.AddPoint(state, moveCoordinate.X, moveCoordinate.Y, DrawSettings.PointColor);
                        moveCoordinate = null;
                        _drawShapes.Add(drawShape);
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
                    arr.Length >= i + 2 &&
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

            TreeViewFill(_drawShapes);
            _x = int.MinValue;
            _y = int.MinValue;
            pictureBoxCanvas.Invalidate();
        }

        private void FormAssaDrawMain_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void FormAssaDrawMain_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1)
                {
                    ImportFile(files[0]);
                }
            }
        }

        private void ImportFile(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (ext == ".assadraw")
            {
                ImportAssaDrawing(fileName);
            }

            if (ext == ".svg")
            {
                _drawShapes.AddRange(SvgReader.LoadSvg(fileName));
                pictureBoxCanvas.Invalidate();
                TreeViewFill(_drawShapes);
            }

            if (ext == ".png" || ext == ".jpg" || ext == ".bmp" || ext == ".tiff" || ext == ".jpe" || ext == ".jpeg" || ext == ".gif")
            {
                SetBackgroundImage(fileName);
            }
        }

        private void SetBackgroundImage(string fileName)
        {
            _backgroundImage?.Dispose();
            _backgroundImage = new Bitmap(fileName);
            numericUpDownWidth.Value = _backgroundImage.Width;
            numericUpDownHeight.Value = _backgroundImage.Height;
            _backgroundOff = false;
            pictureBoxCanvas.Invalidate();
        }

        private void toolStripButtonLine_Click(object sender, EventArgs e)
        {
            toolStripButtonBeizer.Checked = false;
            toolStripButtonCircle.Checked = false;
            toolStripButtonRectangle.Checked = false;
            toolStripButtonLine.Checked = true;
            toolStripButtonColorPicker.Checked = false;
            textBoxColorPicker.Visible = false;
            panelColorPicker.Visible = false;
            panelColorPickerChoice.Visible = false;
            buttonSetColor.Visible = false;
            buttonSetOutlineColor.Visible = false;
        }

        private void toolStripButtonBeizer_Click(object sender, EventArgs e)
        {
            toolStripButtonLine.Checked = false;
            toolStripButtonCircle.Checked = false;
            toolStripButtonRectangle.Checked = false;
            toolStripButtonBeizer.Checked = true;
            toolStripButtonColorPicker.Checked = false;
            textBoxColorPicker.Visible = false;
            panelColorPicker.Visible = false;
            panelColorPickerChoice.Visible = false;
            buttonSetColor.Visible = false;
            buttonSetOutlineColor.Visible = false;
        }

        private void toolStripButtonCircle_Click(object sender, EventArgs e)
        {
            if (_activeDrawShape?.Points.Count > 1 && !_drawShapes.Contains(_activeDrawShape))
            {
                // cannot combine circle with bezier or lines
                return;
            }

            toolStripButtonBeizer.Checked = false;
            toolStripButtonLine.Checked = false;
            toolStripButtonRectangle.Checked = false;
            toolStripButtonCircle.Checked = true;
            toolStripButtonColorPicker.Checked = false;
            textBoxColorPicker.Visible = false;
            panelColorPicker.Visible = false;
            textBoxColorPicker.Visible = false;
            panelColorPickerChoice.Visible = false;
            buttonSetColor.Visible = false;
            buttonSetOutlineColor.Visible = false;
        }


        private void toolStripButtonRectangle_Click(object sender, EventArgs e)
        {
            if (_activeDrawShape?.Points.Count > 1 && !_drawShapes.Contains(_activeDrawShape))
            {
                // cannot combine rect with bezier or lines
                return;
            }

            toolStripButtonBeizer.Checked = false;
            toolStripButtonLine.Checked = false;
            toolStripButtonRectangle.Checked = true;
            toolStripButtonCircle.Checked = false;
            toolStripButtonColorPicker.Checked = false;
            textBoxColorPicker.Visible = false;
            panelColorPicker.Visible = false;
            panelColorPickerChoice.Visible = false;
            buttonSetColor.Visible = false;
            buttonSetOutlineColor.Visible = false;
        }


        private void buttonCopyAssaToClipboard_Click(object sender, EventArgs e)
        {
            var sub = GetAssaDrawCode();
            if (sub.Paragraphs.Count == 1)
            {
                Clipboard.SetText(sub.Paragraphs[0].Text);
            }
            else if (sub.Paragraphs.Count > 1)
            {
                Clipboard.SetText(new AdvancedSubStationAlpha().ToText(sub, string.Empty));
            }

            pictureBoxCanvas.Focus();
        }

        private void _historyTimer_Tick(object sender, EventArgs e)
        {
            lock (_historyLock)
            {
                var newHistoryItem = _history.MakeHistoryItem(_drawShapes, _activeDrawShape, _oldDrawShape, _activePoint, _zoomFactor);
                var newHash = newHistoryItem.GetFastHashCode();
                if (newHash == _historyHash)
                {
                    return;
                }

                _historyHash = newHash;
                if (_historyHash == 0)
                {
                    return;
                }

                _history.AddChange(newHistoryItem);

            }
        }

        private void SetPropertiesFromHistory(DrawHistoryItem item)
        {
            _drawShapes.Clear();
            _drawShapes.AddRange(item.DrawShapes);
            _activeDrawShape = item.ActiveDrawShape;
            _oldDrawShape = item.OldDrawShape;
            _activePoint = item.ActivePoint;
            _zoomFactor = item.ZoomFactor;
        }

        private void Undo()
        {
            lock (_historyLock)
            {
                var item = _history.Undo();
                if (item == null)
                {
                    return;
                }

                SetPropertiesFromHistory(item);
                _historyHash = item.GetFastHashCode();
            }

            pictureBoxCanvas.Invalidate();
            TreeViewFill(_drawShapes);
        }

        private void Redo()
        {
            lock (_historyLock)
            {
                var item = _history.Redo();
                if (item == null)
                {
                    return;
                }

                SetPropertiesFromHistory(item);
                _historyHash = item.GetFastHashCode();
            }

            pictureBoxCanvas.Invalidate();
            TreeViewFill(_drawShapes);
        }

        private void contextMenuStripTreeView_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                e.Cancel = true;
                return;
            }

            deleteShapeToolStripMenuItem.Visible = false;
            deletePointToolStripMenuItem.Visible = false;
            duplicatePointToolStripMenuItem.Visible = false;
            setColorToolStripMenuItem.Visible = false;
            setLayerToolStripMenuItem.Visible = false;
            changeLayerToolStripMenuItem.Visible = false;
            deleteLayerToolStripMenuItem.Visible = false;
            useShapeForEraseToolStripMenuItem.Visible = false;
            useShapeForDrawToolStripMenuItem.Visible = false;
            hideLayerToolStripMenuItem.Visible = false;
            showLayerToolStripMenuItem.Visible = false;
            setOutlineColorToolStripMenuItem.Visible = false;
            setOutlineWidthToolStripMenuItem.Visible = false;
            if (treeView1.SelectedNode.Tag is DrawCoordinate point)
            {
                if (point.DrawType == DrawCoordinateType.Line)
                {
                    duplicatePointToolStripMenuItem.Visible = true;
                    deletePointToolStripMenuItem.Visible = point.DrawShape.Points.Count > 2;
                }
                else if (point.DrawType == DrawCoordinateType.BezierCurve && point.DrawShape.Points.Count > 8)
                {
                    duplicatePointToolStripMenuItem.Visible = false;
                    deletePointToolStripMenuItem.Visible = point.DrawShape.Points.Count > 2;
                }
                else
                {
                    e.Cancel = true;
                }
            }
            else if (treeView1.SelectedNode.Tag is DrawShape shape)
            {
                deleteShapeToolStripMenuItem.Visible = true;
                setLayerToolStripMenuItem.Visible = true;
                useShapeForEraseToolStripMenuItem.Visible = !shape.IsEraser;
                useShapeForDrawToolStripMenuItem.Visible = shape.IsEraser;
            }
            else if (treeView1.SelectedNode.Tag is int layer) // layer
            {
                setColorToolStripMenuItem.Visible = true;
                var c = _drawShapes.First(p => p.Layer == layer).ForeColor;
                if (c.A > 0)
                {
                    setColorToolStripMenuItem.Text = $"Set color... (#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2})";
                }
                else
                {
                    setColorToolStripMenuItem.Text = "Set color...";
                }

                changeLayerToolStripMenuItem.Visible = true;
                deleteLayerToolStripMenuItem.Visible = true;
                hideLayerToolStripMenuItem.Visible = !_drawShapes.First(p => p.Layer == layer).Hidden;
                showLayerToolStripMenuItem.Visible = _drawShapes.First(p => p.Layer == layer).Hidden;
                setOutlineColorToolStripMenuItem.Visible = true;
                setOutlineWidthToolStripMenuItem.Visible = true;
                c = _drawShapes.First(p => p.Layer == layer).OutlineColor;
                if (c.A > 0)
                {
                    setOutlineColorToolStripMenuItem.Text = $"Set outline color... (#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2})";
                }
                else
                {
                    setOutlineColorToolStripMenuItem.Text = "Set outline color...";
                }

                setOutlineWidthToolStripMenuItem.Text = $"Outline witdh ({_drawShapes.First(p => p.Layer == layer).OutlineWidth})";
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void deleteShapeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_activeDrawShape != null && _drawShapes.Contains(_activeDrawShape))
            {
                _drawShapes.Remove(_activeDrawShape);
                TreeViewRemove(_activeDrawShape);
            }
            else if (_activeDrawShape != null && !_drawShapes.Contains(_activeDrawShape))
            {
                _x = int.MinValue;
                _y = int.MinValue;
                _activeDrawShape = null;
                pictureBoxCanvas.Invalidate();
                return;
            }
            else if (treeView1.SelectedNode?.Tag is DrawShape drawShape)
            {
                _drawShapes.Remove(drawShape);
                TreeViewRemove(drawShape);
            }

            if (_drawShapes.Count == 0)
            {
                treeView1.Nodes.Clear();
            }

            _activePoint = null;
            _activeDrawShape = null;
            pictureBoxCanvas.Invalidate();
            _x = int.MinValue;
            _y = int.MinValue;
            EnableDisableCurrentShapeActions();
        }

        private void SelectTreeViewNodePoint(DrawCoordinate point)
        {
            foreach (TreeNode node in treeView1.Nodes)
            {
                foreach (TreeNode subNode in node.Nodes)
                {
                    foreach (TreeNode subSubNode in subNode.Nodes)
                    {
                        if (subSubNode.Tag == point)
                        {
                            treeView1.SelectedNode = subSubNode;
                            return;
                        }
                    }
                }
            }
        }

        private void duplicatePointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is DrawCoordinate point)
            {
                if (point.DrawType == DrawCoordinateType.Line)
                {
                    var newPoint = new DrawCoordinate(point.DrawShape, DrawCoordinateType.Line, point.X, point.Y, point.PointColor);
                    var idx = point.DrawShape.Points.IndexOf(point) + 1;
                    point.DrawShape.Points.Insert(idx, newPoint);
                    duplicatePointToolStripMenuItem.Visible = true;
                    _activePoint = newPoint;

                    TreeViewFill(_drawShapes);
                    SelectTreeViewNodePoint(_activePoint);
                    pictureBoxCanvas.Invalidate();
                }
            }
        }

        private void toolStripButtonNew_Click(object sender, EventArgs e)
        {
            if (_drawShapes.Count == 0)
            {
                return;
            }

            var result = MessageBox.Show(this, "Clear all shapes?", "", MessageBoxButtons.YesNoCancel);
            if (result != DialogResult.Yes)
            {
                return;
            }

            ClosePreviewMode();
            ClearAll();
            _zoomFactor = 1;
            _fileName = null;
            ShowTitle();
            treeView1.Enabled = true;
        }

        private void toolStripButtonMirrorHor_Click(object sender, EventArgs e)
        {
            if (_activeDrawShape == null)
            {
                return;
            }

            var maxY = _activeDrawShape.Points.Max(p => p.Y);
            var newDrawing = new DrawShape { ForeColor = _activeDrawShape.ForeColor, Layer = _activeDrawShape.Layer };
            foreach (var p in _activeDrawShape.Points)
            {
                newDrawing.AddPoint(p.DrawType, p.X, maxY + maxY - p.Y, p.PointColor);
            }

            _drawShapes.Add(newDrawing);
            _activeDrawShape = newDrawing;
            TreeViewFill(_drawShapes);
            pictureBoxCanvas.Invalidate();
        }

        private void toolStripButtonMirrorVert_Click(object sender, EventArgs e)
        {
            if (_activeDrawShape == null)
            {
                return;
            }

            var maxX = _activeDrawShape.Points.Max(p => p.X);
            var newDrawing = new DrawShape { ForeColor = _activeDrawShape.ForeColor, Layer = _activeDrawShape.Layer };
            foreach (var p in _activeDrawShape.Points)
            {
                newDrawing.AddPoint(p.DrawType, maxX + maxX - p.X, p.Y, p.PointColor);
            }

            _drawShapes.Add(newDrawing);
            _activeDrawShape = newDrawing;
            TreeViewFill(_drawShapes);
            pictureBoxCanvas.Invalidate();
            _x = int.MinValue;
            _y = int.MinValue;
        }

        private void chooseBackgroundImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.FileName = string.Empty;
                openFileDialog.Filter = "Image files|*.png;*.jpg;*.bmp;*.tiff;*.jpe;*.jpeg;*.gif";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    SetBackgroundImage(openFileDialog.FileName);
                }
            }
        }

        private void clearBackgroundImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _backgroundImage?.Dispose();
            _backgroundImage = null;
            pictureBoxCanvas.Invalidate();
        }

        private void contextMenuStripCanvasBackground_Opening(object sender, CancelEventArgs e)
        {
            if (toolStripButtonPreview.Checked)
            {
                e.Cancel = true;
                return;
            }

            clearBackgroundImageToolStripMenuItem.Visible = _backgroundImage != null;
        }

        private void toolStripButtonSettings_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new FormAssaDrawSettings())
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    if (DrawSettings.HideSettingsAndTreeView && treeView1.Visible)
                    {
                        SuspendLayout();
                        HideSettingsAndTreeView();
                        ResumeLayout();
                    }
                    else if (!DrawSettings.HideSettingsAndTreeView && !treeView1.Visible)
                    {
                        SuspendLayout();
                        ShowSettingsAndTreeView();
                        ResumeLayout();
                    }

                    pictureBoxCanvas.Invalidate();
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            using (var helpForm = new FormAssaDrawHelp())
            {
                helpForm.ShowDialog();
                pictureBoxCanvas.Invalidate();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            AssaDrawCodes = GetAssaDrawCode();
            DialogResult = DialogResult.OK;
        }

        private void deletePointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is DrawCoordinate point)
            {
                if (point.DrawType == DrawCoordinateType.Line)
                {
                    _activePoint = null;
                    point.DrawShape.Points.Remove(point);
                    TreeViewFill(_drawShapes);
                    SelectTreeViewNodePoint(_activePoint);
                    pictureBoxCanvas.Invalidate();
                }
                else if (point.IsBeizer && point.DrawShape.Points.Count > 8)
                {
                    _activePoint = null;
                    var idx = point.DrawShape.Points.IndexOf(point);
                    if (idx < point.DrawShape.Points.Count - 2 && point.DrawShape.Points[idx + 1].DrawType == DrawCoordinateType.BezierCurveSupport1)
                    {
                        point.DrawShape.Points.RemoveAt(idx + 2);
                        point.DrawShape.Points.RemoveAt(idx + 1);
                        point.DrawShape.Points.RemoveAt(idx);
                    }
                    else if (idx > 2 && point.DrawShape.Points[idx + -2].DrawType == DrawCoordinateType.BezierCurveSupport1)
                    {
                        point.DrawShape.Points.RemoveAt(idx);
                        point.DrawShape.Points.RemoveAt(idx - 1);
                        point.DrawShape.Points.RemoveAt(idx - 2);
                    }

                    TreeViewFill(_drawShapes);
                    SelectTreeViewNodePoint(_activePoint);
                    pictureBoxCanvas.Invalidate();
                }
            }
        }

        private void numericUpDownWidth_ValueChanged(object sender, EventArgs e)
        {
            pictureBoxCanvas.Invalidate();
        }

        private void numericUpDownHeight_ValueChanged(object sender, EventArgs e)
        {
            pictureBoxCanvas.Invalidate();
        }

        private void toolStripButtonCopyToClipboard_Click(object sender, EventArgs e)
        {
            var sub = GetAssaDrawCode();
            if (sub.Paragraphs.Count == 1)
            {
                Clipboard.SetText(sub.Paragraphs[0].Text);
            }
            else if (sub.Paragraphs.Count > 1)
            {
                Clipboard.SetText(new AdvancedSubStationAlpha().ToText(sub, string.Empty));
            }

            pictureBoxCanvas.Focus();
        }

        private void toolStripButtonPreview_Click(object sender, EventArgs e)
        {
            if (GetAssaDrawCode().Paragraphs.Count == 0)
            {
                MessageBox.Show("Nothing to preview");
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                if (!toolStripButtonPreview.Checked)
                {
                    Bitmap backgroundImage = null;
                    if (!_backgroundOff && _backgroundImage != null)
                    {
                        backgroundImage = (Bitmap)_backgroundImage.Clone();
                    }
                    else if (numericUpDownWidth.Value > 0 && numericUpDownHeight.Value > 0)
                    {
                        backgroundImage = new Bitmap((int)numericUpDownWidth.Value, (int)numericUpDownHeight.Value);
                        var rectangleSize = 8;
                        var dark = false;
                        using (var g = Graphics.FromImage(backgroundImage))
                        {

                            for (int y = 0; y < pictureBoxCanvas.Height; y += rectangleSize)
                            {
                                for (int x = 0; x < pictureBoxCanvas.Width; x += rectangleSize)
                                {
                                    var c = dark ? Color.Black : Color.WhiteSmoke;
                                    if (y % (rectangleSize * 2) == 0)
                                    {
                                        if (x % (rectangleSize * 2) == 0)
                                        {
                                            c = dark ? Color.DimGray : Color.LightGray;
                                        }
                                    }
                                    else
                                    {
                                        if (x % (rectangleSize * 2) != 0)
                                        {
                                            c = dark ? Color.DimGray : Color.LightGray;
                                        }
                                    }

                                    using (var brush = new SolidBrush(c))
                                    {
                                        g.FillRectangle(brush, x, y, rectangleSize, rectangleSize);
                                    }
                                }
                            }
                        }
                    }

                    var fileName = VideoPreviewGenerator.GetVideoPreviewFileName((int)numericUpDownWidth.Value, (int)numericUpDownHeight.Value, backgroundImage);
                    if (string.IsNullOrEmpty(fileName) || !LibMpvDynamic.IsInstalled)
                    {
                        return;
                    }

                    _mpv = new LibMpvDynamic();
                    _mpv.Initialize(pictureBoxCanvas, fileName, VideoStartLoaded, null);
                    toolStripButtonPreview.Checked = true;
                }
                else
                {
                    ClosePreviewMode();
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            ShowTitle();
            pictureBoxCanvas.Invalidate();
            EnableDisableCurrentShapeActions();
            treeView1.Enabled = !toolStripButtonPreview.Checked;
            numericUpDownWidth.Enabled = !toolStripButtonPreview.Checked;
            numericUpDownHeight.Enabled = !toolStripButtonPreview.Checked;
        }

        private void ClosePreviewMode()
        {
            _mpv?.HardDispose();
            _mpv = null;
            toolStripButtonPreview.Checked = false;
            EnableDisableCurrentShapeActions();
            treeView1.Enabled = false;
            numericUpDownWidth.Enabled = false;
            numericUpDownHeight.Enabled = false;
        }

        private void VideoStartLoaded(object sender, EventArgs e)
        {
            var subtitle = GetAssaDrawCode();
            var text = subtitle.ToText(new AdvancedSubStationAlpha());
            _mpvTextFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".ass");
            File.WriteAllText(_mpvTextFileName, text);
            _mpv.LoadSubtitle(_mpvTextFileName);
            _mpv.Pause();
            _mpv.CurrentPosition = 0.5;
        }

        private void setLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is DrawShape shape)
            {
                using (var form = new FormSetLayer(shape.Layer))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        var firstNewShape = _drawShapes.FirstOrDefault(p => p.Layer == shape.Layer);
                        shape.Layer = form.Layer;
                        if (firstNewShape != null)
                        {
                            shape.ForeColor = firstNewShape.ForeColor;
                        }

                        TreeViewFill(_drawShapes);
                    }
                }
            }
        }

        private void setColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is int layer)
            {
                var first = _drawShapes.First(p => p.Layer == layer);
                var c = first.ForeColor != Color.Transparent ? first.ForeColor : Configuration.LastColorPickerColor;
                using (var colorChooser = new ColorChooser { Color = c })
                {
                    if (colorChooser.ShowDialog() == DialogResult.OK)
                    {
                        foreach (var shape in _drawShapes.Where(p => p.Layer == layer))
                        {
                            shape.ForeColor = colorChooser.Color;
                        }

                        TreeViewFill(_drawShapes);
                    }
                }
            }
        }

        /// <summary>
        /// Change layer number for a layer including all shapes in that layer.
        /// </summary>
        private void changeLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is int layer)
            {
                using (var form = new FormSetLayer(layer))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        foreach (var shape in _drawShapes.Where(p => p.Layer == layer))
                        {
                            shape.Layer = form.Layer;
                        }

                        TreeViewFill(_drawShapes);
                    }
                }
            }
        }

        private void deleteLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is int layer)
            {
                var i = _drawShapes.Count - 1;
                while (i >= 0)
                {
                    if (_drawShapes[i].Layer == layer)
                    {
                        _drawShapes.RemoveAt(i);
                    }
                    i--;
                }

                pictureBoxCanvas.Invalidate();
                TreeViewFill(_drawShapes);
            }
        }

        private void useShapeForEraseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is DrawShape shape)
            {
                shape.IsEraser = true;
                TreeViewFill(_drawShapes);
            }
        }

        private void useShapeForDrawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is DrawShape shape)
            {
                shape.IsEraser = false;
                TreeViewFill(_drawShapes);
            }
        }

        private void hideLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is int layer)
            {
                foreach (var shape in _drawShapes.Where(p => p.Layer == layer))
                {
                    shape.Hidden = true;
                }

                if (_activeDrawShape?.Hidden == true)
                {
                    _activeDrawShape = null;
                }

                pictureBoxCanvas.Invalidate();
                TreeViewFill(_drawShapes);
            }
        }

        private void showLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is int layer)
            {
                foreach (var shape in _drawShapes.Where(p => p.Layer == layer))
                {
                    shape.Hidden = false;
                }

                if (_activeDrawShape?.Hidden == true)
                {
                    _activeDrawShape = null;
                }

                pictureBoxCanvas.Invalidate();
                TreeViewFill(_drawShapes);
            }
        }

        private void FormAssaDrawMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            DrawSettings.SaveSettings();
        }

        private void timerTreeViewUpdate_Tick(object sender, EventArgs e)
        {
            if (!_treeViewInvalidated)
            {
                return;
            }

            lock (_treeViewLock)
            {
                TreeViewFill(_drawShapes);
                _treeViewInvalidated = false;
            }
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (!(e.Node.Tag is DrawShape command))
            {
                return;
            }

            if (e.Action == TreeViewAction.Expand)
            {
                command.Expanded = true;
            }
            else if (e.Action == TreeViewAction.Collapse)
            {
                command.Expanded = false;
            }
        }

        private void treeView1_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            treeView1_AfterExpand(sender, e);
        }

        private void toolStripButtonColorPicker_Click(object sender, EventArgs e)
        {
            if (_backgroundImage != null)
            {
                _backgroundOff = false;
                toolStripButtonBeizer.Checked = false;
                toolStripButtonLine.Checked = false;
                toolStripButtonRectangle.Checked = false;
                toolStripButtonCircle.Checked = false;
                toolStripButtonColorPicker.Checked = true;

                _activePoint = null;
                numericUpDownX.Enabled = false;
                numericUpDownY.Enabled = false;
                _activeDrawShape = null;

                textBoxColorPicker.Visible = true;
                panelColorPicker.Visible = true;
                panelColorPickerChoice.Visible = true;
                buttonSetColor.Visible = true;
                buttonSetOutlineColor.Visible = true;
            }
            else
            {
                MessageBox.Show("No background image loaded!");
            }
        }

        private void pictureBoxCanvas_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        private void setOutlineColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is int layer)
            {
                var first = _drawShapes.First(p => p.Layer == layer);
                var c = first.OutlineColor.A > 0 ? first.OutlineColor : Configuration.LastColorPickerColor;
                using (var colorChooser = new ColorChooser { Color = c })
                {
                    if (colorChooser.ShowDialog() == DialogResult.OK)
                    {
                        foreach (var shape in _drawShapes.Where(p => p.Layer == layer))
                        {
                            shape.OutlineColor = colorChooser.Color;
                        }

                        TreeViewFill(_drawShapes);
                    }
                }
            }
        }

        private void toolStripMenuItemOutlineWidth(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is int layer)
            {
                foreach (var shape in _drawShapes.Where(p => p.Layer == layer))
                {
                    shape.OutlineWidth = int.Parse((sender as ToolStripMenuItem).Text);
                    if (shape.OutlineWidth > 0 && shape.OutlineColor.A == 0)
                    {
                        setOutlineColorToolStripMenuItem_Click(null, null);
                    }
                }

                TreeViewFill(_drawShapes);
            }
        }

        private void buttonSetColor_Click(object sender, EventArgs e)
        {
            using (var form = new SetColor(_drawShapes, panelColorPickerChoice.BackColor, "Set fore color"))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    foreach (var shape in _drawShapes.Where(p=>form.Layers.Contains(p.Layer)))
                    {
                        shape.ForeColor = form.Color;
                    }
                    TreeViewFill(_drawShapes);
                }
            }
        }

        private void buttonSetOutlineColor_Click(object sender, EventArgs e)
        {
            using (var form = new SetColor(_drawShapes, panelColorPickerChoice.BackColor, "Set outline color"))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    foreach (var shape in _drawShapes.Where(p => form.Layers.Contains(p.Layer)))
                    {
                        shape.OutlineColor = form.Color;
                    }
                    TreeViewFill(_drawShapes);
                }
            }
        }
    }
}
