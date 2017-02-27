using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class TimeCode
    {
        public const double BaseUnit = 1000.00;

        private double _totalMilliseconds = 0d;

        public TimeCode()
        {
        }

        public TimeCode(double totalMilliseconds) :
            this(0, 0, 0, (int)Math.Round(totalMilliseconds))
        {
        }

        public TimeCode(int hours, int minutes, int seconds, int milliseconds)
        {
            _totalMilliseconds = hours * 60 * 60 * BaseUnit + minutes * 60 * BaseUnit + seconds * BaseUnit + milliseconds;
        }

        public double TotalMilliseconds
        {
            get { return _time.TotalMilliseconds; }
            set { _time = TimeSpan.FromMilliseconds(value); }
        }

        public override string ToString()
        {
            string s = string.Format("{0:00}:{1:00}:{2:00},{3:000}", _time.Hours, _time.Minutes, _time.Seconds, _time.Milliseconds);

            if (TotalMilliseconds >= 0)
                return s;
            else
                return "-" + s.Replace("-", string.Empty);
        }

    }
}