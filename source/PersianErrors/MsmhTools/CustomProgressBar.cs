using MsmhTools;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;
/*
* Copyright MSasanMH, May 10, 2022.
*/

namespace CustomControls
{
    public class CustomProgressBar : ProgressBar
    {
        private Color mBorderColor = Color.Red;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Border Color")]
        public Color BorderColor
        {
            get { return mBorderColor; }
            set
            {
                if (mBorderColor != value)
                {
                    mBorderColor = value;
                    Invalidate();
                }
            }
        }

        private Color mChunksColor = Color.LightBlue;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Chunks Color")]
        public Color ChunksColor
        {
            get { return mChunksColor; }
            set
            {
                if (mChunksColor != value)
                {
                    mChunksColor = value;
                    Invalidate();
                }
            }
        }

        private static Color[] OriginalColors;

        private static Color BackColorDisabled;
        private static Color ForeColorDisabled;
        private static Color BorderColorDisabled;
        private static Color ChunksColorDisabled;

        private string mCustomText = string.Empty;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Custom Text")]
        public string CustomText
        {
            get { return mCustomText; }
            set
            {
                if (mCustomText != value)
                {
                    mCustomText = value;
                    Invalidate();
                }
            }
        }

        private Font mFont = DefaultFont;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Text Font")]
        public override Font Font
        {
            get { return mFont; }
            set
            {
                if (mFont != value)
                {
                    mFont = value;
                    Invalidate();
                }
            }
        }

        private DateTime? mStartTime = null;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Set Start Time Programmatically (Optional)")]
        public DateTime? StartTime
        {
            get { return mStartTime; }
            set
            {
                if (mStartTime != value)
                {
                    mStartTime = value;
                    Invalidate();
                }
            }
        }

        private static bool ApplicationIdle = false;
        private TimeSpan elapsedTime;
        private string elapsedTimeString;
        private int once = 0;
        private bool onceIV = true;

        public CustomProgressBar() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            Application.Idle += Application_Idle;
            HandleCreated += CustomProgressBar_HandleCreated;
            EnabledChanged += CustomProgressBar_EnabledChanged;
            RightToLeftChanged += CustomProgressBar_RightToLeftChanged;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            ApplicationIdle = true;
            if (Parent != null)
            {
                if (onceIV == true)
                {
                    Control topParent = Tools.GetTopParent(this);
                    topParent.Move -= TopParent_Move;
                    topParent.Move += TopParent_Move;
                    Parent.Move -= Parent_Move;
                    Parent.Move += Parent_Move;
                    Invalidate();
                    onceIV = false;
                }
            }
        }

