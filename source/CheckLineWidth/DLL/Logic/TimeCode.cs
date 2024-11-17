using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class TimeCode
    {
        private TimeSpan _time;

        public TimeCode(TimeSpan timeSpan)
        {
            TimeSpan = timeSpan;
        }

        public TimeCode(int hour, int minute, int seconds, int milliseconds)
        {
            _time = new TimeSpan(0, hour, minute, seconds, milliseconds);
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

        public double TotalSeconds
        {
            get { return _time.TotalSeconds; }
            set { _time = TimeSpan.FromSeconds(value); }
        }


        public override string ToString()
        {
            string s = string.Format("{0:00}:{1:00}:{2:00},{3:000}", _time.Hours, _time.Minutes, _time.Seconds, _time.Milliseconds);

            if (TotalMilliseconds >= 0)
                return s;
            else
                return "-" + s.Replace("-", string.Empty);
        }

        public void AddTime(int hour, int minutes, int seconds, int milliseconds)
        {
            Hours += hour;
            Minutes += minutes;
            Seconds += seconds;
            Milliseconds += milliseconds;
        }

        public void AddTime(long milliseconds)
        {
            _time = TimeSpan.FromMilliseconds(_time.TotalMilliseconds + milliseconds);
        }

        public void AddTime(TimeSpan timeSpan)
        {
            _time = TimeSpan.FromMilliseconds(_time.TotalMilliseconds + timeSpan.TotalMilliseconds);
        }

        public void AddTime(double milliseconds)
        {
            _time = TimeSpan.FromMilliseconds(_time.TotalMilliseconds + milliseconds);
        }

        public string ToShortString()
        {
            string s;
            if (_time.Minutes == 0 && _time.Hours == 0)
                s = string.Format("{0:0},{1:000}", _time.Seconds, _time.Milliseconds);
            else if (_time.Hours == 0)
                s = string.Format("{0:0}:{1:00},{2:000}", _time.Minutes, _time.Seconds, _time.Milliseconds);
            else
                s = string.Format("{0:0}:{1:00}:{2:00},{3:000}", _time.Hours, _time.Minutes, _time.Seconds, _time.Milliseconds);

            if (TotalMilliseconds >= 0)
                return s;
            else
                return "-" + s.Replace("-", string.Empty);
        }
    }
}