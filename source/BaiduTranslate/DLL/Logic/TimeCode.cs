using System;

namespace BaiduTranslate.Logic
{
    public class TimeCode
    {
        private TimeSpan _time;

        internal TimeCode(TimeSpan timeSpan)
        {
            TimeSpan = timeSpan;
        }

        internal TimeCode(int hour, int minute, int seconds, int milliseconds)
        {
            _time = new TimeSpan(0, hour, minute, seconds, milliseconds);
        }

        internal int Hours
        {
            get { return _time.Hours; }
            set { _time = new TimeSpan(0, value, _time.Minutes, _time.Seconds, _time.Milliseconds); }
        }

        internal int Milliseconds
        {
            get { return _time.Milliseconds; }
            set { _time = new TimeSpan(0, _time.Hours, _time.Minutes, _time.Seconds, value); }
        }

        internal int Minutes
        {
            get { return _time.Minutes; }
            set { _time = new TimeSpan(0, _time.Hours, value, _time.Seconds, _time.Milliseconds); }
        }

        internal int Seconds
        {
            get { return _time.Seconds; }
            set { _time = new TimeSpan(0, _time.Hours, _time.Minutes, value, _time.Milliseconds); }
        }

        internal TimeSpan TimeSpan
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

        internal double TotalMilliseconds
        {
            get { return _time.TotalMilliseconds; }
            set { _time = TimeSpan.FromMilliseconds(value); }
        }

        internal double TotalSeconds
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

        internal void AddTime(double milliseconds)
        {
            _time = TimeSpan.FromMilliseconds(_time.TotalMilliseconds + milliseconds);
        }
    }
}