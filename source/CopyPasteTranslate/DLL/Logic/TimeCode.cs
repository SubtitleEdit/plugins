﻿using System;
using System.Globalization;

namespace WebViewTranslate.Logic
{
    public class TimeCode
    {
        private static readonly char[] TimeSplitChars = { ':', ',', '.' };
        public const double BaseUnit = 1000.0; // Base unit of time
        private double _totalMilliseconds;

        public bool IsMaxTime => Math.Abs(_totalMilliseconds - MaxTimeTotalMilliseconds) < 0.01;
        public const double MaxTimeTotalMilliseconds = 359999999; // new TimeCode(99, 59, 59, 999).TotalMilliseconds

        public static TimeCode FromSeconds(double seconds)
        {
            return new TimeCode(seconds * BaseUnit);
        }

        public static double ParseToMilliseconds(string text)
        {
            var parts = text.Split(TimeSplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4)
            {
                if (int.TryParse(parts[0], out var hours) && int.TryParse(parts[1], out var minutes) && int.TryParse(parts[2], out var seconds) && int.TryParse(parts[3], out var milliseconds))
                {
                    return new TimeSpan(0, hours, minutes, seconds, milliseconds).TotalMilliseconds;
                }
            }
            return 0;
        }

        public TimeCode()
        {
        }

        public TimeCode(TimeSpan timeSpan)
        {
            _totalMilliseconds = timeSpan.TotalMilliseconds;
        }

        public TimeCode(double totalMilliseconds)
        {
            _totalMilliseconds = totalMilliseconds;
        }

        public TimeCode(int hours, int minutes, int seconds, int milliseconds)
        {
            _totalMilliseconds = hours * 60 * 60 * BaseUnit + minutes * 60 * BaseUnit + seconds * BaseUnit + milliseconds;
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
                _totalMilliseconds = new TimeSpan(ts.Days, value, ts.Minutes, ts.Seconds, ts.Milliseconds).TotalMilliseconds;
            }
        }

        public int Minutes
        {
            get => TimeSpan.Minutes;
            set
            {
                var ts = TimeSpan;
                _totalMilliseconds = new TimeSpan(ts.Days, ts.Hours, value, ts.Seconds, ts.Milliseconds).TotalMilliseconds;
            }
        }

        public int Seconds
        {
            get => TimeSpan.Seconds;
            set
            {
                var ts = TimeSpan;
                _totalMilliseconds = new TimeSpan(ts.Days, ts.Hours, ts.Minutes, value, ts.Milliseconds).TotalMilliseconds;
            }
        }

        public int Milliseconds
        {
            get => TimeSpan.Milliseconds;
            set
            {
                var ts = TimeSpan;
                _totalMilliseconds = new TimeSpan(ts.Days, ts.Hours, ts.Minutes, ts.Seconds, value).TotalMilliseconds;
            }
        }

        public double TotalMilliseconds
        {
            get => _totalMilliseconds;
            set => _totalMilliseconds = value;
        }

        public double TotalSeconds
        {
            get => _totalMilliseconds / BaseUnit;
            set => _totalMilliseconds = value * BaseUnit;
        }

        public TimeSpan TimeSpan
        {
            get => TimeSpan.FromMilliseconds(_totalMilliseconds);
            set => _totalMilliseconds = value.TotalMilliseconds;
        }

        public override string ToString() => ToString(false);

        public string ToString(bool localize)
        {
            var ts = TimeSpan;
            string decimalSeparator = localize ? CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator : ",";
            string s = $"{ts.Hours + ts.Days * 24:00}:{ts.Minutes:00}:{ts.Seconds:00}{decimalSeparator}{ts.Milliseconds:000}";

            return PrefixSign(s);
        }

        public string ToShortString(bool localize = false)
        {
            var ts = TimeSpan;
            string decimalSeparator = localize ? CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator : ",";
            string s;
            if (ts.Minutes == 0 && ts.Hours == 0 && ts.Days == 0)
            {
                s = $"{ts.Seconds:0}{decimalSeparator}{ts.Milliseconds:000}";
            }
            else if (ts.Hours == 0 && ts.Days == 0)
            {
                s = $"{ts.Minutes:0}:{ts.Seconds:00}{decimalSeparator}{ts.Milliseconds:000}";
            }
            else
            {
                s = $"{ts.Hours + ts.Days * 24:0}:{ts.Minutes:00}:{ts.Seconds:00}{decimalSeparator}{ts.Milliseconds:000}";
            }
            return PrefixSign(s);
        }

       private string PrefixSign(string time) => TotalMilliseconds >= 0 ? time : $"-{time.RemoveChar('-')}";

        public string ToDisplayString()
        {
            if (IsMaxTime)
            {
                return "-";
            }

            return ToString(true);
        }

        public string ToShortDisplayString()
        {
            if (IsMaxTime)
            {
                return "-";
            }

            return ToShortString(true);
        }
    }
}
