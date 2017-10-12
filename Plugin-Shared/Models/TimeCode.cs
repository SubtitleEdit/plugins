using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class TimeCode
    {
        public static readonly TimeCode MaxTime = new TimeCode(99, 59, 59, 999);

        public const double BaseUnit = 1000.0; // Base unit of time
        private double _totalMilliseconds;

        public bool IsMaxTime => Math.Abs(_totalMilliseconds - MaxTime.TotalMilliseconds) < 0.01;

        public TimeCode(double totalMilliseconds = 0.0D)
        {
            _totalMilliseconds = totalMilliseconds;
        }

        public TimeCode(int hour, int minute, int seconds, int milliseconds)
        {
            _totalMilliseconds = hour * 60 * 60 * BaseUnit + minute * 60 * BaseUnit + seconds * BaseUnit + milliseconds;
        }

        public int Hours
        {
            get
            {
                var ts = TimeSpan;
                return ts.Hours + ts.Days * 24;
            }
            set
            {
                var ts = TimeSpan;
                _totalMilliseconds = new TimeSpan(0, value, ts.Minutes, ts.Seconds, ts.Milliseconds).TotalMilliseconds;
            }
        }

        public int Minutes
        {
            get
            {
                return TimeSpan.Minutes;
            }
            set
            {
                var ts = TimeSpan;
                _totalMilliseconds = new TimeSpan(0, ts.Hours, value, ts.Seconds, ts.Milliseconds).TotalMilliseconds;
            }
        }

        public int Seconds
        {
            get
            {
                return TimeSpan.Seconds;
            }
            set
            {
                var ts = TimeSpan;
                _totalMilliseconds = new TimeSpan(0, ts.Hours, ts.Minutes, value, ts.Milliseconds).TotalMilliseconds;
            }
        }

        public int Milliseconds
        {
            get
            {
                return TimeSpan.Milliseconds;
            }
            set
            {
                var ts = TimeSpan;
                _totalMilliseconds = new TimeSpan(0, ts.Hours, ts.Minutes, ts.Seconds, value).TotalMilliseconds;
            }
        }

        public double TotalMilliseconds
        {
            get { return _totalMilliseconds; }
            set { _totalMilliseconds = value; }
        }

        public double TotalSeconds
        {
            get { return _totalMilliseconds / BaseUnit; }
            set { _totalMilliseconds = value * BaseUnit; }
        }

        public TimeSpan TimeSpan
        {
            get
            {
                return TimeSpan.FromMilliseconds(_totalMilliseconds);
            }
            set
            {
                _totalMilliseconds = value.TotalMilliseconds;
            }
        }

        public override string ToString()
        {
            var ts = TimeSpan;
            var s = $"{(ts.Hours + ts.Days * 24):00}:{ts.Minutes:00}:{ts.Seconds:00},{ts.Milliseconds:000}";

            if (TotalMilliseconds >= 0)
                return s;
            return "-" + s.Replace("-", string.Empty);
        }

    }
}