using System;

namespace Nikse.SubtitleEdit.PluginLogic.Logic
{
    internal class TimeCode
    {
        private TimeSpan _time;

        public TimeCode(TimeSpan timeSpan)
        {
            _time = timeSpan;
        }

        public TimeCode(int hours, int minutes, int seconds, int milliseconds)
        {
            _time = new TimeSpan(0, hours, minutes, seconds, milliseconds);
        }

        public int Hours
        {
            get { return _time.Hours; }
            set { _time = new TimeSpan(0, value, _time.Minutes, _time.Seconds, _time.Milliseconds); }
        }

        public int Milliseconds
        {
            get { return _time.Milliseconds; }
            set { _time = new TimeSpan(0, _time.Hours, _time.Minutes, _time.Seconds, value); }
        }

        public int Minutes
        {
            get { return _time.Minutes; }
            set { _time = new TimeSpan(0, _time.Hours, value, _time.Seconds, _time.Milliseconds); }
        }

        public int Seconds
        {
            get { return _time.Seconds; }
            set { _time = new TimeSpan(0, _time.Hours, _time.Minutes, value, _time.Milliseconds); }
        }

        public TimeSpan TimeSpan
        {
            get { return _time; }
            set { _time = value; }
        }

        public double TotalMilliseconds
        {
            get { return _time.TotalMilliseconds; }
            set { _time = TimeSpan.FromMilliseconds(value); }
        }

        public double TotalSeconds
        {
            get { return _time.TotalSeconds; }
            set { _time = TimeSpan.FromSeconds(value); }
        }

        public override string ToString()
        {
            if (_time.Ticks < 0)
            {
                var time = _time.Negate();
                return string.Format("-{0:00}:{1:00}:{2:00},{3:000}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
            }
            return string.Format("{0:00}:{1:00}:{2:00},{3:000}", _time.Hours, _time.Minutes, _time.Seconds, _time.Milliseconds);
        }
    }
}
