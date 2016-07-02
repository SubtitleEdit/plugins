using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class TimeCode
    {
        private TimeSpan _time;

        public TimeCode(double totalMilliseconds)
        {
            _time = TimeSpan.FromMilliseconds(totalMilliseconds);
        }

        public TimeCode(TimeSpan timeSpan)
        {
            TimeSpan = timeSpan;
        }

        public TimeCode(int hour, int minute, int seconds, int milliseconds)
        {
            _time = new TimeSpan(0, hour, minute, seconds, milliseconds);
        }

        public TimeSpan TimeSpan
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
            }
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