        private void TopParent_Move(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void Parent_Move(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomProgressBar_HandleCreated(object sender, EventArgs e)
        {
            OriginalColors = new Color[] { BackColor, ForeColor, mBorderColor, mChunksColor };
            Invalidate();
        }

        private void CustomProgressBar_EnabledChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomProgressBar_RightToLeftChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Update Colors
            OriginalColors = new Color[] { BackColor, ForeColor, BorderColor, ChunksColor };

            if (ApplicationIdle == false)
                return;

            if (DesignMode)
            {
                BorderColor = mBorderColor;
                ChunksColor = mChunksColor;
            }
            else
            {
                if (OriginalColors == null)
                    return;

                if (Enabled)
                {
                    BackColor = OriginalColors[0];
                    ForeColor = OriginalColors[1];
                    BorderColor = OriginalColors[2];
                    ChunksColor = OriginalColors[3];
                }
                else
                {
                    // Disabled Colors
                    if (OriginalColors[0].DarkOrLight() == "Dark")
                        BackColorDisabled = OriginalColors[0].ChangeBrightness(0.3f);
                    else if (OriginalColors[0].DarkOrLight() == "Light")
                        BackColorDisabled = OriginalColors[0].ChangeBrightness(-0.3f);

                    if (OriginalColors[1].DarkOrLight() == "Dark")
                        ForeColorDisabled = OriginalColors[1].ChangeBrightness(0.2f);
                    else if (OriginalColors[1].DarkOrLight() == "Light")
                        ForeColorDisabled = OriginalColors[1].ChangeBrightness(-0.2f);

                    if (OriginalColors[2].DarkOrLight() == "Dark")
                        BorderColorDisabled = OriginalColors[2].ChangeBrightness(0.3f);
                    else if (OriginalColors[2].DarkOrLight() == "Light")
                        BorderColorDisabled = OriginalColors[2].ChangeBrightness(-0.3f);

                    if (OriginalColors[3].DarkOrLight() == "Dark")
                        ChunksColorDisabled = OriginalColors[3].ChangeBrightness(0.3f);
                    else if (OriginalColors[3].DarkOrLight() == "Light")
                        ChunksColorDisabled = OriginalColors[3].ChangeBrightness(-0.3f);
                }
            }

            Color backColor;
            Color foreColor;
            Color borderColor;
            Color chunksColor;
            Color chunksColorGradient;

            if (Enabled)
            {
                backColor = BackColor;
                foreColor = ForeColor;
                borderColor = BorderColor;
                chunksColor = ChunksColor;
            }
            else
            {
                backColor = BackColorDisabled;
                foreColor = ForeColorDisabled;
                borderColor = BorderColorDisabled;
                chunksColor = ChunksColorDisabled;
            }

            if (chunksColor.DarkOrLight() == "Dark")
                chunksColorGradient = chunksColor.ChangeBrightness(0.5f);
            else
                chunksColorGradient = chunksColor.ChangeBrightness(-0.5f);

            if (DesignMode || !DesignMode)
            {
                Rectangle rect = ClientRectangle;
                Graphics g = e.Graphics;
                // Draw horizontal bar (Background and Border) With Default System Color:
                //ProgressBarRenderer.DrawHorizontalBar(g, rect);

                // Draw horizontal bar (Background and Border) With Custom Color:
                // Fill Background
                using (SolidBrush bgBrush = new SolidBrush(backColor))
                    g.FillRectangle(bgBrush, rect);

                // Draw Border
                //Rectangle borderRect = new Rectangle(0, 0, rect.Width - 1, rect.Height - 1);
                //g.DrawRectangle(new Pen(borderColor, 1), borderRect);
                ControlPaint.DrawBorder(g, rect, borderColor, ButtonBorderStyle.Solid);

                // Padding
                if (Value > 0)
                {
                    // Draw Chunks By Default Color (Green):
                    //Rectangle clip = new(rect.X, rect.Y, (int)Math.Round((float)Value / Maximum * rect.Width), rect.Height);
                    //ProgressBarRenderer.DrawHorizontalChunks(g, clip);

                    // Draw Chunks By Custom Color:
                    // The Following Is The Width Of The Bar. This Will Vary With Each Value.
                    int fillWidth = Width * Value / (Maximum - Minimum);

                    // GDI+ Doesn't Like Rectangles 0px Wide or Height
                    if (fillWidth == 0)
                    {
                        // Draw Only Border And Exit
                        ControlPaint.DrawBorder(g, rect, borderColor, ButtonBorderStyle.Solid);
                        return;
                    }
                    // Rectangles For Upper And Lower Half Of Bar
                    Rectangle topRect = new Rectangle(0, 0, fillWidth, Height / 2);
                    Rectangle buttomRect = new Rectangle(0, Height / 2, fillWidth, Height / 2);

                    // Paint Upper Half
                    using (LinearGradientBrush gbUH = new LinearGradientBrush(new Point(0, 0), new Point(0, Height / 2), chunksColorGradient, chunksColor))
                        e.Graphics.FillRectangle(gbUH, topRect);

                    // Paint Lower Half
                    // (this.Height/2 - 1 Because There Would Be A Dark Line In The Middle Of The Bar)
                    using (LinearGradientBrush gbLH = new LinearGradientBrush(new Point(0, Height / 2 - 1), new Point(0, Height), chunksColor, chunksColorGradient))
                        e.Graphics.FillRectangle(gbLH, buttomRect);

                    // Paint Border
                    ControlPaint.DrawBorder(g, rect, borderColor, ButtonBorderStyle.Solid);
                }

                // Compute Percent
                int percent = (int)(Value / (double)Maximum * 100);
                string textPercent;
                if (Value > 0)
                    textPercent = percent.ToString() + '%';
                else
                {
                    // If Value Is Zero Don't Write Anything
                    textPercent = string.Empty;
                    if (!DesignMode)
                        CustomText = string.Empty;
                }

                // Brush For Writing CustomText And Persentage On Progressbar
                using (SolidBrush brush = new SolidBrush(foreColor))
                {
                    // Percent
                    SizeF lenPercent = g.MeasureString(textPercent, Font);
                    Point locationPercentCenter = new Point(Convert.ToInt32((Width / 2) - lenPercent.Width / 2), Convert.ToInt32((Height / 2) - lenPercent.Height / 2));
                    g.DrawString(textPercent, Font, brush, locationPercentCenter);

                    // Custom Text
                    if (!string.IsNullOrEmpty(CustomText))
                    {
                        SizeF lenCustomText = g.MeasureString(CustomText, Font);
                        if (RightToLeft == RightToLeft.No)
                        {
                            Point locationCustomTextLeft = new Point(5, Convert.ToInt32((Height / 2) - lenCustomText.Height / 2));
                            g.DrawString(CustomText, Font, brush, locationCustomTextLeft);
                        }
                        else
                        {
                            Point locationCustomTextRight = new Point(Convert.ToInt32(Width - lenCustomText.Width - 5), Convert.ToInt32((Height / 2) - lenCustomText.Height / 2));
                            g.DrawString(CustomText, Font, brush, locationCustomTextRight);
                        }
                    }

                    // Compute Elapsed Time
                    if (StartTime.HasValue == true)
                    {
                        if (percent == 0)
                        {
                            elapsedTime = new TimeSpan(0, 0, 0, 0, 0);
                            elapsedTimeString = string.Empty;
                        }
                        else if (1 < percent && percent < 100)
                        {
                            elapsedTime = (TimeSpan)(DateTime.Now - StartTime);
                            elapsedTimeString = string.Format(@"Time: {0:00}:{1:00}:{2:000}", elapsedTime.Minutes, elapsedTime.Seconds, Math.Round((double)elapsedTime.Milliseconds / 10));
                            once = 0;
                        }
                        else if (percent == 100)
                        {
                            if (once == 0)
                            {
                                elapsedTime = (TimeSpan)(DateTime.Now - StartTime);
                                elapsedTimeString = string.Format(@"Time: {0:00}:{1:00}:{2:000}", elapsedTime.Minutes, elapsedTime.Seconds, Math.Round((double)elapsedTime.Milliseconds / 10));
                                once++;
                            }
                        }

                        SizeF lenElapsedTime = g.MeasureString(elapsedTimeString, Font);
                        if (RightToLeft == RightToLeft.No)
                        {
                            Point locationElapsedTimeRight = new Point(Convert.ToInt32(Width - lenElapsedTime.Width - 5), Convert.ToInt32((Height / 2) - lenElapsedTime.Height / 2));
                            g.DrawString(elapsedTimeString, Font, brush, locationElapsedTimeRight);
                        }
                        else
                        {
                            Point locationElapsedTimeLeft = new Point(5, Convert.ToInt32((Height / 2) - lenElapsedTime.Height / 2));
                            g.DrawString(elapsedTimeString, Font, brush, locationElapsedTimeLeft);
                        }
                    }
                }

            }
        }
    }
}
